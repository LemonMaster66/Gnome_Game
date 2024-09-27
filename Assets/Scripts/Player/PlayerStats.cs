using System;
using Unity.Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class PlayerStats : MonoBehaviour
{
    [Tab("Main")]
    [Header("Properties")]
    public float Health = 100;

    [Header("States")]
    public bool Dead = false;


    [Tab("Settings")]
    public PlayerMovement playerMovement;
    public PlayerSFX      playerSFX;
    public CameraFX       cameraFX;



    void Awake()
    {
        //Assign Scripts
        playerMovement = GetComponent<PlayerMovement>();
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        cameraFX       = FindAnyObjectByType<CameraFX>();
    }

    void Update()
    {
        
    }


    public void TakeDamage(float Damage = 100)
    {
        Health -= Damage;
        cameraFX.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(Damage/10f);
        playerSFX.PlaySound(playerSFX.Damage, 1, 1, 0.1f);
        if(Health <= 0) Die();
    }
    public void LoseHealth(float Amount = 100)
    {
        Health -= Amount;
        cameraFX.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(Amount/10f);
        if(Health <= 0) Die();
    }
    public void Die()
    {
        Dead = true;
        cameraFX.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-8);
        playerSFX.StopSound(playerSFX.Damage);
        playerSFX.PlaySound(playerSFX.Death);
    }
}
