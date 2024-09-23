using PalexUtilities;
using TMPro;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private bool Collected;
    public GameObject collectParticle;
    public GameObject collectTextUI;

    void OnTriggerEnter(Collider collider)
    {
        if(Collected || collider.tag != "Player") return;
        Collected = true;

        Destroy(gameObject);
    }
}
