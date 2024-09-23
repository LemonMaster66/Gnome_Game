using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class PlayerSFX : AudioManager
{
    [Space(10)]

    [Tab("Physics")]
    public float TimeBetweenSteps;
    public float stepTimer;
    [Space(10)]
    public AudioClip[] WalkStep;
    public AudioClip[] RunStep;
    [Space(5)]
    public AudioClip[] Jump;
    public AudioClip[] Land;



    [Tab("Stats")]
    public AudioClip StartSFX;
    public AudioClip Damage;
    public AudioClip Death;
    [Space(5)]
    public AudioClip[] ObtainCollectableSFX;


    private PlayerMovement PM;
    private PlayerStats    PS;
    private GroundCheck groundCheck;


    void Awake()
    {
        PM = FindAnyObjectByType<PlayerMovement>();
        PS = FindAnyObjectByType<PlayerStats>();
        groundCheck = GetComponentInChildren<GroundCheck>();
    }


    void FixedUpdate()
    {
        if(PS.Dead || PM.Paused)
        {
            TimeBetweenSteps = 100;
            return;
        }

        if(PM.WalkingCheck()) stepTimer -= Time.deltaTime;
        if(stepTimer < 0)
        {
            if(!PM.Running) PlayRandomSound(WalkStep, PM.Crouching ? 0.3f:0.75f, 1, 0.2f);
            else            PlayRandomSound(RunStep,  0.75f, 1, 0.2f);
            stepTimer = TimeBetweenSteps;
        }

        if(PM.Running)        TimeBetweenSteps = 0.3f;
        else if(PM.Crouching) TimeBetweenSteps = 0.65f;
        else                  TimeBetweenSteps = 0.4f;

        stepTimer = (float)Math.Round(stepTimer, 2);
    }
}

