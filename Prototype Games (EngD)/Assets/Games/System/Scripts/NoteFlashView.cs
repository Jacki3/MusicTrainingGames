using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TO DO: major cleanup and remove duplicate code (see comments)
public class NoteFlashView : MonoBehaviour
{
    public GameObject choices; //this gets shut off (persists through all games) when users start game

    public int currentNote;

    public string currentNoteString;

    public Color32 currentNoteColour;

    public EventSystem eSystem;

    public Dropdown scaleTypeDropDown;

    public Dropdown rootNoteChoice;

    public bool usingSharpScale;

    public bool usingScaleMode;

    public Sprite flatSymbol;

    public Sprite sharpSymbol;

    public List<Toggle> enHarmonicToggles = new List<Toggle>();

    public List<int> allNotes = new List<int>();

    public List<string>
        noteNames =
            new List<string>(new string[] {
                    "c",
                    "c#",
                    "d",
                    "d#",
                    "e",
                    "f",
                    "f#",
                    "g",
                    "g#",
                    "a",
                    "a#",
                    "b"
                });

    public List<Image> enHarmonicPlaces = new List<Image>();

    [System.NonSerialized]
    public List<Color>
        noteColours =
            new List<Color>()
            {
                new Color32(194, 0, 40, 255),
                new Color32(208, 64, 88, 255),
                new Color32(222, 128, 75, 255),
                new Color32(236, 191, 163, 255),
                new Color32(250, 255, 50, 255),
                new Color32(134, 192, 59, 255),
                new Color32(18, 128, 68, 255),
                new Color32(43, 134, 162, 255),
                new Color32(68, 140, 255, 255),
                new Color32(100, 105, 216, 255),
                new Color32(131, 70, 178, 255),
                new Color32(163, 35, 139, 255)
            };

    public bool usePatterns;

    public List<string> notePatterns = new List<string>();

    public int patternIndex = 0;

    public string currentPattern;

    public string previousPattern;

    public List<Image> keyboardStickers = new List<Image>();

    public int startMIDINumber = 60;

    public List<Toggle> staffToggles = new List<Toggle>();

    public List<Toggle> staffTogglesFlats = new List<Toggle>();

    public GameObject staffNoteChoices;

    private List<Sprite> sharpNoteImages = new List<Sprite>();

    private List<Sprite> flatNoteImages = new List<Sprite>();

    private List<Sprite> highSharpNoteImages = new List<Sprite>();

    private List<Sprite> highFlatNoteImages = new List<Sprite>();

    private List<Sprite> noteImages = new List<Sprite>();

    private Sprite currentNotation;

    private List<Sprite> highNoteImages = new List<Sprite>();

    private List<int>
        allScaleNotes = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

    [HideInInspector]
    public int[] sharpsAndFlats = { -2, 1, 3, 6, 8, 10, 13, 15, 18, 20, 22 };

    private List<string>
        sharpScale =
            new List<string> {
                "C",
                "C#",
                "D",
                "D#",
                "E",
                "F",
                "F#",
                "G",
                "G#",
                "A",
                "A#",
                "B",
                "C^",
                "C#^",
                "D^",
                "D#^",
                "E^",
                "F^",
                "F#^",
                "G^",
                "G#^",
                "A^",
                "A#^",
                "B^"
            };

    private List<string>
        flatScale =
            new List<string> {
                "C",
                "Db",
                "D",
                "Eb",
                "E",
                "F",
                "Gb",
                "G",
                "Ab",
                "A",
                "Bb",
                "B",
                "C^",
                "Db^",
                "D^",
                "Eb^",
                "E^",
                "F^",
                "Gb^",
                "G^",
                "Ab^",
                "A^",
                "Bb^",
                "B^"
            };

    private List<string>
        majorSharpScaleNotes =
            new List<string> { "C#", "E", "D", "F#", "G", "A", "B" };

    private List<string>
        minorharpScaleNotes =
            new List<string> { "C#", "D#", "E", "F#", "G#", "A#", "B" };

    private List<int> notesToHideMinor = new List<int> { 3, 5, 11 };

    private List<int> notesToHideMajor = new List<int> { 2, 8, 10 };

    private int priorNote;

    private string priorNoteText = "";

    //There needs to be a way to globalise this accross all games (all games should not have to write a play note function!)
    public void PlayNote(int note)
    {
        int newNote = note - startMIDINumber;
        if (newNote >= 0 && newNote < keyboardStickers.Count)
            keyboardStickers[newNote].enabled = true;
    }

