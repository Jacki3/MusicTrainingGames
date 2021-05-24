using System.Collections;
using System.Collections.Generic;
using MidiJack;
using UnityEngine;

public class MIDIInput : MonoBehaviour
{
    public AudioHelm.HelmController helmController;

    public void NoteOn(MidiChannel channel, int note, float velocity)
    {
        // Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);
        helmController.NoteOn (note, velocity);
    }

    void NoteOff(MidiChannel channel, int note)
    {
        // Debug.Log("NoteOff: " + channel + "," + note);
        helmController.NoteOff (note);
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
