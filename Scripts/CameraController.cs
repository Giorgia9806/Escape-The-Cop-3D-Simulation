using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple third-person camera controller.
/// - Orbits around the player using a fixed pitch
/// - Manual yaw rotation via keyboard
/// - Manual zoom in/out
/// - No smoothing (instant camera movement)
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 200f;
    [SerializeField] private float fixedPitch = 15f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 6f;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 12f;

    private float _yaw;

    private void Start()
    {
        // Auto-assign player if not set in the Inspector
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
        }

        // Initialize yaw from player or camera orientation
        if (player != null)
            _yaw = player.eulerAngles.y;
        else
            _yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        HandleInput(keyboard);
        UpdateCameraPosition();
    }

    /// <summary>
    /// Reads keyboard input and updates yaw and zoom values.
    /// </summary>
    private void HandleInput(Keyboard keyboard)
    {
        // Horizontal rotation (yaw)
        if (keyboard.leftArrowKey.isPressed)
            _yaw -= yawSpeed * Time.deltaTime;

        if (keyboard.rightArrowKey.isPressed)
            _yaw += yawSpeed * Time.deltaTime;

        // Zoom control
        if (keyboard.upArrowKey.isPressed)
            distance -= zoomSpeed * Time.deltaTime;

        if (keyboard.downArrowKey.isPressed)
            distance += zoomSpeed * Time.deltaTime;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    /// <summary>
    /// Calculates and applies the camera position and rotation.
    /// No smoothing: position is applied instantly.
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = player.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(fixedPitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
    }
}
