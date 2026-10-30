using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TempleRunner.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Master game state controller for TempleRunner3D clean-room implementation.
    /// Manages state transitions, pause/resume, and seamless in-memory restart.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;

        public event Action<GameState> OnStateChanged;
        public event Action OnGameRestarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            SetState(GameState.MainMenu);
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            switch (currentState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    break;
            }

            OnStateChanged?.Invoke(currentState);
        }

        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;
            SetState(GameState.GameOver);
        }

        /// <summary>
        /// Restarts the game session without reloading the entire Unity scene,
        /// ensuring zero memory leaks or duplicate UI objects.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            OnGameRestarted?.Invoke();
            SetState(GameState.Playing);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
        }
    }
}
