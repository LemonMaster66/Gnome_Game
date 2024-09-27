using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerSFX      playerSFX;
    private CameraFX       cameraFX;

    public GameObject GroundObject;
    public bool Grounded;

    void Awake()
    {
        //Assign Components
        playerMovement = GetComponentInParent<PlayerMovement>();
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
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(true);
        Grounded = true;

        if(GroundObject == null)
        {
            if(playerMovement.JumpBuffer > 0) playerMovement.Jump();
            else 
            {
                playerMovement.HasJumped = false;
                if(playerMovement.MovementX == 0 && playerMovement.MovementY == 0)
                {
                    playerMovement.rb.linearVelocity = playerMovement.Movement;
                }
            }
        }
        
        GroundObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(false);
        GroundObject = null;
        Grounded = false;

        playerMovement.CoyoteTime = 0.3f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
    }
}