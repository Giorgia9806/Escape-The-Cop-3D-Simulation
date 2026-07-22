using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles main menu actions such as starting the game
/// and quitting the application.
/// </summary>
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// Loads the main game scene.
    /// Called by the Play button.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Quits the application.
    /// Note: this has no effect in the Unity Editor.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
