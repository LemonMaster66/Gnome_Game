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
        player._maxSpeed = Math.Clamp(player.VelocityXZ.magnitude, 7, player._maxSpeed);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player.gameObject) return;
        player.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
    }
}