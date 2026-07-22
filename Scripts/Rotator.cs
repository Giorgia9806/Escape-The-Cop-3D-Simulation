using UnityEngine;

/// <summary>
/// Continuously rotates the object around the world Y axis.
/// Commonly used for pickups or visual elements.
/// </summary>
public class Rotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 60f;

    private void Update()
    {
        // Rotate around the global Y axis at a constant speed
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
