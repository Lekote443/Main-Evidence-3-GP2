using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;           
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Rotation")]
    public float sensitivity = 3f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;

  
    private float yaw;
    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        
        yaw += Input.GetAxisRaw("Mouse X") * sensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        //  position behind player
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredOffset = rotation * offset;
        Vector3 desiredPos = target.position + desiredOffset;

        // Camera collision
        Vector3 finalPos = desiredPos;
        if (Physics.SphereCast(
            target.position,
            collisionRadius,
            desiredOffset.normalized,
            out RaycastHit hit,
            desiredOffset.magnitude,
            collisionMask))
        {
            finalPos = target.position + desiredOffset.normalized * (hit.distance - collisionRadius);
        }

        transform.position = finalPos;
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}