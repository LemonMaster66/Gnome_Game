using System;
using PalexUtilities;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
        [HorizontalGroup("Speed", 0.85f)]
    public float Speed               = 50;
        [HorizontalGroup("Speed")]
        [DisplayAsString]
        [HideLabel]
    public float _speed;

        [HorizontalGroup("MaxSpeed", 0.85f)]
    public float MaxSpeed            = 80;
        [HorizontalGroup("MaxSpeed")]
        [DisplayAsString]
        [HideLabel]
    public float _maxSpeed;
    public float CounterMovement     = 10;

[Space(10)]

    public float JumpForce           = 8;
    public float Gravity             = 100;
    public float SlopeSlipperyness   = 2;


    [Header("Extras")]
    [Range(0,1)] public float WalkingTime;
    [Range(0,1)] public float RunningTime;
[PropertySpace(SpaceAfter = 10, SpaceBefore = 0)]
    [Range(0,1)] public float SkidFactor;

    [FoldoutGroup("States")] public bool  Walking        = false;
    [FoldoutGroup("States")] public bool  Running        = false;
    [FoldoutGroup("States")] public bool  Sprinting      = false;
    [FoldoutGroup("States")] public bool  Skidding       = false;
[Space(5)]
    [FoldoutGroup("States")] public bool  Grounded       = true;
    [FoldoutGroup("States")] public bool  Crouching      = false;
[Space(5)]
    [FoldoutGroup("States")] public bool  CanMove        = true;
    [FoldoutGroup("States")] public bool  Paused         = false;
