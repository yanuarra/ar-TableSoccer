
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace YRA
{
    public class MenuSystem : Singleton<MenuSystem>
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Button References")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button ARLevelButton;
        private GameObject currentActivePanel;
        private bool isGamePaused = false;
        private List<GameObject> allPanels;

        void Start()
        {
                
            // Initialize panels list
            allPanels = new List<GameObject>
            {
                mainMenuPanel,
                pauseMenuPanel,
                winPanel,
                losePanel,
            };

            // Setup initial state
            HideAllPanels();
            ShowMainMenu();

            if (startGameButton) startGameButton.onClick.AddListener(StartGame);
            if (quitGameButton) quitGameButton.onClick.AddListener(QuitGame);
            if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
            if (mainMenuButton) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (retryButton) retryButton.onClick.AddListener(RestartLevel);
            if (ARLevelButton) ARLevelButton.onClick.AddListener(LoadARLevel);
        }

        
        private void Update()
        {
            // Check for escape key to toggle pause menu during gameplay
            if (Keyboard.current.escapeKey.wasPressedThisFrame && SceneManager.Instance.GetCurrentActiveScene().buildIndex != 0)
            {
                TogglePauseMenu();
            }
        }

        #region Panel Management
        private void HideAllPanels()
        {
            foreach (var panel in allPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            currentActivePanel = null;
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel == null) return;

            HideAllPanels();
            panel.SetActive(true);
            currentActivePanel = panel;
        }
        
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
            Time.timeScale = 1f;
            isGamePaused = false;
        }

        public void ShowPauseMenu()
        {
            ShowPanel(pauseMenuPanel);
            Time.timeScale = 0f;
            isGamePaused = true;
        }

        public void ShowWinScreen()
        {
            ShowPanel(winPanel);
            Time.timeScale = 0f;
        }

        public void ShowGameOverScreen()
        {
            ShowPanel(losePanel);
            Time.timeScale = 0f;
        }

        public void TogglePauseMenu()
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                ShowPauseMenu();
            }
        }
        #endregion

        #region Game Flow Functions
        public void StartGame()
        {
            HideAllPanels();
            SceneManager.Instance.OpenScene(1); 
            Time.timeScale = 1f;
            isGamePaused = false;
        }

        public void ResumeGame()
        {
            HideAllPanels();
            Time.timeScale = 1f;
            isGamePaused = false;
        }
        
        public void ReturnToMainMenu()
        {
            HideAllPanels();
            SceneManager.Instance.OpenScene(0); 
            ShowMainMenu();
        }

        public void RestartLevel()
        {
            HideAllPanels();
            SceneManager.Instance.RestartLevel();
            Time.timeScale = 1f;
            isGamePaused = false;
        }

        public void LoadARLevel()
        {
            HideAllPanels();
            SceneManager.Instance.OpenScene(2); 
            Time.timeScale = 1f;
            isGamePaused = false;
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        #endregion
    }
}