    public void NoteOff(int note)
    {
        int newNote = note - startMIDINumber;
        if (newNote >= 0 && newNote < keyboardStickers.Count)
            keyboardStickers[newNote].enabled = false;
    }

    private void Start()
    {
        // noteImages.AddRange (highSharpNoteImages);
        sharpNoteImages.AddRange (highSharpNoteImages);
        flatNoteImages.AddRange (highFlatNoteImages);

        noteNames = sharpScale;

        if (usePatterns)
        {
            currentPattern = notePatterns[Random.Range(0, notePatterns.Count)];
            previousPattern = currentPattern;
        }

        foreach (Toggle
            tog
            in
            staffNoteChoices.GetComponentsInChildren<Toggle>()
        )
        {
            if (!tog.name.Contains("Flat")) staffToggles.Add(tog);
            if (!tog.name.Contains("Sharp")) staffTogglesFlats.Add(tog);
        }

        for (int i = 0; i < staffTogglesFlats.Count; i++)
        {
            if (staffTogglesFlats[i].name.Contains("Flat"))
            {
                int index = staffTogglesFlats.IndexOf((staffTogglesFlats[i]));
                Toggle savedToggle = staffTogglesFlats[i];
                staffTogglesFlats.Remove(staffTogglesFlats[i]);
                staffTogglesFlats.Insert(index - 1, savedToggle);
            }
        }
    }

    public void HideChoices()
    {
        if (choices) choices.SetActive(false);
        foreach (Image sticker in keyboardStickers) sticker.enabled = false;
    }

    public void GetNewNote()
    {
        if (!usePatterns)
        {
            //Build a shuffle function for this
            while (currentNote == priorNote && allNotes.Count > 1)
            {
                currentNote = Random.Range(0, allNotes.Count);
            }

            //if there is a sharp or a flat
            if (
                sharpsAndFlats.Contains(sharpsAndFlats[currentNote]) &&
                !usingScaleMode &&
                allNotes.Count > 1
            )
            {
                //if it was the same note as before (i.e. a sharp or flat then show the opposite)
                if (allNotes[currentNote] == allNotes[priorNote])
                {
                    if (priorNoteText.Contains("#"))
                    {
                        usingSharpScale = false;
                        noteNames = flatScale;
                    }
                    else if (priorNoteText.Contains("b"))
                    {
                        usingSharpScale = true;
                        noteNames = sharpScale;
                    }
                }
                else
                //if they were not the same but is a flat/sharp then detetmine the enharmonic type from the toggle list
                {
                    if (staffToggles[allNotes[currentNote]].isOn)
                    {
                        usingSharpScale = true;
                        noteNames = sharpScale;
                    }
                    else if (staffTogglesFlats[allNotes[currentNote]].isOn)
                    {
                        usingSharpScale = false;
                        noteNames = flatScale;
                    }
                }
            }
        }
        else
        {
            if (patternIndex >= currentPattern.Length)
            {
                patternIndex = 0;
                if (notePatterns.Count > 1)
                {
                    while (currentPattern == previousPattern)
                    currentPattern =
                        notePatterns[Random.Range(0, notePatterns.Count)];
                    previousPattern = currentPattern;
                }
            }
            currentNote =
                (int) System.Char.GetNumericValue(currentPattern[patternIndex]);
        }

        currentNoteString = noteNames[allNotes[currentNote]];
        priorNote = currentNote;
        currentNoteColour = noteColours[allNotes[currentNote] % 12];

        priorNoteText = currentNoteString;
    }

    public void AddNoteToList(int note) //enabling stickers on/off is good but shrps and flats overlap turning both on/off
    {
        if (usingScaleMode)
        {
            allNotes.Clear();
            usingScaleMode = false;
            foreach (Image sticker in keyboardStickers) sticker.enabled = false;
            foreach (Toggle staffToggle in staffToggles)
            staffToggle.GetComponentInChildren<Image>().color = Color.black;
            foreach (Toggle staffToggleFlat in staffTogglesFlats)
            staffToggleFlat.GetComponentInChildren<Image>().color = Color.black;

            foreach (Image enHarmonicSymbol in enHarmonicPlaces)
            enHarmonicSymbol.enabled = false;
        }

        var currentToggle =
            eSystem.currentSelectedGameObject.GetComponent<Toggle>();

        if (currentToggle.name == "Flat")
        {
            usingSharpScale = false;
            noteNames = flatScale;
        }
        else if (currentToggle.name == "Sharp")
        {
            usingSharpScale = true;
            noteNames = sharpScale;
        }

        if (currentToggle.isOn)
        {
            allNotes.Add (note);
            currentToggle.GetComponentInChildren<Image>().color =
                noteColours[note % 12];

            //show the note chosen on the keyboard, if the keyboard exists
            if (keyboardStickers.Count > 0)
                keyboardStickers[note].enabled = true;
        }
        else
        {
            allNotes.Remove (note);

            currentToggle.GetComponentInChildren<Image>().color = Color.black;
            if (keyboardStickers.Count > 0)
                keyboardStickers[note].enabled = false;
        }
    }