[Space(5)]
    [FoldoutGroup("States")] public bool  HasJumped      = false;
    [FoldoutGroup("States")] public bool  HoldingCrouch  = false;
    [FoldoutGroup("States")] public bool  HoldingRun     = false;


    #region Debug Stats
        [Header("Debug Stats")]
        public Vector3     PlayerVelocity;
        public Vector3     SmoothVelocity;
        public float       VelocityMagnitude;
        public float       ForwardVelocityMagnitude;
        public Vector3     VelocityXZ;
        [Space(5)]
        public float slopeAngle;
        public Vector3 slopeVector;
        public Vector3 slopeCorrectionVector;
        [Space(5)]
        public Vector3 CamF;
        public Vector3 CamR;
        [Space(5)]
        public Vector3 Movement;
        public float   MovementX;
        public float   MovementY;
        public Vector3 CorrectMovement;
        [Space(8)]
        public float CoyoteTime;
        public float JumpBuffer;
        [Space(8)]
        
        public float   _counterMovement;
        public float   _gravity;
    #endregion
    
    
    #region Script / Component Reference
        [HideInInspector] public Rigidbody    rb;
        [HideInInspector] public Transform    Camera;

        private PlayerStats  playerStats;
        //private PlayerSFX    playerSFX;
        private GroundCheck  groundCheck;
    #endregion


    void Awake()
    {
        //Assign Components
        Camera  = GameObject.Find("Main Camera").transform;
        rb      = GetComponent<Rigidbody>();

        //Assign Scripts
        //playerSFX    = FindAnyObjectByType<PlayerSFX>();
        playerStats  = GetComponent<PlayerStats>();
        groundCheck  = GetComponentInChildren<GroundCheck>();

        //Component Values
        rb.useGravity = false;

        //Property Values
        _maxSpeed  = MaxSpeed;
        _speed     = Speed;
        _gravity   = Gravity;
    }


    void Update()
    {
        if(CoyoteTime > 0) CoyoteTime = Math.Clamp(CoyoteTime - Time.deltaTime, 0, 100);
        if(JumpBuffer > 0) JumpBuffer = Math.Clamp(JumpBuffer - Time.deltaTime, 0, 100);

        WalkingTime = Math.Clamp(WalkingTime + (Walking ? Time.deltaTime*3 : -Time.deltaTime*3), 0, 1);
        RunningTime = Math.Clamp(RunningTime + (RunningCheck() ? Time.deltaTime :
                                                                            -Time.deltaTime * (RunningTime > 0.9 ? 0.5f : 3)), 0, 1);

        SkidFactor = Math.Clamp(SkidFactor + Time.deltaTime*1.5f, 0, 1);
        Skidding = SkidFactor != 1;
        

        SprintState(RunningTime > 0.9);
    }

    void FixedUpdate()
    {
        #region PerFrame stuff
            #region Camera Orientation Values
                CamF = Camera.forward;
                CamR = Camera.right;
                CamF.y = 0;
                CamR.y = 0;
                CamF = CamF.normalized;
                CamR = CamR.normalized;

                //Rigidbody Velocity Magnitude on the X/Z Axis
                VelocityXZ = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                // Calculate the Forward velocity magnitude
                Vector3 ForwardVelocity = Vector3.Project(rb.linearVelocity, CamF);
                ForwardVelocityMagnitude = ForwardVelocity.magnitude;
                ForwardVelocityMagnitude = (float)Math.Round(ForwardVelocityMagnitude, 2);

                WalkingCheck();
            #endregion

            SmoothVelocity = Vector3.Slerp(SmoothVelocity, rb.linearVelocity, 0.15f);
            SmoothVelocity.x    = (float)Math.Round(SmoothVelocity.x, 4);
            SmoothVelocity.y    = (float)Math.Round(SmoothVelocity.y, 4);
            SmoothVelocity.z    = (float)Math.Round(SmoothVelocity.z, 4);

            //Gravity
            rb.AddForce(Physics.gravity * Gravity /10);

            // Calculate the Forward Angle
            float targetAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);

            LockToMaxSpeed();
        #endregion
        //**********************************

        
        // Movement Code
        if(!Paused && !playerStats.Dead && CanMove)
        {
            Movement = (CamF * MovementY + CamR * MovementX).normalized;
            CalculateCorrectiveMovement();

            rb.AddForce(CorrectMovement * CalculateMoveSpeed());             // Movement
            if(Grounded) rb.AddForce(VelocityXZ * -(CounterMovement / 10));  // CounterMovement
        }

        CalculateSlopeStickForce();


        if(VelocityXZ.magnitude > 0.2)  transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.6f);

        #region Rounding Values
            PlayerVelocity      = rb.linearVelocity;
            PlayerVelocity.x    = (float)Math.Round(PlayerVelocity.x, 2);
            PlayerVelocity.y    = (float)Math.Round(PlayerVelocity.y, 2);
            PlayerVelocity.z    = (float)Math.Round(PlayerVelocity.z, 2);
            VelocityMagnitude   = (float)Math.Round(rb.linearVelocity.magnitude, 2);
        #endregion
    }

    //***********************************************************************
    //***********************************************************************
    //Movement Functions
    public void OnMove(InputAction.CallbackContext MovementValue)
    {
        if(Paused) return;
        Vector2 inputVector = MovementValue.ReadValue<Vector2>();
        MovementX = inputVector.x;
        MovementY = inputVector.y;

        Vector3 movement = (CamF * MovementY + CamR * MovementX).normalized;
        if(Vector3.Dot(movement, rb.linearVelocity) < -0.8 && Grounded)
        {
            if(Running && !Skidding && rb.linearVelocity.magnitude > 5) Skid();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(Paused || !CanMove) return;
        if(context.started && !playerStats.Dead)
        {
            if((Grounded || CoyoteTime > 0) && !HasJumped) Jump();
            else if(!Grounded || CoyoteTime == 0) JumpBuffer = 0.15f;
        }
    }
    public void Jump()
    {
        HasJumped = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, math.clamp(rb.linearVelocity.y, 0, 1), rb.linearVelocity.z);
        rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started)
        {
            HoldingRun = true;
            RunState(true);
        }
        if(context.canceled) 
        {
            HoldingRun = false;
            RunState(false);
        }
    }
    public void RunState(bool state)
    {
        if(state && !Crouching)
        {
            Running = true;
        }
        else
        {
            Running = false;
            Sprinting = false;
        }
    }
    public void SprintState(bool state)
    {
        Sprinting = state;
    }

    //***********************************************************************
    //***********************************************************************
    //Extra MoveTech

    public void Halt()
    {
        Debug.Log("HALT!");
    }

    public void Skid()
    {
        Debug.Log("Skid");
        Skidding = true;

        SkidFactor = 0;
        RunningTime = 0;
    }


    //***********************************************************************
    //***********************************************************************
    //Calculations

    public float CalculateMoveSpeed()
    {
        if(Grounded)
        {
            float speedValue = Speed;

            speedValue *= Running   ? 1.5f : 1;  // Run Boost
            speedValue *= Sprinting ? 1.5f : 1;  // Sprint Boost
            
            speedValue *= WalkingTime;        // Accelerate Hinder
            speedValue *= SkidFactor;         // Skid Hinder
            speedValue *= Crouching ? 0 : 1;  // Crouch Hinder

            _speed = speedValue;
            return speedValue;
        }

        _speed = Speed/2 * WalkingTime;
        return _speed;
    }


    float CalculateMaxSpeed()
    {
        float maxspeedValue = _maxSpeed;
        if(Grounded) _maxSpeed = MaxSpeed;

        return _maxSpeed;
    }


    private void CalculateCorrectiveMovement()
    {
        // Slope Correction
        CorrectMovement = Movement;
        if (Physics.Raycast(transform.position + (Vector3.down * 0.9f), Vector3.down, out RaycastHit hit, 1f))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= 45 && !HasJumped)
            {
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Movement, hit.normal).normalized;
                CorrectMovement = new Vector3(slopeDirection.x, Mathf.Clamp(slopeDirection.y, -0.5f, 0.2f), slopeDirection.z);

                Tools.DrawThickRay(hit.point, rb.linearVelocity, Color.red, 0, 0.001f);
                Tools.DrawThickRay(hit.point, CorrectMovement*10, Color.green, 0, 0.001f);
            }
        }
    }


    private void CalculateSlopeStickForce()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position+(Vector3.down*0.9f), Vector3.down, out hit, 1.5f))
        {
            slopeVector = hit.normal;
            slopeAngle = Vector3.Angle(slopeVector, Vector3.up);

            if (slopeAngle > 0 && slopeAngle <= 45 && !HasJumped)
            {
                // Calculate downward force based on the slope angle
                slopeCorrectionVector = Vector3.ProjectOnPlane(Vector3.down, slopeVector).normalized*-1;
                Debug.DrawRay(hit.point, slopeCorrectionVector * 3, Color.white);
                rb.AddForce(slopeCorrectionVector * (slopeAngle/(SlopeSlipperyness/100) - (slopeAngle-1.3f)), ForceMode.Force);
            }
        }
        // Overshoot
        if(!Grounded && !HasJumped && rb.linearVelocity.y > 0 && slopeAngle < 45)
        {
            if (Physics.Raycast(transform.position, Vector3.down, 1f))
                rb.AddForce(Vector3.down*200, ForceMode.Acceleration);
        }
    }


    //***********************************************************************
    //***********************************************************************
    //Extra Logic

    public void Pause(bool State)
    {
        Paused = State;
        CanMove = !State;

        if(State)
        {
            MovementX = 0;
            MovementY = 0;
        }
    }

    public void LockToMaxSpeed()
    {
        // Get the velocity direction
        Vector3 newVelocity = rb.linearVelocity;
        newVelocity.y = 0f;
        newVelocity = Vector3.ClampMagnitude(newVelocity, CalculateMaxSpeed());
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }


    public void SetGrounded(bool state) 
    {
        Grounded = state;
    }

    public bool WalkingCheck()
    {
        if(MovementX != 0 || MovementY != 0)
        {
            Walking = true;
            if(Grounded) return true;
            else return false;
        }
        else
        {
            Walking = false;
            return false;
        }
    }
    public bool RunningCheck()
    {
        return Running && Walking && rb.linearVelocity.magnitude > 1;
    }

    [Button(DisplayParameters = true, Style = ButtonStyle.FoldoutButton)]
    public void Teleport(Transform newTransform)
    {
        rb.position = newTransform.position;
        CinemachineCamera cinemachine = FindAnyObjectByType<CinemachineCamera>();
        CinemachineOrbitalFollow pov = cinemachine.GetComponent<CinemachineOrbitalFollow>();

        pov.VerticalAxis.Value   = newTransform.eulerAngles.x;
        pov.HorizontalAxis.Value = newTransform.eulerAngles.y;
    }
}
