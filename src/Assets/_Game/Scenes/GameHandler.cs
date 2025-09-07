using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jákup_Viljam;
using Jákup_Viljam.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameHandler : MonoBehaviour
{
    [Header("Music Prefabs")]
    [SerializeField] private GameObject _barSeperator;
    [SerializeField] private GameObject _lineSegment;
    [SerializeField] private GameObject _lineSegmentUp;
    [SerializeField] private GameObject _lineSegmentDown;
    [SerializeField] private GameObject _point;
    [SerializeField] private AudioSource _audioNormal;
    [SerializeField] private AudioSource _audioDistorted;
    [SerializeField] private AudioSource _errorAudioSource;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LineRenderer _scrubber;
    [SerializeField] private float _halfCycleDuration = 2f;
    [SerializeField] public BeatDriver beatDriver;

    [SerializeField] public AudioClip correcting;
    [SerializeField] public AudioClip missplacing;
    [SerializeField] public HighScoreHandler highScoreHandler;
    [SerializeField] public GameTimer gameTimer;
    [SerializeField] public MenuHandler menuHandler;
    [SerializeField] public PlayerController playerController;

    // Player
    [SerializeField] private GameObject _player;
    private MusicNode _playerInitialNode;
    private MusicNode _playerCurrentNode;

    public int CurrentBar = 0;
    public int CurrentBeat = 0;

    private Core.Loggers.ILogger _logger;
    private GameObject _levelParent;
    private MusicGraph _musicGraph;
    private Vector3 _initialPosition;

    private readonly int _barCount = 16;
    private readonly int _beatsPerBar = 8;
    private readonly int _lines = 5;

    public float unitsPerBeat = 1f;

    private int _lastTickRealign = 0;

    private readonly int perFrame = 20;

    private double _currentTickStartDsp = 0;

    private bool _wasDownPressed = false;
    private bool _wasUpPressed = false;
    private bool _isMoving = false;

    /* ----------------------------------------------------------------------------  */
    /*                                   Lifecycle                                   */
    /* ----------------------------------------------------------------------------  */
    private void OnEnable()
    {
        beatDriver.OnTick += HandleTick;
    }

    private void OnDisable()
    {
        beatDriver.OnTick -= HandleTick;
    }

    private IEnumerator Start()
    {
        // optional: hide world until ready
        QualitySettings.vSyncCount = 1; // keep smooth if you changed it elsewhere
        Application.targetFrameRate = 60;

        _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);
        _musicGraph = GenerateStaticMusicGraph();

        highScoreHandler.SetInitialErrors(_musicGraph.AllNodes.Count(o => o.Type == NodeType.Tangled));

        _playerCurrentNode = _musicGraph.GetNode(0, 0, 0);
        _playerInitialNode = _playerCurrentNode;

        _initialPosition = transform.position;

        int i = 0;
        foreach (object item in RenderGraph())
        {
            if ((i + 1) % perFrame == 0)
            {
                yield return null; // give a frame back to the engine
            }

            i++;
        }

        DrawPlayer();

        yield return new WaitForSeconds(0.5f);

        beatDriver.SetDistoredSound(_audioDistorted);
        beatDriver.StartPlaying();
        gameTimer.StartTimer();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isPause)
        {
            return;
        }

        HandlerPlayerInputs();
        HandScrubberMovement();
    }

    /* ----------------------------------------------------------------------------  */
    /*                                PRIVATE METHODS                                */
    /* ----------------------------------------------------------------------------  */
    private bool isPause = false;

    public void Pause()
    {
        isPause = true;
        beatDriver.Pause();
    }

    public void Resume()
    {
        isPause = false;
        beatDriver.Resume();
    }

    private void HandleTick()
    {
        playerController.Tick();
        HandlePlayerMovement();
    }

    private void HandScrubberMovement()
    {

        if (beatDriver.CurrentTick != _lastTickRealign)
        {
            _lastTickRealign = beatDriver.CurrentTick;
            // Establish tick start DSP time (previous tick end)
            _currentTickStartDsp = beatDriver.NextTickDsp - beatDriver.SPB;

            transform.position = new Vector3(_initialPosition.x + 0.45f - (beatDriver.CurrentTick * 0.5f), _initialPosition.y, 0);
        }
        else
        {

            // Interpolate between current tick position and next tick position based on elapsed fraction.
            double now = AudioSettings.dspTime;

            // Guard: if music not started yet.
            if (beatDriver.NextTickDsp <= 0 || now < _currentTickStartDsp)
            {
                return;
            }

            double elapsed = now - _currentTickStartDsp;
            double t = elapsed / beatDriver.SPB;
            if (t < 0)
            {
                t = 0;
            }

            if (t > 1)
            {
                t = 1;
            }

            var currentBasePos = new Vector3(
                _initialPosition.x + 0.45f - (beatDriver.CurrentTick * 0.5f),
                _initialPosition.y,
                0);

            var nextTickPos = new Vector3(
                _initialPosition.x + 0.45f - ((beatDriver.CurrentTick + 1) * 0.5f),
                _initialPosition.y,
                0);

            // Linear interpolation for constant speed. Replace with SmoothStep if you want easing.
            transform.position = Vector3.Lerp(currentBasePos, nextTickPos, (float)t);
        }
    }

    private void HandlerPlayerInputs()
    {
        if (_isMoving == true)
        {
            return;
        }

        bool isUpPressed = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
        bool isDownPressed = Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;

        if (_wasUpPressed == false && isUpPressed)
        {
            _wasUpPressed = true;
            _wasDownPressed = false;
        }
        else if (_wasDownPressed == false && isDownPressed)
        {
            _wasUpPressed = false;
            _wasDownPressed = true;
        }
    }

    private void HandlePlayerMovement()
    {
        if (_isMoving == true)
        {
            return;
        }

        _isMoving = true;

        // Determine next node
        MusicNode nextNode;
        if (_wasDownPressed && _playerCurrentNode?.DownNode != null)
        {
            nextNode = _playerCurrentNode.DownNode;
        }
        else if (_wasUpPressed && _playerCurrentNode?.UpNode != null)
        {
            nextNode = _playerCurrentNode.UpNode;
        }
        else
        {
            nextNode = _playerCurrentNode.RightNode;
        }

        if (nextNode == null)
        {
            //Finished
            Pause();
            menuHandler.ShowFinishedMenu();
            return;
        }

        // Update model
        _playerCurrentNode = nextNode;

        // Update view
        DrawPlayer();

        _wasUpPressed = false;
        _wasDownPressed = false;
        _isMoving = false;

        // HANDLE RESULT OF MOVE!!!
        if (_playerCurrentNode.Type == NodeType.Tangled)
        {
            _playerCurrentNode.Type = NodeType.Nothing;
            RenderNode(_playerCurrentNode);

            PlayCorrectingSound();

            highScoreHandler.AddScore();
        }
        if (_playerCurrentNode.Type == NodeType.Untangled)
        {
            _playerCurrentNode.Type = NodeType.Tangled;

            Array values = Enum.GetValues(typeof(LineType));
            int index = UnityEngine.Random.Range(1, values.Length);
            _playerCurrentNode.LineType = (LineType)values.GetValue(index);

            RenderNode(_playerCurrentNode);

            PlayMissplacingSound();

            highScoreHandler.RemoveScore();
        }
    }

    private void PlayCorrectingSound()
    {
        _errorAudioSource.Stop();
        _errorAudioSource.clip = correcting;
        _errorAudioSource.Play();
    }

    private void PlayMissplacingSound()
    {
        _errorAudioSource.Stop();
        _errorAudioSource.clip = missplacing;
        _errorAudioSource.Play();
    }

    private void DrawPlayer()
    {
        if (_playerCurrentNode == null)
        {
            return;
        }

        _player.transform.position = new Vector3(_player.transform.position.x, _initialPosition.y - (0.5f * _playerCurrentNode.Line), 0);
    }

    private IEnumerable RenderGraph()
    {
        _levelParent = GameObject.Instantiate(new GameObject(), this.transform);

        IEnumerable<MusicNode> allNodes = _musicGraph.AllNodes;
        IEnumerable<IGrouping<int, MusicNode>> bars = allNodes.GroupBy(o => o.Bar);

        for (int barNumber = 0; barNumber < _barCount; barNumber++)
        {
            Vector3 position = new(-0.5f + transform.position.x + (4 * barNumber), transform.position.y - 1f, -2);
            var barSeperator = GameObject.Instantiate(_barSeperator, position, Quaternion.identity);
            barSeperator.transform.parent = _levelParent.transform;

            // Draw Bar Separator

            for (int beatNumber = 0; beatNumber < _beatsPerBar; beatNumber++)
            {
                for (int lineNumber = 0; lineNumber < _lines; lineNumber++)
                {
                    MusicNode node = _musicGraph.GetNode(barNumber, beatNumber, lineNumber);
                    RenderNode(node);

                    yield return null;
                }
            }
        }
    }

    private void RenderNode(MusicNode node)
    {
        var vector = new Vector3(transform.position.x + (4f * node.Bar) + (0.5f * node.Beat), transform.position.y + (-0.5f * node.Line), transform.position.z);

        if (node.GameObjects.Count > 0)
        {
            foreach (GameObject go in node.GameObjects)
            {
                GameObject.Destroy(go);
            }
            node.GameObjects.Clear();
        }

        if (node.Beat % 2 == 0)
        {
            GameObject gameObjectToSpawn;
            if (node.Type == NodeType.Tangled)
            {
                if (node.LineType == LineType.AboveLine)
                {
                    gameObjectToSpawn = _lineSegmentUp;
                }
                else
                {
                    gameObjectToSpawn = _lineSegmentDown;
                }
            }
            else
            {
                gameObjectToSpawn = _lineSegment;
            }

            var segment = GameObject.Instantiate(gameObjectToSpawn, vector, Quaternion.identity);
            node.GameObjects.Add(segment);
            segment.transform.parent = _levelParent.transform;
        }

        // Draw Line Segment
        if (NodeType.Untangled == node.Type)
        {
            var point = GameObject.Instantiate(_point, new Vector3(vector.x, vector.y, -2), Quaternion.identity);
            point.transform.parent = _levelParent.transform;
            node.GameObjects.Add(point);
        }
        else
        {

        }
    }

    private MusicGraph GenerateStaticMusicGraph()
    {
        GraphStructure structure = new()
        {
            Lines = _lines,
            BeatsPerBar = _beatsPerBar,
            Bars = _barCount,
            SpecialNodes = new List<MusicNode>
                {
                    //0-3
                    new(0, 0, 2, NodeType.Untangled),
                    new(0, 0, 3, NodeType.Untangled),
                    new(0, 0, 4, NodeType.Untangled),
                    new(0, 4, 2, NodeType.Tangled),
                    new(0, 2, 0, NodeType.Untangled),
                    new(0, 4, 0, NodeType.Untangled),
                    new(0, 4, 1, NodeType.Untangled),
                    new(0, 6, 2, NodeType.Untangled),

                    new(1, 1, 0, NodeType.Untangled),
                    new(1, 0, 1, NodeType.Untangled),
                    new(1, 0, 2, NodeType.Untangled),
                    new(1, 2, 2, NodeType.Untangled),
                    new(1, 4, 0, NodeType.Untangled),
                    new(1, 4, 0, NodeType.Untangled),
                    new(1, 4, 3, NodeType.Tangled),
                    new(1, 7, 1, NodeType.Untangled),

                    new(2, 0, 0, NodeType.Point),
                    new(2, 0, 1, NodeType.Untangled),
                    new(2, 0, 2, NodeType.Untangled),
                    new(2, 0, 3, NodeType.Untangled),
                    new(2, 2, 0, NodeType.Untangled),
                    new(2, 4, 4, NodeType.Tangled),
                    new(2, 4, 1, NodeType.Untangled),
                    new(2, 6, 2, NodeType.Untangled),
                    new(2, 7, 2, NodeType.Point),

                    new(3, 0, 2, NodeType.Untangled),
                    new(3, 0, 1, NodeType.Tangled),
                    new(3, 0, 3, NodeType.Untangled),
                    new(3, 0, 4, NodeType.Untangled),
                    new(3, 4, 1, NodeType.Untangled),
                    new(3, 4, 2, NodeType.Untangled),
                    new(3, 6, 0, NodeType.Untangled),
                    new(3, 7, 0, NodeType.Untangled),


                    //4-7
                    new(4, 0, 2, NodeType.Untangled),
                    new(4, 0, 3, NodeType.Untangled),
                    new(4, 0, 4, NodeType.Untangled),
                    new(4, 4, 2, NodeType.Tangled),
                    new(4, 2, 0, NodeType.Untangled),
                    new(4, 4, 0, NodeType.Untangled),
                    new(4, 4, 1, NodeType.Untangled),
                    new(4, 6, 2, NodeType.Untangled),

                    new(5, 0, 0, NodeType.Untangled),
                    new(5, 0, 1, NodeType.Untangled),
                    new(5, 0, 2, NodeType.Untangled),
                    new(5, 2, 2, NodeType.Untangled),
                    new(5, 4, 0, NodeType.Untangled),
                    new(5, 4, 0, NodeType.Untangled),
                    new(5, 4, 3, NodeType.Tangled),
                    new(5, 7, 1, NodeType.Untangled),

                    new(6, 0, 0, NodeType.Point),
                    new(6, 0, 1, NodeType.Untangled),
                    new(6, 0, 2, NodeType.Untangled),
                    new(6, 0, 3, NodeType.Untangled),
                    new(6, 2, 0, NodeType.Untangled),
                    new(6, 4, 4, NodeType.Tangled),
                    new(6, 4, 1, NodeType.Untangled),
                    new(6, 6, 2, NodeType.Untangled),
                    new(6, 7, 2, NodeType.Point),

                    new(7, 0, 2, NodeType.Untangled),
                    new(7, 0, 1, NodeType.Tangled),
                    new(7, 0, 3, NodeType.Untangled),
                    new(7, 0, 4, NodeType.Untangled),
                    new(7, 5, 1, NodeType.Untangled),
                    new(7, 5, 2, NodeType.Untangled),
                    new(7, 6, 0, NodeType.Untangled),
                    new(7, 7, 0, NodeType.Point),

                    //8-11
                    new(8, 0, 0, NodeType.Untangled),
                    new(8, 0, 2, NodeType.Untangled),
                    new(8, 0, 3, NodeType.Untangled),
                    new(8, 0, 4, NodeType.Untangled),
                    new(8, 2, 4, NodeType.Untangled),
                    new(8, 3, 0, NodeType.Untangled),
                    new(8, 3, 4, NodeType.Untangled),
                    new(8, 5, 0, NodeType.Untangled),
                    new(8, 5, 1, NodeType.Untangled),
                    new(8, 6, 1, NodeType.Untangled),
                    new(8, 7, 0, NodeType.Untangled),
                    new(8, 7, 1, NodeType.Untangled),

                    new(9, 0, 0, NodeType.Untangled),
                    new(9, 0, 1, NodeType.Untangled),
                    new(9, 0, 2, NodeType.Untangled),
                    new(9, 1, 0, NodeType.Untangled),
                    new(9, 2, 2, NodeType.Untangled),
                    new(9, 3, 0, NodeType.Untangled),
                    new(9, 4, 2, NodeType.Untangled),
                    new(9, 5, 0, NodeType.Untangled),
                    new(9, 5, 3, NodeType.Untangled),
                    new(9, 6, 0, NodeType.Untangled),

                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),

                    new(11, 0, 2, NodeType.Untangled),
                    new(11, 0, 3, NodeType.Untangled),
                    new(11, 0, 4, NodeType.Untangled),
                    new(11, 1, 0, NodeType.Untangled),
                    new(11, 2, 4, NodeType.Untangled),
                    new(11, 3, 4, NodeType.Untangled),
                    new(11, 4, 0, NodeType.Untangled),
                    new(11, 4, 4, NodeType.Untangled),
                    new(11, 5, 0, NodeType.Untangled),
                    new(11, 6, 0, NodeType.Untangled),
                    new(11, 6, 1, NodeType.Untangled),
                    new(11, 6, 2, NodeType.Untangled),
                    new(11, 6, 3, NodeType.Untangled),

                    //12-15
                    new(12, 0, 0, NodeType.Untangled),
                    new(12, 0, 2, NodeType.Untangled),
                    new(12, 0, 3, NodeType.Untangled),
                    new(12, 0, 4, NodeType.Untangled),
                    new(12, 2, 4, NodeType.Untangled),
                    new(12, 3, 0, NodeType.Untangled),
                    new(12, 3, 4, NodeType.Untangled),
                    new(12, 5, 0, NodeType.Untangled),
                    new(12, 5, 1, NodeType.Untangled),
                    new(12, 6, 1, NodeType.Untangled),
                    new(12, 7, 0, NodeType.Untangled),
                    new(12, 7, 1, NodeType.Untangled),

                    new(13, 0, 0, NodeType.Untangled),
                    new(13, 0, 1, NodeType.Untangled),
                    new(13, 0, 2, NodeType.Untangled),
                    new(13, 1, 0, NodeType.Untangled),
                    new(13, 2, 2, NodeType.Untangled),
                    new(13, 3, 0, NodeType.Untangled),
                    new(13, 4, 2, NodeType.Untangled),
                    new(13, 5, 0, NodeType.Untangled),
                    new(13, 5, 3, NodeType.Untangled),
                    new(13, 6, 0, NodeType.Untangled),

                    new(14, 0, 1, NodeType.Untangled),
                    new(14, 0, 2, NodeType.Untangled),
                    new(14, 0, 3, NodeType.Untangled),
                    new(14, 0, 0, NodeType.Untangled),
                    new(14, 0, 1, NodeType.Untangled),
                    new(14, 0, 0, NodeType.Untangled),
                    new(14, 0, 1, NodeType.Untangled),
                    new(14, 0, 2, NodeType.Untangled),
                    new(14, 0, 0, NodeType.Untangled),
                    new(14, 0, 0, NodeType.Untangled),
                    new(14, 0, 3, NodeType.Untangled),

                    new(15, 0, 2, NodeType.Untangled),
                    new(15, 0, 3, NodeType.Untangled),
                    new(15, 0, 4, NodeType.Untangled),
                    new(15, 1, 0, NodeType.Untangled),
                    new(15, 2, 4, NodeType.Untangled),
                    new(15, 3, 4, NodeType.Untangled),
                    new(15, 4, 0, NodeType.Untangled),
                    new(15, 4, 4, NodeType.Untangled),
                    new(15, 5, 0, NodeType.Untangled),
                    new(15, 6, 0, NodeType.Untangled),
                    new(15, 6, 1, NodeType.Untangled),
                    new(15, 6, 2, NodeType.Untangled),
                    new(15, 6, 3, NodeType.Untangled),

                }
        };

        return GraphBuilder.BuildGraph(structure);
    }
}