    private void SetEnHarmonicToggles(string harmonicType, bool activeState)
    {
        foreach (Toggle enHarmonicToggle in enHarmonicToggles)
        {
            if (enHarmonicToggle.name.Contains(harmonicType))
                enHarmonicToggle.interactable = activeState;
        }
    }

    //https://answers.unity.com/questions/1744833/checking-if-all-gameobjects-in-list-are-inactive.html (cleaner approach)
    private bool CheckForEnharmonics(string enHarmonicToCheck)
    {
        foreach (Toggle enHarmonicToggle in enHarmonicToggles)
        {
            if (enHarmonicToggle.name.Contains(enHarmonicToCheck))
                if (enHarmonicToggle.isOn)
                {
                    return false;
                }
        }
        return true;
    }

    public int CheckForDuplicateNotes(int noteToCheck)
    {
        int dupeCount = 0;

        foreach (int note in allNotes)
        {
            if (note == noteToCheck) dupeCount++;
        }
        return dupeCount;
    }

    public void GenerateScale() //bonus: add more modes
    {
        foreach (Image sticker in keyboardStickers) sticker.enabled = false;
        foreach (Toggle staffToggle in staffToggles)
        staffToggle.GetComponentInChildren<Image>().color = Color.black;
        foreach (Toggle staffToggleFlat in staffTogglesFlats)
        staffToggleFlat.GetComponentInChildren<Image>().color = Color.black;

        usingScaleMode = true;

        var currentToggle =
            eSystem.currentSelectedGameObject.GetComponent<Toggle>();

        string rootNoteName =
            rootNoteChoice.transform.GetChild(0).GetComponent<Text>().text;

        int rootNote = sharpScale.IndexOf(rootNoteName);
        if (rootNote < 0) rootNote = flatScale.IndexOf(rootNoteName);

        foreach (Image symbol in enHarmonicPlaces) symbol.enabled = false;

        //reshuffle list with rootNote as start
        allScaleNotes =
            allScaleNotes
                .SkipWhile(x => x != rootNote)
                .Concat(allScaleNotes.TakeWhile(x => x != rootNote))
                .ToList();

        List<int> scalePattern = new List<int>();
        switch (scaleTypeDropDown.value)
        {
            case 1:
                scalePattern = new List<int> { 0, 2, 4, 5, 7, 9, 11 }; //Major
                if (
                    majorSharpScaleNotes
                        .Contains(rootNoteName.ToUpperInvariant())
                )
                {
                    noteNames = sharpScale;

                    highNoteImages = highSharpNoteImages;
                    usingSharpScale = true;
                }
                else
                {
                    noteNames = flatScale;

                    highNoteImages = highFlatNoteImages;
                    usingSharpScale = false;
                }
                break;
            case 2:
                scalePattern = new List<int> { 0, 2, 3, 5, 7, 8, 10 }; //Minor
                if (
                    minorharpScaleNotes
                        .Contains(rootNoteName.ToUpperInvariant())
                )
                {
                    noteNames = sharpScale;

                    highNoteImages = highSharpNoteImages;
                    usingSharpScale = true;
                }
                else
                {
                    noteNames = flatScale;

                    highNoteImages = highFlatNoteImages;
                    usingSharpScale = false;
                }
                break;
            case 3:
                scalePattern = new List<int> { 0, 2, 4, 7, 9 }; //Major Pentatonic
                if (
                    majorSharpScaleNotes
                        .Contains(rootNoteName.ToUpperInvariant())
                )
                {
                    noteNames = sharpScale;
                    highNoteImages = highSharpNoteImages;
                    usingSharpScale = true;
                }
                else
                {
                    noteNames = flatScale;
                    highNoteImages = highFlatNoteImages;
                    usingSharpScale = false;
                }
                break;
        }

        //first note in scale letters = the letter provided
        List<int> scale = new List<int>();

        //Generate a scale based on the scale pattern and add relevant enharmonics to the staff
        for (int i = 0; i < scalePattern.Count; i++)
        {
            scale.Add(allScaleNotes[scalePattern[i]]);
            if (noteNames[scale[i]].Contains("b"))
                SetEnHarmonicSymbols(true, flatSymbol);
            else if (noteNames[scale[i]].Contains("#"))
                SetEnHarmonicSymbols(true, sharpSymbol);
        }

        allNotes = scale;

        int endOfScale = allNotes.IndexOf(11);
        if (endOfScale < 0)
        {
            endOfScale = allNotes.IndexOf(10);
            if (endOfScale < 0) endOfScale = allNotes.IndexOf(9);
        }
        if (endOfScale >= 0)
        {
            for (int i = endOfScale + 1; i < allNotes.Count; i++)
            {
                allNotes[i] = allNotes[i] + 12;
            }
        }

        foreach (int note in allNotes)
        {
            if (keyboardStickers.Count > 0)
                keyboardStickers[note].enabled = true;
            if (usingSharpScale)
            {
                staffToggles[note].GetComponentInChildren<Image>().color =
                    noteColours[note % 12];
                noteNames = sharpScale;
            }
            else
            {
                staffTogglesFlats[note].GetComponentInChildren<Image>().color =
                    noteColours[note % 12];
                noteNames = flatScale;
            }
        }
    }

