using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    public float songBPM;

    public float secPerBeat;

    public float songPosition;

    public float songPosInBeats;

    public float dspSongTime;

    public AudioSource musicSource;

    public float[] notes = { 1f, 2f, 3f, 4f, 5f, 6f };

    public int nextIndex = 0;

    public bool oneCycleComplete = false;

    public bool isOnBeat = true;

    public int cycles = 0;

    public float lastBeat;

    public AudioClip metronomeBeat;

    public AudioSource metronome;

    private AudioSource menuMusic;

    float timerMultiplier = 4;

    bool songStarted = false;

    private void Awake()
    {
        secPerBeat = 60f / songBPM;
    }

    private void Start()
    {
        menuMusic = GetComponent<AudioSource>();
        lastBeat = 0;
    }

    public void StartSong()
    {
        menuMusic.Stop();
        musicSource.Play();

        //Calculate number of seconds in each beat
        secPerBeat = 60f / songBPM;

        //Record start time of music
        dspSongTime = (float) AudioSettings.dspTime;

        // musicSource.Play();
        songStarted = true;

        StartCoroutine(PlayMetronomeBeat());
    }

    void Update()
    {
        //calculate song position in seconds
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);

        //calculate this position in beats
        songPosInBeats = songPosition / secPerBeat;

        if (songPosition > lastBeat + secPerBeat)
        {
            lastBeat += secPerBeat;
        }

        if ((secPerBeat * timerMultiplier) <= songPosition && songStarted)
        {
            timerMultiplier += 4;
            if (oneCycleComplete) cycles++;
            oneCycleComplete = true;
        }
    }

    IEnumerator PlayMetronomeBeat()
    {
        while (true)
        {
            metronome.PlayOneShot (metronomeBeat);
            yield return new WaitForSeconds(secPerBeat);
        }
    }
}
