using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        // follow player but only on x and y axis, keep z axis unchanged
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
