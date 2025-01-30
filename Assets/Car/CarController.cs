using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 1500f;
    public float turnTorque = 500f;
    public float maxSpeed = 20f;
    public float drag = 0.5f;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Slight drag helps keep the car from endlessly sliding.
        rb.drag = drag;

        // Optionally, lower the center of mass to reduce tipping.
        // rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void FixedUpdate()
    {
        // Get movement input (W/S) and turning input (A/D).
        moveInput = Input.GetAxis("Vertical");   // W = 1, S = -1
        turnInput = Input.GetAxis("Horizontal"); // A = -1, D = 1

        // Limit max speed (optional)
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // Limit angular velocity to avoid uncontrollable spin (optional)
        Vector3 angVel = rb.angularVelocity;
        if (Mathf.Abs(angVel.y) < 5f) 
        {
            rb.AddTorque(transform.up * turnInput * turnTorque * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }
}
