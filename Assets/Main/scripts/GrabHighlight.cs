using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class GrabHighlight : MonoBehaviour
{
    [Tooltip("Emission color while grabbed.")]
    public Color highlightEmission = Color.yellow;
    [Tooltip("Strength of the emission.")]
    public float emissionIntensity = 2f;

    // We'll cache all renderers & their original emission colors
    private Renderer[] renderers;
    private Dictionary<Material, Color> originalEmissive = new Dictionary<Material, Color>();
    private XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        renderers = GetComponentsInChildren<Renderer>();

        // Cache each material's original _EmissionColor
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                // Ensure the keyword is on so we can read/write emission
                mat.EnableKeyword("_EMISSION");
                originalEmissive[mat] = mat.GetColor("_EmissionColor");
            }
        }
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
        interactable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Turn up the emission color
        Color boosted = highlightEmission * emissionIntensity;
        foreach (var kv in originalEmissive)
        {
            var mat = kv.Key;
            mat.SetColor("_EmissionColor", boosted);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Restore each material's original emission color
        foreach (var kv in originalEmissive)
        {
            var mat = kv.Key;
            mat.SetColor("_EmissionColor", kv.Value);
        }
    }
}
