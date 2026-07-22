using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple intro hint that hides itself as soon as the player
/// provides any movement input.
/// Uses the same InputAction as the Player.
/// </summary>
public class IntroHint : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction moveAction;

    private bool _isHidden = false;

    private void OnEnable()
    {
        // Enable the input action when the object becomes active
        moveAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        moveAction.Disable();
    }

    private void Update()
    {
        if (_isHidden)
            return;

        // Read movement input (any direction)
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // If the player provides any movement input, hide the hint
        if (moveInput.sqrMagnitude > 0.01f)
        {
            _isHidden = true;
            gameObject.SetActive(false);
        }
    }
}
