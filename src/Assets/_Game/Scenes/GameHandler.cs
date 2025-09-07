using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LineRenderer _scrubber;
    [SerializeField] private float _halfCycleDuration = 2f;
    [SerializeField] public BeatDriver beatDriver;

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

    private readonly float _perfectWindowThreshold = 50f;
    private readonly float _goodWindowThreshold = 100f;

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

    int perFrame = 20;
    IEnumerator Start()
    {
        // optional: hide world until ready
        QualitySettings.vSyncCount = 1; // keep smooth if you changed it elsewhere
        Application.targetFrameRate = 60;

        _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);
        _musicGraph = GenerateStaticMusicGraph();

        _playerCurrentNode = _musicGraph.GetNode(0, 0, 0);
        _playerInitialNode = _playerCurrentNode;

        _initialPosition = transform.position;

        var enumerable = RenderGraph();

        int i = 0;
        foreach (var item in RenderGraph())
        {
            if ((i + 1) % perFrame == 0)
                yield return null; // give a frame back to the engine

            i++;
        }
        
        DrawPlayer();

        yield return new WaitForSeconds(0.5f); 

        beatDriver.StartPlaying();
    }

    // Update is called once per frame
    private void Update()
    {
        HandScrubberMovement();
    }

    private void FixedUpdate()
    {
        HandlerPlayerControls();
    }

    /* ----------------------------------------------------------------------------  */
    /*                                PRIVATE METHODS                                */
    /* ----------------------------------------------------------------------------  */



    private void HandleTick()
    {
        Debug.Log("Tick ");
        HandlePlayerMovement();
    }


    private int _lastTickRealign = 0;
    private double _currentTickStartDsp = 0;

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
                return;

            double elapsed = now - _currentTickStartDsp;
            double t = elapsed / beatDriver.SPB;
            if (t < 0) t = 0;
            if (t > 1) t = 1;

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

            //double now = AudioSettings.dspTime;
            //double timeUntilNextTick = beatDriver.NextTickDsp - now;
            //var nextTickPosition = new Vector3(_initialPosition.x + 0.45f - ((beatDriver.CurrentTick + 1) * 0.5f), _initialPosition.y, 0);

            //transform.position = Vector3.Lerp(
            //    transform.position,
            //    nextTickPosition,
            //    (float)timeUntilNextTick
            //);
        }
    }

    private void HandlePlayerMovement()
    {
        if (_playerCurrentNode.DownNode != null)
        {
            _playerCurrentNode = _playerCurrentNode.DownNode;
        }
        else if (_playerCurrentNode.UpNode != null)
        {
            _playerCurrentNode = _playerCurrentNode.UpNode;
        }

        DrawPlayer();
    }

    private void DrawPlayer()
    {
        if (_playerCurrentNode == null)
        {
            return;
        }

        _player.transform.position = new Vector3(_player.transform.position.x, _initialPosition.y - (0.5f * _playerCurrentNode.Line), 0);
    }

    private void HandlerPlayerControls()
    {
        bool isUpPressed = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
        bool isDownPressed = Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;

        if (isUpPressed || isDownPressed)
        {
            double now = AudioSettings.dspTime;
            double diff = now - beatDriver.NextTickDsp;
            if (diff < 0)
            {
                diff = -diff;
            }

            bool shouldTryToMove = diff <= _goodWindowThreshold;

            if (shouldTryToMove)
            {
                // player index

                // check if movement direction is valid

                // move player

                // handle movement events
            }
        }
    }

    private IEnumerable RenderGraph()
    {
        Vector3 start = transform.position;

        _levelParent = GameObject.Instantiate(new GameObject(), this.transform);


        IEnumerable<MusicNode> allNodes = _musicGraph.AllNodes;
        IEnumerable<IGrouping<int, MusicNode>> bars = allNodes.GroupBy(o => o.Bar);

        for (int barNumber = 0; barNumber < _barCount; barNumber++)
        {
            Vector3 position = new(-0.5f + start.x + (4 * barNumber), start.y - 1f, start.z);
            var barSeperator = GameObject.Instantiate(_barSeperator, position, Quaternion.identity);
            barSeperator.transform.parent = _levelParent.transform;

            // Draw Bar Separator

            for (int beatNumber = 0; beatNumber < _beatsPerBar; beatNumber++)
            {
                for (int lineNumber = 0; lineNumber < _lines; lineNumber++)
                {
                    MusicNode node = _musicGraph.GetNode(barNumber, beatNumber, lineNumber);

                    var vector = new Vector3(start.x + (4f * barNumber) + (0.5f * beatNumber), start.y + (-0.5f * lineNumber), start.z);

                    if (beatNumber % 2 == 0)
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
                        segment.transform.parent = _levelParent.transform;
                    }

                    // Draw Line Segment
                    if (NodeType.Untangled == node.Type)
                    {
                        var point = GameObject.Instantiate(_point, vector, Quaternion.identity);
                        point.transform.parent = _levelParent.transform;
                    }
                    else
                    {

                    }

                    yield return null;


                    //var barSeperator2 = GameObject.Instantiate(_barSeperator, vector, Quaternion.identity);
                    //barSeperator2.transform.parent = _levelParent.transform;


                    // Draw Note or Gem
                }
            }
        }
    }

    private void RenderGraph2()
    {
        Vector3 start = transform.position;

        _levelParent = GameObject.Instantiate(new GameObject(), this.transform);


        IEnumerable<MusicNode> allNodes = _musicGraph.AllNodes;
        IEnumerable<IGrouping<int, MusicNode>> bars = allNodes.GroupBy(o => o.Bar);

        for (int barNumber = 0; barNumber < _barCount; barNumber++)
        {
            Vector3 position = new(-0.5f + start.x + (4 * barNumber), start.y - 1f, start.z);
            var barSeperator = GameObject.Instantiate(_barSeperator, position, Quaternion.identity);
            barSeperator.transform.parent = _levelParent.transform;

            // Draw Bar Separator

            for (int beatNumber = 0; beatNumber < _beatsPerBar; beatNumber++)
            {
                for (int lineNumber = 0; lineNumber < _lines; lineNumber++)
                {
                    MusicNode node = _musicGraph.GetNode(barNumber, beatNumber, lineNumber);

                    var vector = new Vector3(start.x + (4f * barNumber) + (0.5f * beatNumber), start.y + (-0.5f * lineNumber), start.z);

                    if (beatNumber % 2 == 0)
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
                        segment.transform.parent = _levelParent.transform;
                    }

                    // Draw Line Segment
                    if (NodeType.Untangled == node.Type)
                    {
                        var point = GameObject.Instantiate(_point, vector, Quaternion.identity);
                        point.transform.parent = _levelParent.transform;
                    }
                    else
                    {

                    }


                    //var barSeperator2 = GameObject.Instantiate(_barSeperator, vector, Quaternion.identity);
                    //barSeperator2.transform.parent = _levelParent.transform;


                    // Draw Note or Gem
                }
            }
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
                    new(0, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(0, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(0, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 2, NodeType.Tangled, LineType.AboveLine),
                    new(0, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(0, 6, 2, NodeType.Untangled, LineType.OnLine),

                    new(1, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(1, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(1, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 3, NodeType.Tangled, LineType.OnLine),
                    new(1, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(2, 0, 0, NodeType.Point, LineType.OnLine),
                    new(2, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(2, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(2, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(2, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(2, 4, 4, NodeType.Tangled, LineType.OnLine),
                    new(2, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(2, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(2, 7, 2, NodeType.Point, LineType.OnLine),

                    new(3, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(3, 0, 1, NodeType.Tangled, LineType.OnLine),
                    new(3, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(3, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(3, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(3, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(3, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(3, 7, 0, NodeType.Untangled, LineType.OnLine),


                    //4-7
                    new(4, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(4, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(4, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 2, NodeType.Tangled, LineType.OnLine),
                    new(4, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(4, 6, 2, NodeType.Untangled, LineType.OnLine),

                    new(5, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(5, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(5, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 3, NodeType.Tangled, LineType.OnLine),
                    new(5, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(6, 0, 0, NodeType.Point, LineType.OnLine),
                    new(6, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(6, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(6, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(6, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(6, 4, 4, NodeType.Tangled, LineType.OnLine),
                    new(6, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(6, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(6, 7, 2, NodeType.Point, LineType.OnLine),

                    new(7, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(7, 0, 1, NodeType.Tangled, LineType.OnLine),
                    new(7, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(7, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(7, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(7, 5, 2, NodeType.Untangled, LineType.OnLine),
                    new(7, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(7, 7, 0, NodeType.Point, LineType.OnLine),

                    //8-11
                    new(8, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(9, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 3, NodeType.Untangled, LineType.OnLine),
                    new(9, 6, 0, NodeType.Untangled, LineType.OnLine),

                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),

                    new(11, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 3, NodeType.Untangled, LineType.OnLine),

                    //12-15
                    new(8, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(9, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 3, NodeType.Untangled, LineType.OnLine),
                    new(9, 6, 0, NodeType.Untangled, LineType.OnLine),

                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),

                    new(11, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 3, NodeType.Untangled, LineType.OnLine),

                }
        };

        return GraphBuilder.BuildGraph(structure);
    }
}
