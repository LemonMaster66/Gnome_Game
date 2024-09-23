using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public GameObject Target;

    [Space(10)]

    public bool Position;
    public bool Rotation;
    public bool Scale;

    [Space(10)]

    [Range(0,1)] public float PSmooth = 1;
    [Range(0,1)] public float RSmooth = 1;
    [Range(0,1)] public float SSmooth = 1;

    private Vector3    Poffset;
    private Quaternion Roffset;
    private Vector3    Soffset;

    void Start()
    {
        Poffset = transform.position - Target.transform.position;
        Roffset = Quaternion.Inverse(Target.transform.rotation) * transform.rotation;
        Soffset = transform.localScale - Target.transform.localScale;
    }

    void LateUpdate()
    {
        if (Position) transform.position   = Vector3.Slerp   (transform.position,   Target.transform.position   + Poffset, PSmooth);
        if (Rotation) transform.rotation   = Quaternion.Slerp(transform.rotation,   Target.transform.rotation   * Roffset, RSmooth);
        if (Scale)    transform.localScale = Vector3.Slerp   (transform.localScale, Target.transform.localScale + Soffset, SSmooth);
    }
}