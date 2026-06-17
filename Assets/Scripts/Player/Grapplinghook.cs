using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("References")]
    public Transform grapplePoint;      
    public LayerMask grappleableMask;   
    public Camera cam;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 30f;
    public float overshootYAxis = 2f;   // Upward launch boost when pulling to target

    [Header("Spring Settings")]
    public float spring = 4f;
    public float damper = 7f;
    public float massScale = 4.5f;

    [Header("Cooldown")]
    public float grappleCooldown = 1f;

  
    public bool IsGrappling { get; private set; }
    public Vector3 GrappleHitPoint { get; private set; }

   
    private SpringJoint joint;
    private LineRenderer lr;
    private PlayerMovement playerMovement;
    private Rigidbody rb;
    private float cooldownTimer;
    private bool grappleQueued;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        if (cam == null) cam = Camera.main;

        lr.enabled = false;
        lr.positionCount = 2;
    }

    private void Update()
    {
      
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1))
            TryStartGrapple();

        if (Input.GetMouseButtonUp(1))
            StopGrapple();
    }

    private void LateUpdate()
    {
       
        if (IsGrappling)
        {
            lr.SetPosition(0, grapplePoint.position);
            lr.SetPosition(1, GrappleHitPoint);
        }
    }

    private void TryStartGrapple()
    {
        if (cooldownTimer > 0f) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleableMask))
        {
            GrappleHitPoint = hit.point;
            IsGrappling = true;

            lr.enabled = true;

            CreateJoint();
            cooldownTimer = grappleCooldown;
        }
    }

    private void CreateJoint()
    {
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = GrappleHitPoint;

        float distanceFromPoint = Vector3.Distance(transform.position, GrappleHitPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = spring;
        joint.damper = damper;
        joint.massScale = massScale;
    }
    public void StopGrapple()
    {
        if (!IsGrappling) return;

        IsGrappling = false;
        lr.enabled = false;

        if (joint != null)
            Destroy(joint);
    }
    public void ExecutePull()
    {
        if (!IsGrappling) return;

        StopGrapple();

        
        Vector3 lowestPoint = new Vector3(transform.position.x,
                                            transform.position.y - 1f,
                                            transform.position.z);

        float grapplePointRelativeYPos = GrappleHitPoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0f)
            highestPointOnArc = overshootYAxis;

        JumpToPosition(GrappleHitPoint, highestPointOnArc);
    }

    
    private void JumpToPosition(Vector3 targetPos, float trajectoryHeight)
    {
        Vector3 velocity = CalculateJumpVelocity(transform.position, targetPos, trajectoryHeight);
        playerMovement.SetVelocity(velocity);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        
        float timeToPeak = Mathf.Sqrt(-2f * trajectoryHeight / gravity);
        
        float timeToFall = Mathf.Sqrt(2f * (displacementY - trajectoryHeight) / gravity) * -1f;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2f * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (timeToPeak + timeToFall);

        return velocityXZ + velocityY;
    }
    public bool IsOnCooldown() => cooldownTimer > 0f;
    public float CooldownRemaining => cooldownTimer;
}