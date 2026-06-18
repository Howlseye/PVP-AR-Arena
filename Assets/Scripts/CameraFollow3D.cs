using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -6);
    public float smoothSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
