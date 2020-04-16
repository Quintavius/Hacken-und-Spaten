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
    private Vector3 raycastFloorPos;
    private Vector3 floorMovement;
    private Vector3 combinedRaycast;

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

    void FixedUpdate()
    {
        // 1 Figure out if grounded
        grounded = FloorRaycasts(0, 0, rayLengthGroundedCheck) != Vector3.zero;

        // 2 if not, figure out current downward force
        // 3 if grounded, set y to averaged floor
        // 4 apply all forces
        // if not grounded, add gravity
        if (!grounded)
        {
            currentGravity += Vector3.up * gravityStrength * Time.fixedDeltaTime;
        }

        // combine flat movement with gravity plus jump
        Vector3 jumpStrength = isJumping ? Vector3.up * 4 : Vector3.zero;
        rb.velocity = (moveDirection * moveSpeed) + currentGravity + jumpStrength;

        // find desired Y position via cursed raycasts
        floorMovement = new Vector3(rb.position.x, FindFloor().y, rb.position.z);

        // if we're grounded, stick to floor
        if (grounded && floorMovement != rb.position)
        {
            // snap to floor
            rb.MovePosition(floorMovement);
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

    private int floorAverage = 1;
    Vector3 FindFloor()
    {
        floorAverage = 1;
        combinedRaycast = FloorRaycasts(0, 0, rayLengthAverageCheck);
        floorAverage += (
            getFloorAverage(raycastWidth, 0) +
            getFloorAverage(-raycastWidth, 0) +
            getFloorAverage(0, raycastWidth) +
            getFloorAverage(0, -raycastWidth)
        );

        return combinedRaycast / floorAverage;
    }

    int getFloorAverage(float offsetx, float offsetz)
    {
        if (FloorRaycasts(offsetx, offsetz, 1) != Vector3.zero)
        {
            combinedRaycast += FloorRaycasts(offsetx, offsetz, rayLengthAverageCheck);
            return 1;
        }
        else { return 0; }
    }

    // Returns position of floor
    Vector3 FloorRaycasts(float offsetx, float offsetz, float raycastLength)
    {
        RaycastHit hit;
        raycastFloorPos = transform.TransformPoint(0 + offsetx, 0 + floorRaycastOrigin, 0 + offsetz);

        Debug.DrawRay(raycastFloorPos, Vector3.down, Color.magenta);
        if (Physics.Raycast(raycastFloorPos, Vector3.down, out hit, raycastLength))
        {
            return hit.point;
        }
        else return Vector3.zero;
    }
}
