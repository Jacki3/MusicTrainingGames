using System.Collections;
using System.Collections.Generic;
using MidiJack;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputTestMIDI : MonoBehaviour
{
    public MIDITest test;

    public void NoteOn(MidiChannel channel, int note, float velocity)
    {
        // Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);
        test.PlayNote (note, velocity);
    }

    void NoteOff(MidiChannel channel, int note)
    {
        // Debug.Log("NoteOff: " + channel + "," + note);
        test.NoteOff (note);
    }

    void OnEnable()
    {
        MidiMaster.noteOnDelegate += NoteOn;
        MidiMaster.noteOffDelegate += NoteOff;
    }

    void OnDisable()
    {
        MidiMaster.noteOnDelegate -= NoteOn;
        MidiMaster.noteOffDelegate -= NoteOff;
    }
}
