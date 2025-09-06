using UnityEngine;
using UnityEngine.InputSystem;

namespace Jákup_Viljam
{
    public class PlayerHandler : MonoBehaviour
    {
        public int CurrentLine = 2; // start middle

        [SerializeField]
        private JVDGameHandler _gameHandler;

        private Core.Loggers.ILogger _logger;

        public void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);
        }

        public void Update()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                HandleInput(+1);
            }

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                HandleInput(-1);
            }
        }

        public void TryMoveVertical(int direction)
        {
            CurrentLine = Mathf.Clamp(CurrentLine + direction, 0, _gameHandler.Lines - 1);
        }

        private void HandleInput(int direction)
        {
            TryMoveVertical(direction);
            _gameHandler.OnPlayerAction();
        }
    }
}
