using Unity.Cinemachine;
using PalexUtilities;
using UnityEngine;
using VInspector;

public class CameraFX : MonoBehaviour
{
    public float TargetDutch;
    private float BlendDutch;

    public float TargetFOV;
    private float BlendFOV;

    public float TargetShakeAmplitude;
    private float BlendShakeAmplitude;
    public float TargetShakeFrequency;
    private float BlendShakeFrequency;

    [Space(10)]

    public GameObject LookingTarget;
    public GameObject LookingTargetStorage;


    [Foldout("Cinemachine stuff")]
    public Camera cam;
    public CinemachineVirtualCamera VirtualCamera;
    public CinemachineInputProvider InputProvider;
    public CinemachineBasicMultiChannelPerlin PerlinShake;
    public CinemachineImpulseSource ImpulseSource;
    public CinemachineImpulseListener ImpulseListener;
    public CinemachinePOV POV;
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;

    void Awake()
    {
        //Assign Components
        cam = Camera.main;
        VirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        PerlinShake = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        POV = VirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        InputProvider = GetComponent<CinemachineInputProvider>();
        ImpulseListener = GetComponent<CinemachineImpulseListener>();

        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    void Update()
    {
        InputProvider.enabled = true;
        
        if(playerStats.Dead || playerMovement.Paused)
        {
            VirtualCamera.m_Lens.Dutch = 0;
            VirtualCamera.m_Lens.FieldOfView = 60;
            PerlinShake.AmplitudeGain = 0;
            PerlinShake.FrequencyGain = 0;
            return;
        }


        //Dutch Tilt + Field Of View
        VirtualCamera.m_Lens.Dutch = Mathf.SmoothDamp(VirtualCamera.m_Lens.Dutch, TargetDutch, ref BlendDutch, 0.1f);
        VirtualCamera.m_Lens.FieldOfView = Mathf.SmoothDamp(VirtualCamera.m_Lens.FieldOfView, TargetFOV, ref BlendFOV, 0.2f);
        TargetDutch = playerMovement.MovementX * -2f;


        //Footstep
        PerlinShake.AmplitudeGain = Mathf.SmoothDamp(PerlinShake.AmplitudeGain, TargetShakeAmplitude, ref BlendShakeAmplitude, 0.1f);
        PerlinShake.FrequencyGain = Mathf.SmoothDamp(PerlinShake.FrequencyGain, TargetShakeFrequency, ref BlendShakeFrequency, 0.1f);

        
        //Land Force
        if(!playerMovement.Crouching) ImpulseSource.DefaultVelocity.y = 0.25f;
        else ImpulseSource.DefaultVelocity.y = 0.15f;


        //Footstep Shake Conditions
        if(playerMovement.WalkingCheck())
        {
            if(playerMovement.Running)
            {
                TargetShakeAmplitude = 4f;
                TargetShakeFrequency = 0.05f;
            }
            else if(playerMovement.Crouching)
            {
                TargetShakeAmplitude = 1f;
                TargetShakeFrequency = 0.03f;
            }
            else
            {
                TargetShakeAmplitude = 3f;
                TargetShakeFrequency = 0.04f;
            }
        }
        else
        {
            TargetShakeAmplitude = 0f;
            TargetShakeFrequency = 0f;
        } 
    }

    public void Die()
    {
        VirtualCamera.enabled = false;
        TargetShakeAmplitude = 0f;
        TargetShakeFrequency = 0f;
    }
}
