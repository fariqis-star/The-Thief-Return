using RunAndGun.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunAndGun.UI
{
    /// <summary>
    /// Shows a Game Over screen when the player dies and lets them restart.
    ///
    /// It listens to the player's Health.OnDied event, enables a Game Over panel,
    /// and freezes time (Time.timeScale = 0) so nothing keeps moving. The Restart
    /// button on the panel should call Restart(), which unfreezes time and reloads
    /// the current scene from scratch.
    /// </summary>
    public class GameOverManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player's Health component. When it dies, the game over screen shows.")]
        [SerializeField] private Health _playerHealth;

        [Tooltip("The Game Over panel (a UI GameObject). Starts hidden, shown on death.")]
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Options")]
        [Tooltip("If true, freezes the game (Time.timeScale = 0) while the game over screen is up.")]
        [SerializeField] private bool _freezeTimeOnGameOver = true;

        private void Awake()
        {
            // Make sure the panel is hidden when the game starts.
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_playerHealth != null) _playerHealth.OnDied += ShowGameOver;
        }

        private void OnDisable()
        {
            if (_playerHealth != null) _playerHealth.OnDied -= ShowGameOver;
        }

        /// <summary>Called when the player's Health hits 0.</summary>
        private void ShowGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);

            // Freeze everything so enemies/bullets stop while the screen is up.
            if (_freezeTimeOnGameOver) Time.timeScale = 0f;
        }

        /// <summary>
        /// Hook this up to the Restart button's OnClick. Unfreezes time and reloads
        /// the current scene, resetting the player, enemies and everything else.
        /// </summary>
        public void Restart()
        {
            // Always restore time before loading, or the new scene would start frozen.
            Time.timeScale = 1f;

            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        /// <summary>Optional: hook this to a Quit button. Does nothing in the editor.</summary>
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
