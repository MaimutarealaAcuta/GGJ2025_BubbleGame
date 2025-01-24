using UnityEngine;

public class WindowedModeOnStart : MonoBehaviour
{
    // Windowed resolution, change these to your desired window size
    public int windowWidth = 854;
    public int windowHeight = 480;

    void Start()
    {
#if !UNITY_WEBGL
        // Only run this code for standalone builds (not WebGL)
        // Set the game to windowed mode
        Screen.fullScreenMode = FullScreenMode.Windowed;

        // Set the resolution of the window
        Screen.SetResolution(windowWidth, windowHeight, false); // 'false' means windowed mode
#endif

        // Confine the cursor to the window and keep it visible
        Cursor.lockState = CursorLockMode.Confined;  // Keeps the cursor within the window bounds
        Cursor.visible = true;  // Keep the cursor visible
    }
}