using UnityEngine;

/// <summary>
/// Starts the menu background music when this scene loads.
/// Place this script in the Menu scene on any active GameObject.
/// </summary>
public class MenuAudioStarter : MonoBehaviour
{
    private void Start()
    {
        // AudioManager is a persistent singleton, so it may already exist.
        AudioManager.I?.MusicMenuStart();
    }
}