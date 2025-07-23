using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SoundSelector2 : MonoBehaviour
{
    [Header("References")]
    public GameObject soundItemPrefab;    // Assign the SoundItemPrefab in the Inspector
    public Transform soundListContainer;  // Assign the container (an empty GameObject)

    [Header("Sounds")]
    public AudioClip[] availableSounds;   // Assign sound files in the Inspector

    void Start()
    {
        PopulateSoundList();
    }

    void PopulateSoundList()
    {
        if (availableSounds == null || availableSounds.Length == 0)
        {
            Debug.LogError("No sounds available! Assign AudioClips in the Inspector.");
            return;
        }

        foreach (AudioClip sound in availableSounds)
        {
            // Instantiate the sound item
            GameObject item = Instantiate(soundItemPrefab, soundListContainer);
            // Get the SoundItem component and assign the audio clip
            SoundItem soundItem = item.GetComponent<SoundItem>();
            if (soundItem != null)
            {
                soundItem.AudioClip = sound;
            }
            else
            {
                Debug.LogError("SoundItem component missing on prefab!");
            }

            // Set the text to display the sound name
            TextMeshPro text = item.GetComponentInChildren<TextMeshPro>();
            if (text != null)
            {
                text.text = sound.name;
            }
        }
    }
}
