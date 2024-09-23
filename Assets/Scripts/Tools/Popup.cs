using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VInspector;

public class Popup : MonoBehaviour
{
    public bool Decay;
    public bool PunchOnAwake;
    public bool BillboardRotate;

    public float DecayAfter;
    public float Age;

    private Transform cam;
    private Animator animator;

    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
        cam      = Camera.main.transform;

        if(PunchOnAwake) Punch();
    }

    public virtual void LateUpdate()
    {
        if(Decay)
        {
            Age += Time.deltaTime;
            if(Age >= DecayAfter) animator.Play("Fade");
        }

        Vector3 rot = transform.eulerAngles;
        if(BillboardRotate) transform.LookAt(transform.position + cam.forward);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, rot.z);
    }

    public virtual void DestroyObj()
    {
        transform.DOComplete();
        Destroy(gameObject);
    }
    
    [Button]
    public virtual void Punch()
    {
        DOTween.Complete(true);
        transform.DOPunchRotation(new Vector3(0,0,25), 0.2f, UnityEngine.Random.Range(15, 30), 1);
        transform.DOPunchScale(new Vector3(0.1f,0.1f,0.1f), 0.2f, UnityEngine.Random.Range(15, 30), 1);
    }
}
