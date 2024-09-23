using UnityEngine;

public class DemoText : MonoBehaviour
{
    public KeyCode[] keyCodes;

    void Update()
    {
        foreach(KeyCode keyCode in keyCodes)
        {
            if(Input.GetKeyDown(keyCode)) Destroy(gameObject);
        }
    }
}
