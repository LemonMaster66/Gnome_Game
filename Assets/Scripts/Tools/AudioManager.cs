using UnityEngine;
using VInspector;

public class AudioManager : MonoBehaviour
{
    public GameObject AudioPrefab;

    public void PlaySound(AudioClip audioClip, float Volume = 1, float Pitch = 1, float PitchVariation = 0, bool Loop = false)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, transform.position, Quaternion.identity, transform);
        AudioObj.name = audioClip.name;

        AudioSource audioSource = AudioObj.GetComponent<AudioSource>();
        audioSource.clip   =  audioClip;
        audioSource.volume =  Volume;
        audioSource.pitch  =  Pitch + Random.Range(-PitchVariation, PitchVariation);
        audioSource.loop   =  Loop;

        audioSource.Play();
        if(!Loop) Destroy(AudioObj, audioClip.length);
    }


    public void PlayRandomSound(AudioClip[] audioClip, float Volume = 1, float Pitch = 1, float PitchVariation = 0, bool Loop = false)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, transform.position, Quaternion.identity, transform);

        AudioSource audioSource = AudioObj.GetComponent<AudioSource>();
        AudioClip RandomClip = audioClip[Random.Range(0, audioClip.Length)];

        AudioObj.name = RandomClip.name;

        audioSource.clip     = RandomClip;
        audioSource.volume   = Volume;
        audioSource.pitch    = Pitch + Random.Range(-PitchVariation, PitchVariation);

        audioSource.Play();
        Destroy(AudioObj, RandomClip.length);
    }

    public void StopSound(AudioClip audioClip)
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach(AudioSource audioSource in audioSources)
        {
            if(audioSource.clip == audioClip) Destroy(audioSource.gameObject);
        }
    }

    public void SetValues(AudioClip audioClip, float Volume = 1, float Pitch = 1, float PitchVariation = 0, bool Loop = false)
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach(AudioSource audioSource in audioSources)
        {
            if(audioSource.clip == audioClip)
            {
                audioSource.volume   = Volume;
                audioSource.pitch    = Pitch + Random.Range(-PitchVariation, PitchVariation);
            }
        }
    }

    public bool isPlaying(AudioClip audioClip)
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach(AudioSource audioSource in audioSources)
        {
            if(audioSource.clip == audioClip) return true;
        }
        return false;
    }
}

