using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDITest : MonoBehaviour
{
    public AudioHelm.HelmController helmController;

    public void PlayNote(int note, float velocity)
    {
        helmController.NoteOn (note, velocity);
    }

    public void NoteOff(int note)
    {
        helmController.NoteOff (note);
    }
}