    private void SetEnHarmonicSymbols(bool activeState, Sprite symbolType)
    {
        if (activeState)
        {
            foreach (Image enHarmonicSymbol in enHarmonicPlaces)
            {
                enHarmonicSymbol.sprite = symbolType;

                if (!enHarmonicSymbol.enabled)
                {
                    enHarmonicSymbol.enabled = true;
                    break;
                }
            }
        }
        else
        {
            //find last active symbol and deactivate
            for (int i = enHarmonicPlaces.Count - 1; i >= 0; i--)
            {
                if (enHarmonicPlaces[i].enabled)
                {
                    enHarmonicPlaces[i].enabled = false;
                    break;
                }
            }
        }
    }

    //For disabling major/minor choices for specific notes (if the scale is theoretical we wont allow this)
    public void CheckEnHarmonics()
    {
        //Resets all drop down lists
        scaleTypeDropDown
            .GetComponent<DropDownController>()
            .EnableOption(1, true);
        scaleTypeDropDown
            .GetComponent<DropDownController>()
            .EnableOption(2, true);

        foreach (int note in notesToHideMajor)
        rootNoteChoice
            .GetComponent<DropDownController>()
            .EnableOption(note, true);
        foreach (int note in notesToHideMinor)
        rootNoteChoice
            .GetComponent<DropDownController>()
            .EnableOption(note, true);

        //If user chooses a specific note, hide major/minor/etc that create theoretical scales
        if (notesToHideMajor.Contains(rootNoteChoice.value))
            scaleTypeDropDown
                .GetComponent<DropDownController>()
                .EnableOption(1, false);
        else if (notesToHideMinor.Contains(rootNoteChoice.value))
            scaleTypeDropDown
                .GetComponent<DropDownController>()
                .EnableOption(2, false);

        //If user chooses a specific mode, hide partiular notes that create theoretical scales
        if (scaleTypeDropDown.value == 1)
            foreach (int note in notesToHideMajor)
            rootNoteChoice
                .GetComponent<DropDownController>()
                .EnableOption(note, false);
        else if (scaleTypeDropDown.value == 2)
            foreach (int note in notesToHideMinor)
            rootNoteChoice
                .GetComponent<DropDownController>()
                .EnableOption(note, false);
    }

    public void AddSharpsAndFlats()
    {
        for (int i = 0; i < sharpsAndFlats.Length; i++)
        allNotes.Add(sharpsAndFlats[i]);
    }

    public void ShowLetterChoices(Text toggleText)
    {
        if (!staffNoteChoices.activeSelf)
        {
            staffNoteChoices.SetActive(true);
            toggleText.text = "Show Note Letters";
        }
        else
        {
            staffNoteChoices.SetActive(false);
            toggleText.text = "Show Staff";
        }
    }
}
