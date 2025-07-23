using UnityEngine;
using UnityEngine.SceneManagement;

// Define the interaction modes.
public enum InteractionMode
{
    RayGrab,       // Use a ray and grab.
    TriggerOnly,   // Use only the trigger while pointing with a ray.
    DirectTouch    // Use grab and trigger
}

public class InteractionModeManager : MonoBehaviour
{
    // The current mode is available as a static field for easy access.
    public static InteractionMode currentMode = InteractionMode.DirectTouch;

    // This OnGUI method is for debugging; it will appear in the Game view (and in VR if you have a window).
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 220, 60), "Debug Controls");
        if (GUI.Button(new Rect(20, 40, 200, 25), "Reset Scene"))
        {
            // Cycle through the available modes.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
