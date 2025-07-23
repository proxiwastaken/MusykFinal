using TMPro;
using UnityEngine;

[System.Serializable]
public class SoundCategory
{
    public string categoryName;
    public AudioClip[] audioClips;
}
public class SoundPanelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject soundItemPrefab;      // Your SoundItem prefab (a 0.1-scale cube)
    public GameObject categoryHeaderPrefab; // A 3D Text prefab for displaying category names

    [Header("Sound Categories")]
    public SoundCategory[] soundCategories; // Set up via Inspector; e.g., "crashes", "hats", "kicks", "snares", "weird"

    [Header("Layout Settings")]
    public float spacing = 0.15f;     // Spacing between items
    public int itemsPerRow = 4;       // How many items per row
    public float headerSpacing = 0.2f; // Vertical space for category header text
    public float categorySpacing = 0.3f; // Extra spacing between categories

    void Start()
    {
        PopulatePanel();
    }

    void PopulatePanel()
    {
        // We'll lay out items vertically from the top down.
        // currentY starts at 0 and then decreases for each category.
        float currentY = 0f;

        foreach (SoundCategory category in soundCategories)
        {
            // Instantiate a header for the category if a header prefab is provided.
            if (categoryHeaderPrefab != null)
            {
                GameObject header = Instantiate(categoryHeaderPrefab, transform);
                header.transform.localPosition = new Vector3(0, currentY, 0);
                TMP_Text headerText = header.GetComponent<TMP_Text>();
                if (headerText != null)
                {
                    headerText.text = category.categoryName;
                }
                else
                {
                    Debug.LogWarning("Category header prefab is missing a TMP_Text component.");
                }

                // Move down a bit after the header.
                currentY -= headerSpacing;
            }

            // Create a grid for each audio clip in this category.
            for (int i = 0; i < category.audioClips.Length; i++)
            {
                GameObject item = Instantiate(soundItemPrefab, transform);
                // Calculate grid position for this item.
                float x = (i % itemsPerRow) * spacing;
                float y = currentY - ((int)(i / itemsPerRow)) * spacing;
                item.transform.localPosition = new Vector3(x, y, 0);

                // Set the AudioClip on the SoundItem script.
                SoundItem soundItem = item.GetComponent<SoundItem>();
                if (soundItem != null)
                {
                    soundItem.AudioClip = category.audioClips[i];
                    soundItem.soundPanel = transform;
                }
                else
                {
                    Debug.LogWarning("SoundItem prefab is missing the SoundItem script.");
                }
            }

            // Calculate how many rows this category occupies.
            int rowsForCategory = Mathf.CeilToInt((float)category.audioClips.Length / itemsPerRow);
            // Move the currentY down by the height of these rows.
            currentY -= rowsForCategory * spacing;
            // Add extra spacing between categories.
            currentY -= categorySpacing;
        }
    }
}
