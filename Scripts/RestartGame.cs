using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reloads the current scene, effectively restarting the game.
/// Typically called by a UI button.
/// </summary>
public class RestartGame : MonoBehaviour
{
    /// <summary>
    /// Restarts the current scene and ensures time scale is reset.
    /// </summary>
    public void Restart()
    {
        // Ensure the game is not paused (e.g. after win/lose)
        Time.timeScale = 1f;

        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
