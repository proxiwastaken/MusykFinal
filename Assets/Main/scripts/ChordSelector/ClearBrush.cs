using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class ClearBrush : MonoBehaviour
{
    [Tooltip("Panel to return to")]
    public Transform soundPanel;

    public float returnDelay = 3f;

    Rigidbody rb;
    XRGrabInteractable grab;
    Vector3 initialLocalPos;
    Quaternion initialLocalRot;
    bool isHeld = false;
    bool returnDisabled = false;
    Coroutine returnCoroutine;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // start locked in place
        rb.useGravity = false;
        rb.isKinematic = true;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void Start()
    {
        // record palette position
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        // detach from panel if parented
        if (transform.parent == soundPanel) transform.SetParent(null);

        // stop any pending return
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        // allow physics so user can fling it if they want
        rb.useGravity = true;
        rb.isKinematic = false;

        if (!returnDisabled)
            returnCoroutine = StartCoroutine(ReturnToPalette());
    }

    IEnumerator ReturnToPalette()
    {
        yield return new WaitForSeconds(returnDelay);

        if (!isHeld && !returnDisabled)
        {
            // snap back under the panel
            if (soundPanel != null && transform.parent != soundPanel)
                transform.SetParent(soundPanel);

            rb.useGravity = false;
            rb.isKinematic = true;

            // smoothly lerp back
            float duration = 1f, t = 0f;
            Vector3 startPos = transform.localPosition;
            Quaternion startRot = transform.localRotation;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(startPos, initialLocalPos, t / duration);
                transform.localRotation = Quaternion.Slerp(startRot, initialLocalRot, t / duration);
                yield return null;
            }
            transform.localPosition = initialLocalPos;
            transform.localRotation = initialLocalRot;
        }
    }
}
