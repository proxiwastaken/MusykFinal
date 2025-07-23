using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    /// <summary>
    /// Reloads the currently active scene, completely resetting all GameObjects.
    /// </summary>
    public void ReloadScene()
    {
        var scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }
}
