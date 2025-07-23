using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SoundSelector : MonoBehaviour
{
    public GameObject soundButtonPrefab; // Assign the SoundButtonPrefab in the Inspector
    public Transform soundListPanel; // Assign the SoundListPanel in the Inspector
    public AudioClip[] availableSounds; // Assign sound files in the Inspector

    private Dictionary<Button, AudioClip> buttonSoundMapping = new Dictionary<Button, AudioClip>();
    private AudioClip selectedSound;

    void Start()
    {
        PopulateSoundList();
    }

    void PopulateSoundList()
    {
        foreach (AudioClip sound in availableSounds)
        {
            GameObject buttonObj = Instantiate(soundButtonPrefab, soundListPanel);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = sound.name;
            buttonSoundMapping[button] = sound;

            button.onClick.AddListener(() => SelectSound(sound));
        }
    }

    public void SelectSound(AudioClip sound)
    {
        selectedSound = sound;
        Debug.Log("Selected Sound: " + sound.name);
    }

    public AudioClip GetSelectedSound()
    {
        return selectedSound;
    }
}
