using UnityEngine;

// spawned while running, pings nearby enemies to investigate
public class FootstepNoise : MonoBehaviour
{
    [SerializeField] private float hearingRadius = 3f;

    private void Start()
    {
        // Detect enemies within hearing radius and alert them
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hearingRadius);
        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Investigate(transform.position);
            }
        }
        Destroy(gameObject, 0.05f); // tiny delay so start runs before it gets destroyed
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
