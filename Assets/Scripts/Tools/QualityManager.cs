using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class QualityManager : MonoBehaviour
{
    private Volume PostProcessing;

    void Awake()
    {
        PostProcessing = GetComponent<Volume>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Hash)) ToggleMotionBlur();
    }

    public void ChangeQualitySettings(int Index)
    {
        Index--;
        QualitySettings.SetQualityLevel(Index, true);
    }

    public void ToggleMotionBlur()
    {
        PostProcessing.profile.TryGet(out MotionBlur motionBlur);
        
        if(motionBlur.intensity.value == 0.7f)
        {
            Debug.Log("Motion Blur Disabled");
            motionBlur.intensity.value = 0;
        }
        else
        {
            Debug.Log("Motion Blur Enabled");
            motionBlur.intensity.value = 0.7f;
        }
        
    }
}
