using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Displays a 3D arrow above the player that points toward
/// the nearest active coin in the scene.
/// The arrow can be toggled on/off and optionally bobs up and down.
/// </summary>
public class CoinGuideArrow3D : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private Transform arrow;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 2f, 0f);

    [Header("Coins")]
    [SerializeField] private string coinTag = "PickUp";
    [SerializeField] private float maxSearchRadius = 9999f;

    [Header("Toggle")]
    [SerializeField] private Key toggleKey = Key.H;

    [Header("Behaviour")]
    [SerializeField] private bool rotateOnlyOnY = true;
    [SerializeField] private bool bobUpDown = true;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobAmount = 0.08f;

    private bool _isEnabled = true;

    private void Start()
    {
        if (arrow == null)
            return;

        arrow.localPosition = localOffset;
        arrow.gameObject.SetActive(_isEnabled);
    }

    private void Update()
    {
        HandleToggleInput();

        if (!_isEnabled || arrow == null)
            return;

        UpdateArrowPosition();
        UpdateArrowRotation();
    }

    /// <summary>
    /// Handles keyboard input to toggle the arrow on/off.
    /// </summary>
    private void HandleToggleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard[toggleKey].wasPressedThisFrame)
        {
            _isEnabled = !_isEnabled;
            if (arrow != null)
                arrow.gameObject.SetActive(_isEnabled);
        }
    }

    /// <summary>
    /// Updates the arrow local position, including optional bobbing motion.
    /// </summary>
    private void UpdateArrowPosition()
    {
        Vector3 offset = localOffset;

        if (bobUpDown)
            offset.y += Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        arrow.localPosition = offset;
    }

    /// <summary>
    /// Rotates the arrow to point toward the nearest active coin.
    /// </summary>
    private void UpdateArrowRotation()
    {
        GameObject nearestCoin = FindNearestActiveCoin();
        if (nearestCoin == null)
        {
            arrow.gameObject.SetActive(false);
            _isEnabled = false;
            return;
        }

        Vector3 direction = nearestCoin.transform.position - transform.position;

        if (rotateOnlyOnY)
            direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        // The arrow's +Z axis points toward the target
        arrow.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    /// <summary>
    /// Finds the nearest active coin within the search radius.
    /// </summary>
    private GameObject FindNearestActiveCoin()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag(coinTag);

        GameObject bestCoin = null;
        float bestDistance = float.MaxValue;
        Vector3 currentPosition = transform.position;

        foreach (GameObject coin in coins)
        {
            if (!coin.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(currentPosition, coin.transform.position);
            if (distance <= maxSearchRadius && distance < bestDistance)
            {
                bestDistance = distance;
                bestCoin = coin;
            }
        }

        return bestCoin;
    }
}
