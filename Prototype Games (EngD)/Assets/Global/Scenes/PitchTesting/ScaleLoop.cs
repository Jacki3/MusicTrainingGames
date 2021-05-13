using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleLoop : MonoBehaviour
{
    public AudioSource[] audioSources;

    public AudioClip keySound;

    float scale = Mathf.Pow(2f, 1.0f / 12f);

    int startMIDINumber = 60;

    AudioSource sound;

    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    public void PlayNote(int note)
    {
        note -= startMIDINumber;

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                audioSources[i].clip = keySound;
                audioSources[i].pitch = Mathf.Pow(scale, note);
                audioSources[i].Play();
                break;
            }
        }
    }
}
