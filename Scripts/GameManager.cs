using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages global gameplay state (locked / unlocked).
/// When locked:
/// - Pickups are visible but not collectable
/// - Enemies are visible but do not move
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene Roots (Drag & Drop)")]
    [SerializeField] private Transform pickupsRoot;   // Parent of all pickups
    [SerializeField] private Transform enemiesRoot;   // Parent of all enemies

    /// <summary>
    /// Global gameplay lock flag.
    /// True = gameplay unlocked.
    /// </summary>
    public static bool unlocked = false; 

    private void Start()
    {
        SetLocked(true);
    }

    /// <summary>
    /// Unlocks gameplay elements (pickups and enemies).
    /// Safe to call multiple times.
    /// </summary>
    public void UnlockGameplay()
    {
        if (unlocked)
            return;

        unlocked = true;
        SetLocked(false);
    }

    /// <summary>
    /// Enables or disables gameplay-related components
    /// without hiding the objects.
    /// </summary>
    private void SetLocked(bool locked)
    {
        TogglePickups(!locked);
        ToggleEnemies(!locked);
    }

    /// <summary>
    /// Enables or disables pickup colliders.
    /// Pickups remain visible at all times.
    /// </summary>
    private void TogglePickups(bool enabled)
    {
        if (pickupsRoot == null)
            return;

        Collider[] colliders = pickupsRoot.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }
    }

    /// <summary>
    /// Enables or disables enemy NavMeshAgents.
    /// When enabling, agents are safely repositioned on the NavMesh.
    /// </summary>
    private void ToggleEnemies(bool enabled)
    {
        if (enemiesRoot == null)
            return;

        NavMeshAgent[] agents = enemiesRoot.GetComponentsInChildren<NavMeshAgent>(true);
        foreach (NavMeshAgent agent in agents)
        {
            if (agent == null)
                continue;

            agent.enabled = enabled;

            if (enabled)
            {
                // Ensure the agent is placed on a valid NavMesh position
                if (NavMesh.SamplePosition(
                        agent.transform.position,
                        out NavMeshHit hit,
                        2f,
                        NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }
    }
}
