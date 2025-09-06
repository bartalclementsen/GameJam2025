using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jákup_Viljam;
using Jákup_Viljam.Models;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.RuleTile.TilingRuleOutput;

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

    // Player
    [SerializeField] private GameObject _player;
    private MusicNode _playerInitialNode;
    private MusicNode _playerCurrentNode;

    public int CurrentBar = 0;
    public int CurrentBeat = 0;

    private RhythmHandler2 _rhythmHandler;
    private Core.Loggers.ILogger _logger;
    private GameObject _levelParent;
    private MusicGraph _musicGraph;
    private Vector3 _initialPosition;
    private bool _isGameOver;

    private readonly int _barCount = 16;
    private readonly int _beatsPerBar = 8;
    private readonly int _lines = 5;

    private float _speed = 1.5f;

    private float _eighthNoteInterval;
    private float _nextEventTime;
    private bool _isPlaying = false;

    public float unitsPerBeat = 1f;
    private float _beatDuration;
    private double _nextBeatTime;
    private Vector3 _scrubberTargetPosition;
    private double _startTime;

    private void Start()
    {
        _startTime = AudioSettings.dspTime;

        _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);
        _musicGraph = GenerateStaticMusicGraph();

        _playerCurrentNode = _musicGraph.GetNode(0, 0, 0);
        _playerInitialNode = _playerCurrentNode;

        _initialPosition = transform.position;

        _rhythmHandler = new RhythmHandler2(_startTime);

        _audioNormal.Play();

        RenderGraph();
        DrawPlayer();
    }

    // Update is called once per frame
    private void Update()
    {
        HandScrubberMovement();
        HandleBeetChecker();


        if (!_audioNormal.isPlaying && _isPlaying)
        {
            _isPlaying = false;
        }
    }

    private void FixedUpdate()
    {
        HandlerPlayerControls();
    }

    private void AdvanceBeat()
    {
        CurrentBeat++;
        if (CurrentBeat >= _beatsPerBar)
        {
            CurrentBeat = 0;
            CurrentBar++;
        }

        if (CurrentBar >= _barCount)
        {
            _logger?.Log("Song finished!");
            _isGameOver = true;
        }
    }

    private float _perfectWindowThreshold = 50f;
    private float _goodWindowThreshold = 100f;

    private void HandScrubberMovement()
    {
        var elapsed = (float)(AudioSettings.dspTime - _startTime);

        transform.position = new Vector3(_initialPosition.x + 0.45f - (elapsed / 0.6f), _initialPosition.y, 0);
    }

    private void HandlePlayerMovement()
    {
        if(_playerCurrentNode.DownNode != null)
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
        if(_playerCurrentNode == null)
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
            var now = Time.time * 1000f;
            bool shouldTryToMove = Mathf.Abs(now - _rhythmHandler.NextTickTime) <= _goodWindowThreshold;

            if(shouldTryToMove)
            {
                // player index

                // check if movement direction is valid

                // move player

                // handle movement events
            }
        }
    }

    private void HandleBeetChecker()
    {

        if (_rhythmHandler.ShouldTick())
        {
            Debug.Log("Tick " + _rhythmHandler.CurrentTick);
            AdvanceBeat();
            HandlePlayerMovement();
        }

        if (_audioNormal.isPlaying && !_isPlaying)
        {
            _isPlaying = true;
            _nextEventTime = (float)AudioSettings.dspTime + _eighthNoteInterval;
        }

        if (_isPlaying && AudioSettings.dspTime >= _nextEventTime)
        {
            // Do something on every eighth note
            Debug.Log("Eighth note!");

            _nextEventTime += _eighthNoteInterval;
        }
    }

    private void RenderGraph()
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

    public class RhythmHandler2
    {
        public int BPM = 100;
        public int Subdivisions = 8;
        public int CurrentTick { get; private set; }
        public float NextTickTime { get; set; }

        private float _msPerTick;
        private double _startTime;        

        public RhythmHandler2(double startTime)
        {
            _startTime = startTime;

            var elapsed = (float)(AudioSettings.dspTime - _startTime);

            //_logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);

            _msPerTick = 300f;
            NextTickTime = _msPerTick - (elapsed % 300f);
        }

      
        public bool ShouldTick()
        {
            var elapsed = (float)(AudioSettings.dspTime - _startTime);
            float now = (elapsed * 1000f);

            if(now >= NextTickTime)
            {
                CurrentTick++;
                NextTickTime += _msPerTick - (elapsed % 300f);
                return true;
            }

            return false;
        }
    }
}
