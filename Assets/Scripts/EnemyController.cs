using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 1.5f;

    [Header("Detection")]
    [SerializeField] private float fov = 55f;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private float detectionRateFull = 0.8f; // how fast the meter fills when player is right in front of them
    [SerializeField] private float detectionRateMin = 0.2f; // how fast the meter fills when player is at max view distance
    [SerializeField] private float drainRateLow = 0.3f; // how fast the meter drains when they barely saw the player (gives them a chance to hide again)
    [SerializeField] private float drainRateHigh = 0.12f; // how fast the meter drains when they were pretty sure they saw the player (drains slower to keep them on edge)
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private Transform player;

    [Header("Investigation")]
    [SerializeField] private float investigateDuration = 3f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("References")]
    [SerializeField] private FieldOfView1 fieldOfView1;
    [SerializeField] private MeshRenderer fovRenderer;

    private static readonly Color COL_NORMAL = new Color(1f, 1f, 1f, 0.20f);
    private static readonly Color COL_SUSPICIOUS = new Color(1f, 0.85f, 0f, 0.30f);
    private static readonly Color COL_ALERT = new Color(1f, 0.40f, 0f, 0.45f);
    private static readonly Color COL_CAUGHT = new Color(1f, 0.10f, 0.10f, 0.55f);

    public float detectionMeter;
    public Vector2 velocity;

    // animator - which way to face when idle
    public Vector2 GetAimDirection()
    {
        return (Vector2)aimDirection;
    }

    private enum State
    { 
        Patrolling,
        Investigating 
    }
    private State state;

    // patrol variables
    private int waypointIndex;
    private float waitTimer;
    private bool waiting = true;
    private bool alerted;

    // investigation variables
    private Vector3 aimDirection = Vector3.right;
    private Vector3 basePatrolDirection = Vector3.right;
    private float lookAngle;
    private float lookDirection = 1f;
    private float lookTimer;

    // stores the last place they saw the player or heard a noise, used for investigation and slowing down when suspicious
    private Vector3 lastKnownPosition;
    private float investigateTimer;

    private Rigidbody2D rigidBody;
    private PlayerController playerController;
    private Material fovMaterial;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        waitTimer = waitTime;
        lookTimer = Random.Range(0.8f, 1.5f); // randomize so all guards dont sweep in sync

        if (waypoints.Length > 0) // if no waypoints assigned, just stay where they are placed in the scene
        {
            transform.position = waypoints[0].position;
            rigidBody.position = waypoints[0].position;
        }

        if (fieldOfView1 != null)
        {
            fieldOfView1.SetFoV(fov);
            fieldOfView1.SetViewDistance(viewDistance);
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemy(this);
        }

        if (fovRenderer != null)
        {
            fovRenderer.sortingOrder = FogOfWar.FOVSortOrder;
            fovMaterial = fovRenderer.material = new Material(fovRenderer.sharedMaterial);
        }
    }

    private void Update()
    {
        if (alerted)
        {
            velocity = Vector2.zero; // freeze after catching the player
            return;
        }

        UpdateDetection();

        velocity = Vector2.zero; // reset each frame, state handlers set it if needed

        switch (state)
        {
            case State.Patrolling:
                HandlePatrol();
                break;

            case State.Investigating:
                HandleInvestigate();
                break;

        }

        if (fieldOfView1 != null)
        {
            fieldOfView1.SetOrigin(transform.position);
            fieldOfView1.SetAimDirection(aimDirection);
        }
    }

    private void FixedUpdate()
    {
        rigidBody.linearVelocity = velocity;
    }

    private void HandlePatrol()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        if (waiting)
        {
            HandleLookAround();
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                lookAngle = 0f; // reset so they dont start turned sideways
                waypointIndex = (waypointIndex + 1) % waypoints.Length; // wraps back to start
            }
            return;
        }

        Vector3 targetPosition = waypoints[waypointIndex].position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // slow down when suspicious
        if (detectionMeter > 0.65f)
        {
            aimDirection = (lastKnownPosition - transform.position).normalized; // stare at last known spot while still walking
            velocity = (Vector2)direction * moveSpeed * 0.4f;
        }
        else
        {
            aimDirection = direction;
            velocity = (Vector2)direction * moveSpeed;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            rigidBody.position = waypoints[waypointIndex].position; // snap to avoid floating point drift
            basePatrolDirection = aimDirection; // save so look around knows which way to sweep from
            waiting = true;
            waitTimer = waitTime;
            velocity = Vector2.zero;
        }
    }

    // look around while waiting at a waypoint
    private void HandleLookAround()
    {
        lookTimer -= Time.deltaTime;
        if (lookTimer <= 0f)
        {
            lookTimer = Random.Range(0.7f, 1.4f);
            lookDirection *= -1f; // flip direction
        }

        lookAngle += lookDirection * 55f * Time.deltaTime;
        lookAngle = Mathf.Clamp(lookAngle, -50f, 50f);

        // offset the look angle from the direction they were walking
        float baseAngle = Mathf.Atan2(basePatrolDirection.y, basePatrolDirection.x) * Mathf.Rad2Deg;
        float currentAngle = (baseAngle + lookAngle) * Mathf.Deg2Rad;
        aimDirection = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0f);
    }

    private void HandleInvestigate()
    {
        if (Vector3.Distance(transform.position, lastKnownPosition) > 0.3f)
        {
            Vector3 direction = (lastKnownPosition - transform.position).normalized;
            aimDirection = direction;
            velocity = (Vector2)direction * chaseSpeed;
        }
        else
        {
            // reached the spot, look around for a bit then give up
            HandleLookAround();
            investigateTimer -= Time.deltaTime;
            if (investigateTimer <= 0f)
            {
                state = State.Patrolling;
                detectionMeter = 0f; // clear suspicion, back to normal
            }
        }
    }

    // called by footsteps, distractions, or when they lose sight of the player
    public void Investigate(Vector3 position)
    {
        if (alerted)
        {
            return;
        }
        lastKnownPosition = position;
        investigateTimer = investigateDuration;
        state = State.Investigating;
    }

    // called by the level builder at runtime to assign patrol points
    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        waypointIndex = 0;
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            if (rigidBody != null)
            {
                rigidBody.position = waypoints[0].position;
            }
        }
    }

    private void UpdateDetection()
    {
        if (IsPlayerVisible())
        {
            float distance = Vector3.Distance(transform.position, player.position);
            float distanceFraction = Mathf.Clamp01(distance / viewDistance);
            // detect slower when player is far
            float detectionRate = Mathf.Lerp(detectionRateFull, detectionRateMin, distanceFraction);

            if (playerController != null && playerController.IsCrouching)
            {
                detectionRate *= 0.3f; // crouching makes it much harder to detect
            }

            detectionMeter = Mathf.Clamp01(detectionMeter + detectionRate * Time.deltaTime);
            lastKnownPosition = player.position;

            if (detectionMeter >= 1f)
            {
                StartCoroutine(AlertSequence());
            }
        }
        else
        {
            float drainRate;
            if (detectionMeter < 0.35f)
            {
                drainRate = drainRateLow;
            }
            else
            {
                drainRate = drainRateHigh;
            }
            detectionMeter = Mathf.Clamp01(detectionMeter - drainRate * Time.deltaTime);

            // lost sight but still suspicious, go check last spot
            if (detectionMeter > 0.1f && detectionMeter < 0.65f && state != State.Investigating)
            {
                Investigate(lastKnownPosition);
            }
        }

        UpdateFOVColour();
    }

    private bool IsPlayerVisible()
    {
        if (player == null)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > viewDistance)
        {
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(aimDirection, directionToPlayer) > fov * 0.5f) // half fov on each side
        {
            return false;
        }

        // no wall in the way = can see player
        return Physics2D.Raycast(transform.position, directionToPlayer, distance, wallMask).collider == null;
    }

    private void UpdateFOVColour()
    {
        if (fovMaterial == null)
        {
            return;
        }

        Color targetColor;
        if (detectionMeter < 0.30f)
        {
            targetColor = COL_NORMAL;
        }
        else if (detectionMeter < 0.65f)
        {
            targetColor = COL_SUSPICIOUS;
        }
        else if (detectionMeter < 1.00f)
        {
            targetColor = COL_ALERT;
        }
        else
        {
            targetColor = COL_CAUGHT;
        }

        fovMaterial.color = Color.Lerp(fovMaterial.color, targetColor, Time.deltaTime * 6f); // smooth color transition
    }

    // wait a bit so the red flash actually shows before game over
    private IEnumerator AlertSequence()
    {
        alerted = true;
        if (fovMaterial != null)
        {
            fovMaterial.color = COL_CAUGHT;
        }
        yield return new WaitForSeconds(0.4f);
        GameManager.Instance.PlayerDetected();
    }
}
