﻿using System.Collections;
using System.Collections.Generic;
using MidiJack;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MIDI_Input : MonoBehaviour
{
    public NoteFlashView notesController;

    public FlashCardController cardController;

    public AsteroidSpawner shooterController;

    public EarForgeType earForgeController;

    public SonarNotesController sonarNotesController;

    public RhythmDotsController dotsController;

    public LineSwitcherController lineSwitcherController;

    public NoteCatcherController catcherController;

    public ScaleClimbController scaleClimbController;

    public CrossyStaffController crossyStaffController;

    public NoteDodgerController dodgerController;

    public ScaleLoop conductor;

    public void NoteOn(MidiChannel channel, int note, float velocity)
    {
        // Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);
        // there should be a cleaner way to do this; whilst each controller handles playing notes slightly differently, there should be a way to have one playnote function to avoid this
        if (shooterController) shooterController.PlayNote(note);
        if (cardController) cardController.PlayNote(note);
        if (earForgeController) earForgeController.PlayNote(note);
        if (sonarNotesController) sonarNotesController.PlayNote(note);
        if (dotsController) dotsController.PlayNote(note);
        if (lineSwitcherController)
        {
            if (lineSwitcherController.usingRhythm)
            {
                if (lineSwitcherController.canHit)
                    lineSwitcherController.PlayNote(note);
                else
                    return;
            }
            else
                lineSwitcherController.PlayNote(note);
        }
        if (catcherController) catcherController.PlayNote(note);
        if (notesController) notesController.PlayNote(note);
        if (scaleClimbController) scaleClimbController.PlayNote(note);
        if (crossyStaffController) crossyStaffController.PlayNote(note);
        if (dodgerController) dodgerController.PlayNote(note);
        if (!lineSwitcherController && !catcherController)
            conductor.PlayNote(note);
    }

    void NoteOff(MidiChannel channel, int note)
    {
        // Debug.Log("NoteOff: " + channel + "," + note);
        if (notesController) notesController.NoteOff(note);
        if (scaleClimbController) scaleClimbController.NoteOff(note);
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
