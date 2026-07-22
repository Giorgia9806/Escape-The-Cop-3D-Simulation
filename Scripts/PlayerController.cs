using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Player controller:
/// - Movement (Rigidbody + smooth rotation)
/// - Jump with coyote time + jump buffer
/// - Coin pickup + win condition
/// - Enemy collision + lose condition (only after gameplay unlock)
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Input (New Input System)")]
    public InputAction MoveAction; // Keep this name to preserve Inspector references (WASD)

    [Header("UI")]
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject restartButton;

    [Header("Enemy")]
    public GameObject enemy;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float walkSpeed = 1.0f;
    public float turnSpeed = 20f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Jump Feel")]
    public float coyoteTime = 0.12f;        // Allows jumping shortly after leaving an edge
    public float jumpBuffer = 0.12f;        // If pressed shortly before landing, jump triggers on landing
    public float groundCheckDistance = 0.35f;

    private Rigidbody m_Rigidbody;
    private Vector3 m_Movement;
    private Quaternion m_Rotation = Quaternion.identity;

    private int count;
    private bool isDead = false;

    // Jump helpers
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpPressedThisFrame;

    private void Start()
    {
        // Rigidbody setup
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody != null)
        {
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Full restart safety
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Enable movement action (WASD / stick)
        MoveAction.Enable();

        // UI reset
        count = 0;
        SetCountText();
        if (winTextObject != null) winTextObject.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // Gameplay state reset
        isDead = false;
        GameManager.unlocked = false;

        // Make sure player collider is enabled
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Make sure player model is visible (in case it was hidden on lose)
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Re-enable enemy (you disable it on WIN)
        if (enemy != null)
            enemy.SetActive(true);

        // Restart menu music (as in your original logic)
        AudioManager.I?.MusicMenuStart();
    }

    private void Update()
    {
        // Very reliable jump input (won't be missed)
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
            jumpPressedThisFrame = true;

        // Jump buffer countdown
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    /// <summary>
    /// Reads movement input and moves/rotates the Rigidbody.
    /// </summary>
    /// 
    
    //
    private void HandleMovement()
    {
        Vector2 input = MoveAction.ReadValue<Vector2>();
        float horizontal = input.x;
        float vertical   = input.y;

        // Fallback se manca la camera
        if (cameraTransform == null)
        {
            m_Movement.Set(horizontal, 0f, vertical);
        }
        else
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight   = cameraTransform.right;

            // Togli la componente verticale (così il movimento resta sul piano)
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Input trasformato nello spazio della camera
            Vector3 move = camRight * horizontal + camForward * vertical;
            m_Movement = move.sqrMagnitude > 0.0001f ? move.normalized : Vector3.zero;
        }

        // Rotazione verso la direzione di movimento (se ti stai muovendo)
        if (m_Movement.sqrMagnitude > 0.0001f)
        {
            Vector3 desiredForward = Vector3.RotateTowards(
                transform.forward,
                m_Movement,
                turnSpeed * Time.fixedDeltaTime,
                0f);

            m_Rotation = Quaternion.LookRotation(desiredForward);
            m_Rigidbody.MoveRotation(m_Rotation);
        }

        // Movimento
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * walkSpeed * Time.fixedDeltaTime);
    }

    // VECCHIO HandleMovement()
    /*private void HandleMovement()
    {
        Vector2 input = MoveAction.ReadValue<Vector2>();

        float horizontal = input.x;
        float vertical = input.y;

        m_Movement.Set(horizontal, 0f, vertical);

        if (m_Movement.sqrMagnitude > 0.0001f)
        {
            m_Movement.Normalize();

            // Smoothly rotate toward movement direction
            Vector3 desiredForward = Vector3.RotateTowards(
                transform.forward,
                m_Movement,
                turnSpeed * Time.fixedDeltaTime,
                0f);

            m_Rotation = Quaternion.LookRotation(desiredForward);
            m_Rigidbody.MoveRotation(m_Rotation);
        }

        // Move in the movement direction
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * walkSpeed * Time.fixedDeltaTime);
    }*/

    /// <summary>
    /// Jump logic with coyote time + jump buffer.
    /// </summary>
    private void HandleJump()
    {
        bool groundedNow = IsGrounded();

        // Update coyote timer
        if (groundedNow)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // If jump pressed in Update, store it in the buffer
        if (jumpPressedThisFrame)
        {
            jumpBufferCounter = jumpBuffer;
            jumpPressedThisFrame = false;
        }

        // Execute jump if buffer + coyote are valid
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            // Consistent jump: reset vertical velocity first
            Vector3 v = m_Rigidbody.linearVelocity;
            v.y = 0f;
            m_Rigidbody.linearVelocity = v;

            m_Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Consume buffer and coyote
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    /// <summary>
    /// Simple grounded check using a raycast.
    /// </summary>
    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Collect coins
        if (!other.CompareTag("PickUp"))
            return;

        AudioManager.I?.SfxCoin();

        other.gameObject.SetActive(false);
        count++;

        SetCountText();
    }

    private void SetCountText()
    {
        if (countText != null)
            countText.text = "Count: " + count;

        // WIN condition
        if (count >= 23)
        {
            if (winTextObject != null) winTextObject.SetActive(true);
            if (restartButton != null) restartButton.SetActive(true);
            if (enemy != null) enemy.SetActive(false);

            AudioManager.I?.WinDuckAndPlay();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;

        if (!collision.gameObject.CompareTag("Enemy"))
            return;

        // Before unlock, the enemy is harmless
        if (!GameManager.unlocked)
            return;

        isDead = true;

        // 1) Stop player movement/physics
        MoveAction.Disable();

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic = true;
        }

        // 2) Disable collider (avoid repeated collisions)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3) Show lose message (reusing the same UI text object)
        if (winTextObject != null)
        {
            winTextObject.SetActive(true);
            TextMeshProUGUI tmp = winTextObject.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "YOU LOSE!";
        }

        if (restartButton != null)
            restartButton.SetActive(true);

        // 4) Stop all enemies (optional but good)
        foreach (var agent in Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>(FindObjectsSortMode.None))
            agent.enabled = false;

        // 5) Hide player model (keep UI visible)
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        AudioManager.I?.FailDuckAndPlay();
    }
}