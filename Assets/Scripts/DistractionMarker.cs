using UnityEngine;

public class DistractionMarker : MonoBehaviour
{
    [SerializeField] private float hearingRadius = 8f;
    [SerializeField] private float lifetime = 0.1f;

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
        Destroy(gameObject, lifetime);
    }

    private void OnDrawGizmosSelected()
    {
        // draw yellow circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
