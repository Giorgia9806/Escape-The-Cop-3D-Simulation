using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jump pad trigger:
/// - If the entering object has a Rigidbody (and is NOT a NavMeshAgent), it gets bounced upward.
/// - If the entering object has a NavMeshAgent, it gets "launched" in an arc to a landing point,
///   then re-attached to the NavMesh.
/// </summary>
public class JumpPad : MonoBehaviour
{
    [Header("Player (Rigidbody)")]
    [SerializeField] private float playerBounceImpulse = 10f;

    [Header("Enemy (NavMeshAgent)")]
    [SerializeField] private Transform agentLandingPoint;
    [SerializeField] private float agentJumpHeight = 2.5f;
    [SerializeField] private float agentJumpDuration = 0.6f;
    [SerializeField] private float sampleRadius = 2f;

    private void OnTriggerEnter(Collider other)
    {
        // ================= PLAYER =================
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rb.GetComponent<NavMeshAgent>() == null)
        {
            BounceRigidbody(rb);
            return;
        }

        // ================= ENEMY =================
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            if (agentLandingPoint == null)
                return;

            StartCoroutine(JumpAgentCoroutine(agent, agentLandingPoint.position));
        }
    }

    /// <summary>
    /// Resets the vertical component and applies an upward impulse.
    /// Uses linearVelocity (as required by your project setup).
    /// </summary>
    private void BounceRigidbody(Rigidbody rb)
    {
        // Trampoline SFX
        AudioManager.I?.SfxTrampoline();

        // Reset vertical velocity to make the bounce consistent
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * playerBounceImpulse, ForceMode.Impulse);
    }

    /// <summary>
    /// Temporarily disables the NavMeshAgent and manually moves it along a parabolic arc.
    /// Then it re-enables the agent and snaps it back onto the NavMesh near the landing point.
    /// </summary>
    private IEnumerator JumpAgentCoroutine(NavMeshAgent agent, Vector3 landingPos)
    {
        if (!agent.enabled)
            yield break;

        agent.isStopped = true;
        agent.enabled = false;

        Vector3 startPos = agent.transform.position;
        float duration = Mathf.Max(0.01f, agentJumpDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            Vector3 pos = Vector3.Lerp(startPos, landingPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * agentJumpHeight;

            agent.transform.position = pos;
            yield return null;
        }

        // Re-attach to the NavMesh near the landing position
        if (NavMesh.SamplePosition(landingPos, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.transform.position = hit.position;
        }
        else
        {
            agent.transform.position = landingPos;
        }

        agent.enabled = true;
        agent.isStopped = false;
    }
}
