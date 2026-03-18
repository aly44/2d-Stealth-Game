using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeedMult = 0.4f;

    [Header("Distractions")]
    [SerializeField] private GameObject distractionMarkerPrefab;
    public int maxCharges = 3;
    [SerializeField] private float chargeCooldown = 4f;

    [Header("Footsteps")]
    [SerializeField] private GameObject footstepNoisePrefab;
    [SerializeField] private float footstepInterval = 0.35f;

    public bool IsCrouching;
    public bool IsMoving;
    public int charges;

    private Rigidbody2D rigidBody;
    private float chargeTimer;
    private float footstepTimer;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        charges = maxCharges; // start with full charges
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameOver)
        {
            return;
        }

        IsCrouching = Input.GetKey(KeyCode.LeftShift);

        HandleDistraction();
        HandleFootsteps();
    }

    private void HandleDistraction()
    {
        // add a charge back after waiting
        if (charges < maxCharges)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeCooldown)
            {
                charges++;
                chargeTimer = 0f;
            }
        }

        if (Input.GetMouseButtonDown(0) && distractionMarkerPrefab != null && charges > 0)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f; // camera gives a z value, zero it out for 2d
            Instantiate(distractionMarkerPrefab, mouseWorldPos, Quaternion.identity);
            charges--;
            chargeTimer = 0f; // restart cooldown
        }
    }

    private void HandleFootsteps()
    {
        // no noise when crouching
        if (IsMoving && !IsCrouching && footstepNoisePrefab != null)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = footstepInterval;
                Instantiate(footstepNoisePrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            footstepTimer = footstepInterval; // reset so next step fires quickly when they start running again
        }
    }

    private void FixedUpdate()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized; // normalize so diagonal isnt faster
        IsMoving = moveDir.magnitude > 0.1f; // small threshold to avoid floating point issues

        float speed;
        if (IsCrouching)
        {
            speed = moveSpeed * crouchSpeedMult;
        }
        else
        {
            speed = moveSpeed;
        }
        rigidBody.linearVelocity = moveDir * speed;
    }
}
