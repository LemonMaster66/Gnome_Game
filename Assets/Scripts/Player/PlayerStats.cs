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
    public Image _damageScreen;
    public Image _deathScreen;
    public GameObject textPopup;



    void Awake()
    {
        //Assign Scripts
        playerMovement = GetComponent<PlayerMovement>();
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        cameraFX       = FindAnyObjectByType<CameraFX>();

        GameObject canvas = GameObject.Find("Canvas");
        if(canvas != null) _damageScreen = canvas.transform.GetChild(0).GetComponent<Image>();
        if(canvas != null) _deathScreen  = canvas.transform.GetChild(1).GetComponent<Image>();
    }

    void Update()
    {
        if(_damageScreen != null) _damageScreen.color = new Color(1,1,1, !Dead ? (Health / 80*-1)+1 : 0);
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
        _deathScreen.color = new Color(1,1,1,1);
        cameraFX.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-8);
        playerSFX.StopSound(playerSFX.Damage);
        playerSFX.PlaySound(playerSFX.Death);
    }


    public void SpawnTextUI(string text)
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject MoneyTextObj = Instantiate(textPopup, canvas.transform.position, Quaternion.identity, canvas.transform);
        MoneyTextObj.GetComponent<TextMeshProUGUI>().text = text;
        MoneyTextObj.transform.position = new Vector3(50, 25, 0);
    }
}
