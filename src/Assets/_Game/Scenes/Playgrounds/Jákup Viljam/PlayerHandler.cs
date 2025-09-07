using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jákup_Viljam.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jákup_Viljam
{
    public class PlayerHandler : MonoBehaviour
    {
        public bool ConsumedInputThisTick { get; set; }

        public MusicNode CurrentNode { get; private set; }
        [SerializeField]
        private JVDGameHandler _gameHandler;
        [SerializeField]
        private RhythmHandler _rhythmHandler;

        private Core.Loggers.ILogger _logger;

        public void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);

            CurrentNode = _gameHandler.StartNode;
        }

        public void Update()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                HandleInput(+1);

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                HandleInput(-1);
        }

        public void OnRhythmTick()
        {
            AutoAdvance();
        }

        public void AutoAdvance()
        {
            if (CurrentNode == null) return;

            // Default move: take the "same line" node if it exists
            MusicNode auto = CurrentNode.NextNodes.Find(n => n.Line == CurrentNode.Line);
            if (auto == null && CurrentNode.NextNodes.Count > 0)
            {
                auto = CurrentNode.NextNodes[0]; // fallback
            }

            if (auto != null)
            {
                CurrentNode = auto;
            }

            _logger?.Log($"Auto-advanced to node {CurrentNode.Bar}/{CurrentNode.Beat}/{CurrentNode.Line}");
        }

        private void HandleInput(int direction)
        {
            if (CurrentNode == null) return;

            float now = Time.time * 1000f;
            float diff = Mathf.Abs(now - _rhythmHandler.NextTickTimeMs());

            // only allow input close to tick
            if (diff <= _gameHandler.GoodWindowMs)
            {
                TryMove(direction);
                _gameHandler.OnPlayerAction(CurrentNode, diff);
                ConsumedInputThisTick = true;

                _logger?.Log($"Accepted input {direction} (diff={diff:F0}ms) for node: {CurrentNode.Bar}/{CurrentNode.Beat}/{CurrentNode.Line}");
            }
            else
            {
                _logger?.Log($"Ignored input {direction} (too far from tick, diff={diff:F0}ms) for node: {CurrentNode.Bar}/{CurrentNode.Beat}/{CurrentNode.Line}");
            }
        }

        private void TryMove(int direction)
        {
            // Look for a next node that matches vertical move
            int targetLine = CurrentNode.Line + direction;
            MusicNode candidate = CurrentNode.NextNodes.Find(n => n.Line == targetLine);

            if (candidate != null)
            {
                CurrentNode = candidate;
                _logger?.Log($"Moved to node {CurrentNode.Bar}/{CurrentNode.Beat}/{CurrentNode.Line}");
            }
            else
            {
                _logger?.Log("Illegal move ignored (no neighbour in that direction).");
            }
        }
    }
}
