using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private PlayerMovement player;
    private PlayerSFX      playerSFX;
    private CameraFX       cameraFX;

    public GameObject GroundObject;
    public bool Grounded;

    void Awake()
    {
        //Assign Components
        player = GetComponentInParent<PlayerMovement>();
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        cameraFX       = FindAnyObjectByType<CameraFX>();
    }

    void Update()
    {
        //CheckStep();
    }

    // public void CheckStep()
    // {
    //     RaycastHit hit;
    //     float stepHeight = 0.5f;
    //     Vector3 FloorPos = player.transform.position - new Vector3(0, player.transform.localScale.y, 0);
    //     Vector3 StartPos = FloorPos + player.Movement + Vector3.up*stepHeight;

    //     DebugPlus.DrawWireSphere(FloorPos, 0.05f);
    //     DebugPlus.DrawWireSphere(StartPos, 0.05f);

    //     if (Physics.Raycast(StartPos, Vector3.down, out hit, 2f))
    //     {
    //         float distanceFromFloor = (FloorPos.y - hit.point.y) *-1;

    //         //Debug.Log("Step: " + Math.Round(distanceFromFloor, 3));

    //         if(Vector3.Dot(player.slopeVector, hit.normal) < 0.3f) return;
            
    //         //Step Up
    //         if (Grounded && !player.HasJumped && distanceFromFloor > 0)
    //         {
    //             Debug.DrawRay(StartPos, Vector3.down/2, Color.green);
    //             player.stepClimb(distanceFromFloor);
    //         }
    //         //Step Down
    //         if(Grounded && !player.HasJumped && distanceFromFloor < 0)
    //         {
    //             Debug.DrawRay(StartPos, Vector3.down/2, Color.green);
    //             player.stepClimb(distanceFromFloor/2);
    //         }
    //     }
    // }

    public bool CheckGround()
    {
        if(GroundObject != null) return true;
        else return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject) return;
        player.SetGrounded(true);
        Grounded = true;

        if(GroundObject == null)
        {
            if(player.JumpBuffer > 0) player.Jump();
            else 
            {
                player.HasJumped = false;
                if(player.MovementX == 0 && player.MovementY == 0 && player.slopeAngle <= 45)
                {
                    player.rb.linearVelocity = player.Movement;
                }
            }
        }
        
        GroundObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player.gameObject) return;
        player.SetGrounded(false);
        GroundObject = null;
        Grounded = false;

        player.CoyoteTime = 0.3f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player.gameObject) return;
        player.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
    }
}