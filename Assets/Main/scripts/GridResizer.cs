using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class GridResizer : MonoBehaviour
{
    [Tooltip("Reference to the VRSequencer script.")]
    public VRSequencer sequencer;

    [Tooltip("Spacing between blocks in the grid (should match VRSequencer).")]
    public float spacing = 0.2f;

    [Tooltip("Minimum and maximum grid dimensions.")]
    public int minRows = 1;
    public int minColumns = 1;
    public int maxRows = 16;
    public int maxColumns = 32;

    [Tooltip("Duration for the resizer to snap to the new position after release.")]
    public float snapDuration = 0.5f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 gridAnchor; // Bottom-left corner of the grid in world space

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("GridResizer requires an XRGrabInteractable component.");
        }
        // We want the handle to float and not be affected by physics.
        // Ensure it has a Rigidbody that is kinematic and with gravity disabled.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        if (sequencer == null)
        {
            sequencer = FindObjectOfType<VRSequencer>();
        }
        // Calculate the grid's bottom-left corner in world space.
        gridAnchor = sequencer.transform.position + sequencer.gridStartPosition;
        // Position the resizer at the top-right corner of the current grid.
        Vector3 initialPos = gridAnchor + new Vector3((sequencer.columns - 1) * spacing, -(sequencer.rows - 1) * spacing, 0);
        transform.position = initialPos;
    }

    // When the user releases the resizer, update the grid dimensions.
    void OnSelectExited(SelectExitEventArgs args)
    {
        // Get the current position of the resizer handle.
        Vector3 currentPos = transform.position;
        // Calculate the offset from the grid anchor.
        float deltaX = currentPos.x - gridAnchor.x;
        float deltaY = gridAnchor.y - currentPos.y; // grid extends downward (Y decreases)

        // Calculate new dimensions:
        int newColumns = Mathf.Clamp(Mathf.FloorToInt(deltaX / spacing) + 1, minColumns, maxColumns);
        int newRows = Mathf.Clamp(Mathf.FloorToInt(deltaY / spacing) + 1, minRows, maxRows);

        // Resize the grid.
        if (sequencer != null)
        {
            sequencer.ResizeGrid(newRows, newColumns);
            // Compute the new top-right corner position based on the new grid dimensions.
            Vector3 newHandlePos = gridAnchor + new Vector3((newColumns - 1) * spacing, -(newRows - 1) * spacing, 0);
            // Animate snapping the resizer to the new position.
            StartCoroutine(SnapToPosition(newHandlePos));
        }

        Debug.Log("Resizer released. New grid: " + newRows + " rows, " + newColumns + " columns.");
    }

    IEnumerator SnapToPosition(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / snapDuration);
            yield return null;
        }
        transform.position = targetPos;
    }
}
