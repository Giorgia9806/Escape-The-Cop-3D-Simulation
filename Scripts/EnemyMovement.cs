using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple enemy AI that continuously chases the player using Unity's NavMeshAgent.
/// The destination is updated at a fixed interval for stability and performance.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Chase Settings")]
    [Tooltip("How often the destination is updated (seconds). 0.1 is smooth and lightweight.")]
    [SerializeField] private float repathInterval = 0.1f;

    [Tooltip("Radius used to sample a valid NavMesh position near the player.")]
    [SerializeField] private float sampleRadius = 4f;

    private NavMeshAgent _agent;
    private float _repathTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
            enabled = false;
    }

    private void Update()
    {
        if (!CanUpdate())
            return;

        _repathTimer += Time.deltaTime;
        if (_repathTimer < repathInterval)
            return;

        _repathTimer = 0f;
        UpdateDestination();
    }

    /// <summary>
    /// Checks whether the agent is in a valid state to update its path.
    /// </summary>
    private bool CanUpdate()
    {
        if (player == null)
            return false;

        if (_agent == null || !_agent.enabled)
            return false;

        if (!_agent.isOnNavMesh)
            return false;

        return true;
    }

    /// <summary>
    /// Updates the NavMeshAgent destination toward the player by sampling a valid
    /// NavMesh position near the player's actual 3D position.
    /// </summary>
    private void UpdateDestination()
    {
        Vector3 desired = player.position;

        // Sample a NavMesh point near the player
        if (!NavMesh.SamplePosition(desired, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            return;

        // Validate reachability before committing
        NavMeshPath path = new NavMeshPath();
        bool hasPath = _agent.CalculatePath(hit.position, path);

        if (!hasPath || path.status == NavMeshPathStatus.PathInvalid)
        {
            _agent.ResetPath();
            return;
        }

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _agent.SetDestination(hit.position);
            return;
        }

        // PathPartial: move as far as possible (usually to an edge/ledge)
        if (path.corners != null && path.corners.Length > 0)
        {
            _agent.SetDestination(path.corners[path.corners.Length - 1]);
        }
    }
}
