using UnityEngine;
public class GrappleRope : MonoBehaviour
{
    [Header("References")]
    public GrapplingHook grapplingHook;
    public Transform grapplePoint;   

    [Header("Rope Settings")]
    public int quality = 200;  // Segment count — higher = smoother
    public float damper = 14f;
    public float strength = 800f;
    public float velocity = 15f;
    public float waveCount = 3f;   // Number of waves in wobble
    public float waveHeight = 1f;
    public AnimationCurve affectCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

  
    private LineRenderer lr;
    private Spring spring;
    private Vector3 currentGrapplePos;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0f);
    }

    private void OnEnable()
    {
        spring.Reset();
        if (grapplingHook.IsGrappling)
            currentGrapplePos = grapplePoint.position;
    }

    private void Update()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        // Hide rope when not grappling
        if (!grapplingHook.IsGrappling)
        {
            currentGrapplePos = grapplePoint.position;
            spring.Reset();
            spring.SetVelocity(velocity);
            lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grappleHitPoint = grapplingHook.GrappleHitPoint;
        Vector3 up = Quaternion.LookRotation((grappleHitPoint - grapplePoint.position).normalized)
                     * Vector3.up;

        currentGrapplePos = Vector3.Lerp(currentGrapplePos, grappleHitPoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight
                             * Mathf.Sin(delta * waveCount * Mathf.PI)
                             * spring.Value
                             * affectCurve.Evaluate(delta);

            lr.SetPosition(i,
                Vector3.Lerp(grapplePoint.position, currentGrapplePos, delta) + offset);
        }
    }
}


public class Spring
{
    private float strength;
    private float damper;
    private float target;
    private float currentVelocity;

    public float Value { get; private set; }

    public void Update(float deltaTime)
    {
        float direction = target - Value >= 0 ? 1f : -1f;
        float force = Mathf.Abs(target - Value) * strength;
        currentVelocity += (force * direction - currentVelocity * damper) * deltaTime;
        Value += currentVelocity * deltaTime;
    }

    public void Reset() => Value = 0f;
    public void SetVelocity(float v) => currentVelocity = v;
    public void SetDamper(float d) => damper = d;
    public void SetStrength(float s) => strength = s;
    public void SetTarget(float t) => target = t;
}