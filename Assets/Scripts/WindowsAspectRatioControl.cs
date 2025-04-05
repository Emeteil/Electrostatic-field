#if UNITY_STANDALONE_WIN
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class WindowsAspectRatioControl : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    private struct RECT { public int left, top, right, bottom; }

    private const float TARGET_ASPECT = 16f / 9f;
    private IntPtr windowHandle;

    void Start()
    {
        windowHandle = GetActiveWindow();
    }

    void Update()
    {
        if (Screen.fullScreen) return;

        GetClientRect(windowHandle, out RECT clientRect);
        int clientWidth = clientRect.right - clientRect.left;
        int clientHeight = clientRect.bottom - clientRect.top;

        float currentAspect = (float)clientWidth / clientHeight;

        if (Mathf.Abs(currentAspect - TARGET_ASPECT) > 0.01f)
        {
            GetWindowRect(windowHandle, out RECT windowRect);
            int windowWidth = windowRect.right - windowRect.left;
            int windowHeight = windowRect.bottom - windowRect.top;

            int nonClientWidth = windowWidth - clientWidth;
            int nonClientHeight = windowHeight - clientHeight;

            int newClientHeight = Mathf.RoundToInt(clientWidth / TARGET_ASPECT);
            int newWindowHeight = newClientHeight + nonClientHeight;

            SetWindowPos(windowHandle, 0, windowRect.left, windowRect.top, windowWidth, newWindowHeight, 0x0040);
        }
    }
}
#endif