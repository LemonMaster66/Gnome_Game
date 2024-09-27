using UnityEngine;

public class ExtraGravity : MonoBehaviour
{
    private Rigidbody rb;
    public float gravity;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravity /10);
    }
}


