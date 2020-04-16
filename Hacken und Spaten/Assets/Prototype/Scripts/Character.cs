using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Character : MonoBehaviour
{
    #region Variables
    // Settings
    public float moveSpeed;

    [Header("Gravity Fine Tuning")]
    public float gravityStrength;
    
    // Width of cast to average floor position
    public float raycastWidth = 0.25f;
    public float floorRaycastOrigin;
    public float rayLengthGroundedCheck = 0.51f;
    public float rayLengthAverageCheck = 0.7f;

    // References
    private Rigidbody rb;

    // Internals
    // Camera & Movement
    private float vertical;
    private float horizontal;
    private float camX;
    private float camY;

    // Jump
    private bool isJumping = false;
    private bool jumpedLastFrame = false;

    private Vector3 correctedVertical;
    private Vector3 correctedHorizontal;

    private Vector3 moveDirection;

    // Gravity
    private bool grounded;
    private Vector3 currentGravity;

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {

        if (vertical != 0 || horizontal != 0)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            transform.rotation = rot;
        }

        ProcessInput();
        CorrectDirections();

    }

    private Vector3 floorSnapPosition;
    void FixedUpdate()
    {
        // 1 Figure out if grounded
        grounded = FindFloorPosition(0, 0, rayLengthGroundedCheck) != Vector3.zero;


   // if not grounded, add gravity
        if (!grounded)
        {
            currentGravity += Vector3.up * gravityStrength * Time.fixedDeltaTime;
        }

                // combine flat movement with gravity plus jump
        Vector3 jumpStrength = isJumping ? Vector3.up * 40 : Vector3.zero;
        rb.velocity = (moveDirection * moveSpeed) + currentGravity + jumpStrength;

        // 2 if not, figure out current downward force
        // 3 if grounded, set y to averaged floor
        // 4 apply all forces
                // find desired Y position via cursed raycasts
        floorSnapPosition = FindAverageFloorPosition();
        // if we're grounded, stick to floor
        if (grounded && floorSnapPosition != rb.position)
        {
            // snap to floor
            rb.MovePosition(new Vector3(rb.position.x, floorSnapPosition.y, rb.position.z));
            currentGravity.y = 0;
        }        
    }

    void ProcessInput()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        camX = Input.GetAxis("HorizontalCam");
        camY = Input.GetAxis("VerticalCam");

        isJumping = false;

        if (Input.GetButtonDown("Jump")){
            isJumping = true;
        }

    }

    private Vector3 combinedInput;
    void CorrectDirections()
    {
        correctedHorizontal = horizontal * Camera.main.transform.right;
        correctedVertical = vertical * Camera.main.transform.forward;

        combinedInput = correctedHorizontal + correctedVertical;
        moveDirection = new Vector3(combinedInput.normalized.x, 0, combinedInput.normalized.z);
    }

    private Vector3 floorAverage;
    Vector3 FindAverageFloorPosition()
    {
        floorAverage = (
            FindFloorPosition(0, 0, rayLengthGroundedCheck + 0.2f) +
            FindFloorPosition(raycastWidth, 0, rayLengthGroundedCheck + 0.2f) +
            FindFloorPosition(-raycastWidth, 0, rayLengthGroundedCheck + 0.2f) +
            FindFloorPosition(0, raycastWidth, rayLengthGroundedCheck + 0.2f) +
            FindFloorPosition(0, -raycastWidth, rayLengthGroundedCheck + 0.2f)
        );

        return floorAverage / 5;
    }

    // Returns position of floor or 0 if no floor found
    Vector3 origin;
    Vector3 FindFloorPosition(float offsetx, float offsetz, float raycastLength)
    {
        RaycastHit hit;
        origin = new Vector3(rb.position.x + offsetx, rb.position.y + floorRaycastOrigin, rb.position.z + offsetz);

        Debug.DrawRay(origin, Vector3.down, Color.magenta);
        if (Physics.Raycast(origin, Vector3.down, out hit, raycastLength))
        {
            return hit.point;
        }
        else return Vector3.zero;
    }
}
