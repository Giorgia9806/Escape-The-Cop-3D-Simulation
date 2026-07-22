using UnityEngine;

/// <summary>
/// Trigger that shows a hint UI when the player enters,
/// starts chase music, and unlocks gameplay when the player exits.
/// This trigger can only be activated once.
/// </summary>
public class HintUnlockTrigger : MonoBehaviour
{
    [Header("UI Hint")]
    [SerializeField] private GameObject hintUI;

    [Header("Gameplay Unlock")]
    [SerializeField] private GameManager gameManager;

    private bool _hasTriggered = false;

    private void Start()
    {
        if (hintUI != null)
            hintUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the player can activate the trigger
        if (_hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        _hasTriggered = true;

        // Show hint UI
        if (hintUI != null)
            hintUI.SetActive(true);

        // Start chase music
        AudioManager.I?.MusicChase();
    }

    private void OnTriggerExit(Collider other)
    {
        // Only the player
        if (!other.CompareTag("Player"))
            return;

        // Hide hint UI
        if (hintUI != null)
            hintUI.SetActive(false);

        // Unlock gameplay (safe to call multiple times)
        if (gameManager != null)
            gameManager.UnlockGameplay();
    }
}
