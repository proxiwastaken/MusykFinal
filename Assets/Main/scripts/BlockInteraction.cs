using UnityEngine;

using TMPro;

public class BlockInteraction : MonoBehaviour
{
    private bool isActive = false;
    private Renderer blockRenderer;
    private Light blockLight;

    public bool IsActive => isActive;

    // Cache the mode so we can detect changes.
    private InteractionMode cachedMode;

    public void Setup(int row, int col, System.Action<int, int, bool> onToggled)
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer == null)
            Debug.LogError("Renderer missing on block!");
        else
            blockRenderer.material.color = Color.white;

        blockLight = gameObject.AddComponent<Light>();
        blockLight.type = LightType.Point;
        blockLight.color = Color.white;
        blockLight.intensity = 0f;
        blockLight.range = 1f;

        if (!TryGetComponent<BoxCollider>(out _))
        {
            gameObject.AddComponent<BoxCollider>();
        }

        // Initialize the event listeners.
        UpdateEventListeners();

        // Cache the current mode.
        cachedMode = InteractionModeManager.currentMode;
    }

    void Update()
    {
        // Check if the interaction mode has changed.
        if (InteractionModeManager.currentMode != cachedMode)
        {
            // Reinitialize event listeners based on the new mode.
            UpdateEventListeners();
            cachedMode = InteractionModeManager.currentMode;
            Debug.Log("BlockInteraction updated event listeners to mode: " + cachedMode);
        }
    }

    void UpdateEventListeners()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogWarning("No XRInteractable component found on block.");
            return;
        }

        // Clear previous listeners.
        interactable.activated.RemoveAllListeners();
        interactable.selectEntered.RemoveAllListeners();

        switch (InteractionModeManager.currentMode)
        {
            case InteractionMode.TriggerOnly:
                // Use onActivate for trigger-only mode.
                interactable.activated.AddListener((args) => { ToggleBlock(); });
                break;
            case InteractionMode.RayGrab:
                // Use selectEntered for ray-grab mode.
                interactable.selectEntered.AddListener((args) => { ToggleBlock(); });
                break;
            case InteractionMode.DirectTouch:
                // For DirectTouch, we rely on collision events.
                break;
        }
    }

    public void ToggleBlock()
    {
        isActive = !isActive;
        blockRenderer.material.color = isActive ? Color.green : Color.white;
        blockLight.intensity = isActive ? 2f : 0f;
    }

    public void SetHighlight(bool highlight)
    {
        if (blockLight == null)
        {
            Debug.LogWarning("Block light is null in SetHighlight.");
            return;
        }

        if (highlight)
        {
            blockLight.color = Color.yellow;
            blockLight.intensity = 3f;
            blockRenderer.material.color = Color.yellow;
        }
        else
        {
            blockLight.color = isActive ? Color.green : Color.white;
            blockLight.intensity = isActive ? 2f : 0f;
            blockRenderer.material.color = isActive ? Color.green : Color.white;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (InteractionModeManager.currentMode == InteractionMode.DirectTouch)
        {
            ToggleBlock();
        }
    }
}
