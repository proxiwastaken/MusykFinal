using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class SimpleHelpButton : MonoBehaviour
{
    public GameObject helpPanel3DPrefab;
    [TextArea] public string helpText = "Help text…";
    public float displayDuration = 5f;

    XRSimpleInteractable simpleInteractable;
    GameObject panelInst;
    Coroutine hideCoroutine;

    void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        if (simpleInteractable == null)
            Debug.LogError("No XRSimpleInteractable on " + name);
        else
        {
            Debug.Log("Subscribing to activate on " + name);
            simpleInteractable.activated.AddListener(OnActivated);
        }
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
            simpleInteractable.activated.RemoveListener(OnActivated);
    }

    void OnActivated(ActivateEventArgs args)
    {
        Debug.Log(name + " was activated! Showing help.");
        ShowHelp();
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    void ShowHelp()
    {
        if (panelInst == null)
        {
            panelInst = Instantiate(helpPanel3DPrefab, transform);
            panelInst.transform.localPosition = Vector3.up * 1.9f;
            panelInst.transform.localRotation = Quaternion.identity;
        }
        var tmp = panelInst.GetComponentInChildren<TMPro.TextMeshPro>();
        if (tmp != null) tmp.text = helpText;
        panelInst.SetActive(true);
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (panelInst != null)
            panelInst.SetActive(false);
    }
}
