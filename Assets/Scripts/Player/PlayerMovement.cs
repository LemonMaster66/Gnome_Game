using System;
using PalexUtilities;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class PlayerMovement : MonoBehaviour
{
    [Tab("Main")]
    public float Speed            = 50;
    public float MaxSpeed         = 80;
    public float CounterMovement  = 10;
    public float JumpForce        = 8;
    public float Gravity          = 100;


    [Header("States")]
    public bool  Grounded       = true;
    public bool  Crouching      = false;
    public bool  Running        = false;

    public bool  CanMove        = true;
    public bool  Paused         = false;

    public bool  HasJumped      = false;
    public bool  HoldingCrouch  = false;
    public bool  HoldingRun     = false;


    [Header("Extras")]
    public float extraSpeed;


    #region Debug Stats
        [Tab("Settings")]
        public Vector3     TargetScale;
        public Vector3     PlayerVelocity;
        public Vector3     SmoothVelocity;
        public float       VelocityMagnitude;
        public float       ForwardVelocityMagnitude;
        public Vector3     VelocityXZ;
        [Space(5)]
        public Vector3 CamF;
        public Vector3 CamR;
        [Space(5)]
        public Vector3 Movement;
        public float   MovementX;
        public float   MovementY;
        [Space(8)]
        public float CoyoteTime;
        public float JumpBuffer;
        [Space(8)]
        public float   _speed;
        public float   _maxSpeed;
        public float   _gravity;
    #endregion
    
    
    #region Script / Component Reference
        [HideInInspector] public Rigidbody    rb;
        [HideInInspector] public Transform    Camera;

        private PlayerStats  playerStats;
        private PlayerSFX    playerSFX;
        private GroundCheck  groundCheck;
    #endregion


    void Awake()
    {
        //Assign Components
        Camera  = GameObject.Find("Main Camera").transform;
        rb      = GetComponent<Rigidbody>();

        //Assign Scripts
        playerSFX    = FindAnyObjectByType<PlayerSFX>();
        playerStats  = GetComponent<PlayerStats>();
        groundCheck  = GetComponentInChildren<GroundCheck>();

        //Component Values
        rb.useGravity = false;

        //Property Values
        _maxSpeed  = MaxSpeed;
        _speed     = Speed;
        _gravity   = Gravity;
        TargetScale.y = 1.5f;
    }


    void Update()
    {
        if(CoyoteTime > 0) CoyoteTime = Math.Clamp(CoyoteTime - Time.deltaTime, 0, 100);
        if(JumpBuffer > 0) JumpBuffer = Math.Clamp(JumpBuffer - Time.deltaTime, 0, 100);
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
            #endregion

            SmoothVelocity = Vector3.Slerp(SmoothVelocity, rb.linearVelocity, 0.15f);
            SmoothVelocity.x    = (float)Math.Round(SmoothVelocity.x, 4);
            SmoothVelocity.y    = (float)Math.Round(SmoothVelocity.y, 4);
            SmoothVelocity.z    = (float)Math.Round(SmoothVelocity.z, 4);

            //Gravity
            rb.AddForce(Physics.gravity * Gravity /10);

            LockToMaxSpeed();
        #endregion
        //**********************************

        transform.localScale = Vector3.Slerp(transform.localScale, TargetScale, 0.175f);

        // Movement Code
        if(!Paused && !playerStats.Dead && CanMove)
        {
            Movement = (CamF * MovementY + CamR * MovementX).normalized;
            rb.AddForce(Movement * Speed);
        }

        //CounterMovement
        rb.AddForce(VelocityXZ * -(CounterMovement / 10));

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

        //if(MovementX == 0 && MovementY == 0) playerSFX.stepTimer = 0;
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, math.clamp(rb.linearVelocity.y, 0, math.INFINITY), rb.linearVelocity.z);
        rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);

        CrouchState(false);

        playerSFX.PlayRandomSound(playerSFX.Jump, 1, 1, 0.15f);
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
            Speed = _speed * 1.6f + extraSpeed;
        }
        else
        {
            Running = false;
            if(!Crouching) Speed = _speed + extraSpeed;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started && !Crouching) 
        {
            HoldingCrouch = true;
            CrouchState(true);
        }
        if(context.canceled && Crouching)
        {
            HoldingCrouch = false;
            CrouchState(false);
        }
    }
    public void CrouchState(bool state)
    {
        if(state)
        {
            RunState(false);

            Crouching = true;
            TargetScale.y = 0.75f;
            Speed = 50;
        }
        else
        {
            Crouching = false;
            TargetScale.y = 1.5f;
            Speed = _speed;

            if(HoldingRun) RunState(true);
        }
    }


    //***********************************************************************
    //***********************************************************************
    //Extra Logic

    // public void OnPause(InputAction.CallbackContext context)
    // {
    //     if(context.started)
    //     {
    //         Pause(bool State)

    //         Debug.Log("InputLock = " + Paused);
    //     }
    // }


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
        newVelocity = Vector3.ClampMagnitude(newVelocity, MaxSpeed);
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
            if(Grounded) return true;
            else return false;
        }
        else return false;
    }

    public void Teleport(Transform newTransform)
    {
        rb.position = newTransform.position;
        CinemachineVirtualCamera cinemachine = FindAnyObjectByType<CinemachineVirtualCamera>();
        CinemachinePOV pov = cinemachine.GetCinemachineComponent<CinemachinePOV>();

        pov.m_VerticalAxis.Value   = newTransform.eulerAngles.x;
        pov.m_HorizontalAxis.Value = newTransform.eulerAngles.y;
    }
}
