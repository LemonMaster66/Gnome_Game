using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public AudioClip[] steps;

    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GetComponent<AudioManager>();
    }
    
}
