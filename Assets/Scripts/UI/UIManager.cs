using UnityEngine;
using UnityEngine.UI;
using TempleRunner.Core;

namespace TempleRunner.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            ShowOnly(mainMenuPanel);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    ShowOnly(mainMenuPanel);
                    break;
                case GameState.Playing:
                    ShowOnly(hudPanel);
                    break;
                case GameState.Paused:
                    pausePanel?.SetActive(true);
                    break;
                case GameState.GameOver:
                    ShowOnly(gameOverPanel);
                    break;
            }
        }

        private void ShowOnly(GameObject activePanel)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == activePanel);
            if (hudPanel != null) hudPanel.SetActive(hudPanel == activePanel);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(gameOverPanel == activePanel);
        }

        // Button Callbacks
        public void OnClickPlay()
        {
            GameManager.Instance?.StartGame();
        }

        public void OnClickPause()
        {
            GameManager.Instance?.PauseGame();
        }

        public void OnClickResume()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnClickRestart()
        {
            GameManager.Instance?.RestartGame();
        }

        public void OnClickMainMenu()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
