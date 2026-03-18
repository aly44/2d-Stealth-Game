using UnityEngine;

public class FieldOfView1 : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private Mesh mesh;
    private float fov;
    private float viewDistance;
    private Vector3 origin;
    private float startingAngle;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    // rebuild the mesh every frame so the fov reacts to walls in real time
    private void LateUpdate()
    {
        const int RAY_COUNT = 120;
        float angle = startingAngle;
        float angleIncrease = fov / RAY_COUNT;

        // +2 for the origin point and closing the cone
        Vector3[] vertices = new Vector3[RAY_COUNT + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[RAY_COUNT * 3];

        vertices[0] = transform.InverseTransformPoint(origin); // first vertex is the eye point

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int rayIndex = 0; rayIndex <= RAY_COUNT; rayIndex++)
        {
            Vector3 vertex;
            RaycastHit2D hit = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layerMask);

            if (hit.collider == null)
            {
                vertex = origin + GetVectorFromAngle(angle) * viewDistance; // nothing hit, go full distance
            }
            else
            {
                vertex = hit.point; // stop at the wall
            }

            vertices[vertexIndex] = transform.InverseTransformPoint(vertex);

            if (rayIndex > 0)
            {
                // each triangle fans out from the origin (vertex 0)
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f); // huge bounds so unity never culls it
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = GetAngleFromVectorFloat(aimDirection) + fov / 2f; // start from left edge of the cone
    }

    public void SetFoV(float fov)
    {
        this.fov = fov;
    }

    public void SetViewDistance(float viewDistance)
    {
        this.viewDistance = viewDistance;
    }

    private static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRadians = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
    }

    private static float GetAngleFromVectorFloat(Vector3 direction)
    {
        direction = direction.normalized;
        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angleDegrees < 0)
        {
            angleDegrees += 360; // keep it in 0-360 range
        }
        return angleDegrees;
    }
}
