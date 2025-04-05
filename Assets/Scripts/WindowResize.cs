using UnityEngine;

public class WindowResize : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}