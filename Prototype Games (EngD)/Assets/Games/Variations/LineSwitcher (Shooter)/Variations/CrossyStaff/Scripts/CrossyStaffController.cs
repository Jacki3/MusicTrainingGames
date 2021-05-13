using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CrossyStaffController : MonoBehaviour
{
    public bool gameStarted = false;

    public bool usingRhythm = false;

    public bool tapMode;

    public bool adjancementMode;

    public NoteFlashView notesController;

    public Conductor theConductor;

    public GameObject noteHero;

    public GoalController levelGoal;

    public ScoreGainEffect scoreGainEffect;

    public int startMIDINumber = 60;

    public Transform[] sharpStaffPositions;

    public Transform[] flatStaffPositions;

    public int currentNote;

    public float score;

    public float scoreMultiplier;

    public int roundsToMultiplier = 3;

    public int maxMultiplier = 4;

    public float speedMultiplier = 1;

    public float speedIncrease = .1f;

    public int totalLives = 5;

    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI roundText;

    public List<Image> hearts = new List<Image>();

    public RhythmDetector rhythmDetector;

    public bool useColour = true;

    public bool quickFireMode = false;

    public float maxDifficultyMultiplier = 4;

    public float doubleSpeedTime = 60;

    public int goalsToMultiplier = 4;

    public SpriteRenderer flashBackground;

    private Transform[] spawnPoints;

    float t = 1;

    public float posX = 12;

    Color defaultFlashColor;

    [HideInInspector]
    public float carT = 0;

    [HideInInspector]
    public bool canFlash = false;

    private float elapsedGameTime;

    void Start()
    {
        defaultFlashColor = flashBackground.color;
        spawnPoints = sharpStaffPositions;
    }

    void Update()
    {
        if (gameStarted) elapsedGameTime = 1 + Time.deltaTime;

        // if (speedMultiplier < maxDifficultyMultiplier && !usingRhythm)
        //     speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;
        if (carT <= theConductor.secPerBeat)
        {
            carT += Time.deltaTime / theConductor.secPerBeat;
        }
        if (carT >= theConductor.secPerBeat)
        {
            canFlash = true;
            carT = 0;
        }
        else
            canFlash = false;

        if (Input.GetKeyUp(KeyCode.Space) && notesController.allNotes.Count >= 1
        )
        {
            carT = 0;
            theConductor.StartSong();
            if (notesController.usingScaleMode)
            {
                int endOfScale = notesController.allNotes.IndexOf(11);
                if (endOfScale < 0)
                    endOfScale = notesController.allNotes.IndexOf(10);
                for (
                    int i = endOfScale + 1;
                    i < notesController.allNotes.Count;
                    i++
                )
                {
                    notesController.allNotes[i] =
                        notesController.allNotes[i] + 12;
                }
            }
            currentNote = notesController.allNotes[0] + startMIDINumber;
            gameStarted = true;
            SetGoals();
            noteHero.transform.position =
                sharpStaffPositions[notesController.allNotes[0]].position;
        }
        if (gameStarted)
        {
            //work on this later date!
            // int noteIndex = notesController.allNotes.IndexOf(currentNote % 12);
            // int index = notesController.allNotes[noteIndex + 1];
            // int previousInedx = notesController.allNotes[noteIndex];
            // sharpStaffPositions[index].GetComponent<SpriteRenderer>().color =
            //     Color.green;
            // sharpStaffPositions[previousInedx]
            //     .GetComponent<SpriteRenderer>()
            //     .color = Color.black;
            if (notesController.usingScaleMode)
            {
                for (int i = 1; i < notesController.allNotes.Count; i++)
                {
                    sharpStaffPositions[notesController.allNotes[i]]
                        .GetComponent<NoteCarSpawner>()
                        .enabled = true;
                }
            }
            else
            {
                for (int i = 1; i < notesController.allNotes.Count - 1; i++)
                {
                    sharpStaffPositions[notesController.allNotes[i]]
                        .GetComponent<NoteCarSpawner>()
                        .enabled = true;
                }
            }
        }

        scoreText.text = Mathf.FloorToInt(score) + "x" + scoreMultiplier;

        flashBackground.color =
            Color.Lerp(flashBackground.color, defaultFlashColor, t);
        if (t < 1) t += Time.deltaTime / 10;

        if (GameOver())
        {
            Time.timeScale = 0;
        }
        if (Input.GetKey(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (tapMode && speedMultiplier >= 4) speedMultiplier = 4; //stuck at quarter notes if you want to tap to metro
    }

    private void SetGoals()
    {
        int goalIndex = 0;
        for (int i = 0; i < 2; i++)
        {
            var goal = Instantiate(levelGoal);
            Transform goalPosition =
                spawnPoints[notesController.allNotes[0] + goalIndex];
            var tempY = goal.transform.position;
            tempY.y = goalPosition.position.y;
            goal.transform.position = tempY;
            goal.controller = this;
            goal.index = i;
            goal.bonusEffect = scoreGainEffect;
            if (notesController.usingScaleMode)
                goalIndex += 12;
            else
                goalIndex +=
                    notesController
                        .allNotes[notesController.allNotes.Count - 1] -
                    notesController.allNotes[0];
        }
    }

    public void PlayNote(int note)
    {
        //needed for ryhthm tap mode
        if (!tapMode || rhythmDetector.canHit)
        {
            if (
                !adjancementMode &&
                notesController.allNotes.Contains(note - startMIDINumber) ||
                AreNotesScaleAdjacent(note, currentNote)
            )
            {
                currentNote = note;
                var tempY = noteHero.transform.position;
                tempY.y = spawnPoints[note - startMIDINumber].position.y;
                noteHero.transform.position = tempY;
            }
        }
    }

    public void AddScore(float scoreToAdd)
    {
        score += scoreToAdd * scoreMultiplier;
        flashBackground.color = Color.green;
        t = 0;
    }

    public void LoseLife()
    {
        //play a ouch sound?
        flashBackground.color = Color.red;
        t = 0;
        totalLives--;
        foreach (Image heart in hearts)
        if (heart.enabled)
        {
            heart.enabled = false;
            break;
        }
    }

    private bool AreNotesScaleAdjacent(int a, int b) =>
        a == NextInScale(b) || b == NextInScale(a);

    private int NextInScale(int note)
    {
        var scaleIndex = -1;
        for (int i = 0; i < notesController.allNotes.Count; i++)
        {
            if (note - startMIDINumber == notesController.allNotes[i])
                scaleIndex = i;
        }
        if (scaleIndex < 0) return -1;

        int nextNote;
        if (scaleIndex + 1 < notesController.allNotes.Count)
        {
            nextNote =
                note -
                notesController.allNotes[scaleIndex] +
                notesController.allNotes[scaleIndex + 1];
        }
        else
        {
            nextNote =
                note -
                notesController.allNotes[scaleIndex] +
                notesController.allNotes[0] +
                12;
        }
        return nextNote;
    }

    protected bool GameOver() => totalLives <= 0;

    //--Menu Controls--//
    public void UpdateBPM(InputField value)
    {
        float newBPM = float.Parse(value.text);
        if (newBPM > 0) theConductor.songBPM = newBPM;
        theConductor.secPerBeat = 60 / theConductor.songBPM;
    }

    public void DifficultySwitch()
    {
        if (usingRhythm)
            usingRhythm = false;
        else
            usingRhythm = true;
    }

    public void UpdateTapOnBeat()
    {
        if (tapMode)
            tapMode = false;
        else
            tapMode = true;
    }

    public void UpdateColourSwitch()
    {
        if (useColour)
            useColour = false;
        else
            useColour = true;
    }

    public void UpdateMusicVol()
    {
        if (theConductor.musicSource.volume > 0)
            theConductor.musicSource.volume = 0;
        else
            theConductor.musicSource.volume = .65f;
    }

    public void UpdateMetroVol()
    {
        if (theConductor.metronome.volume > 0)
            theConductor.metronome.volume = 0;
        else
            theConductor.metronome.volume = .35f;
    }

    public void UpdateQuickFire(Toggle difficultyToggle)
    {
        if (quickFireMode)
        {
            quickFireMode = false;
            usingRhythm = false;
            difficultyToggle.interactable = true;
        }
        else
        {
            quickFireMode = true;
            usingRhythm = true;
            difficultyToggle.interactable = false;
        }
    }
}
