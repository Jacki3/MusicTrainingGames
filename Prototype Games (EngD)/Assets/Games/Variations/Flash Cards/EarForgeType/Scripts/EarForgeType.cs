using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//MODES: Level Mode w/ and w/out time aspect (add duolingo streak style), time mode w/ difficulty increase (survival), set amount of time to see how many you can get w/ out time, infinite mode w/ and w/ out timer; should incorrect notes loose lives in certain modes?
//where the note plays a sound; implement helm there
public class EarForgeType : MonoBehaviour
{
    [Header("Game Settings")]
    public bool colourMode = true;

    public float endTime = 5;

    public int notesToComplete = 10;

    public int totalLives = 3;

    public float waitTime = 2;

    public bool survivalMode = false;

    public float doubleSpeedTime = 60;

    [Header("UI & Objects")]
    public TextMeshProUGUI noteText;

    public TextMeshProUGUI responseText;

    public Image notationDisplay;

    public Image circleOutline;

    public TextMeshProUGUI notesToCompleteText;

    public TextMeshProUGUI completedNotesText;

    public TextMeshProUGUI streakText;

    public TextMeshProUGUI correctText;

    public TextMeshProUGUI incorrectText;

    public Slider noteCountSlider;

    public List<Image> keyboardStickers = new List<Image>();

    public List<Sprite> sharpNotationImages = new List<Sprite>();

    public List<Sprite> flatNotationImages = new List<Sprite>();

    public List<Image> hearts = new List<Image>();

    [Header("System")]
    public AudioHelm.HelmController helmController;

    public FlashSonarDuoController controller;

    public NoteFlashView system;

    public ScaleLoop conductor;

    public int startMIDINumber;

    private List<Sprite> notationImages = new List<Sprite>();

    private int timeMultiplier = 0;

    private float timeToAnswer;

    private int notesCompleted;

    private float t = 1;

    private Color defaultCamColor;

    bool gameStarted = false;

    bool showNotation = true;

    bool canFail = false;

    bool infiniteTimeMode = false;

    bool infiniteLevelMode = false;

    float elapsedGameTime = 1;

    float speedMultiplier = 1;

    int totalCorrect;

    int totalIncorrect;

    int streakScore;

    void Start()
    {
        defaultCamColor = Camera.main.backgroundColor;
        controller = GetComponent<FlashSonarDuoController>();
        if (!controller) Debug.LogError("No Controller Found!");
    }

    void Update()
    {
        if (system.usingSharpScale)
            notationImages = sharpNotationImages;
        else
            notationImages = flatNotationImages;

        // can play forever w/ timer mode OR play forever /timer mode which increases in speed gradually
        timeToAnswer += Time.deltaTime * timeMultiplier * speedMultiplier;
        circleOutline.fillAmount = timeToAnswer / endTime;

        float seconds = Mathf.FloorToInt(timeToAnswer + 1 % 60);
        if (timeMultiplier == -1)
        {
            noteText.enabled = true;
            noteText.text = seconds.ToString();
            notationDisplay.enabled = false;
            canFail = false;
        }
        else
            canFail = true;
        if (gameStarted)
        {
            if (timeToAnswer <= 0 && !GameOver())
            {
                responseText.text = null;
                ShowNextNote();
                timeMultiplier = 1;
            }
            if (!infiniteTimeMode && survivalMode)
            {
                elapsedGameTime += Time.deltaTime;
                speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;
            }
        }

        if (TimeOver())
        {
            totalLives--;
            timeToAnswer = waitTime;
            timeMultiplier = -1;

            responseText.color = Color.red;
            Camera.main.backgroundColor = Color.red;
            t = 0;
            controller.RemoveLife();
        }

        if (GameOver())
        {
            controller.GameOver();
            timeMultiplier = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space) && system.allNotes.Count >= 1)
        {
            timeMultiplier = -1;
            gameStarted = true;
            timeToAnswer = waitTime;

            system.HideChoices(); // required?
        }

        if (!infiniteLevelMode)
            notesToCompleteText.text = notesToComplete.ToString();
        else
        {
            notesToCompleteText.text =
                Mathf.FloorToInt(elapsedGameTime).ToString();

            infiniteLevelMode = true;
            notesToComplete = 0;
        }

        controller.streakText.text = streakScore.ToString();
        controller.totalCorrectText.text = totalCorrect.ToString();
        controller.totalInCorrectText.text = totalIncorrect.ToString();

        completedNotesText.text = notesCompleted.ToString();

        Camera.main.backgroundColor =
            Color.Lerp(Camera.main.backgroundColor, defaultCamColor, t);
        if (t < 1) t += Time.deltaTime / 10;
    }

    public void PlayNote(int note)
    {
        if (gameStarted && !GameOver())
        {
            int actualNote = system.allNotes[system.currentNote];
            if (note == actualNote + startMIDINumber && timeMultiplier == 1)
            {
                totalCorrect++;
                streakScore++;
                timeToAnswer = waitTime;
                timeMultiplier = -1;
                responseText.color = Color.green;

                Camera.main.backgroundColor = Color.green;
                t = 0;

                if (notesCompleted < notesToComplete) notesCompleted++;
                noteCountSlider.value += 1f / notesToComplete;
            }
            else
            {
                totalIncorrect++;
                streakScore = 0;
                Camera.main.backgroundColor = Color.red;
                t = 0;
                keyboardStickers[system.allNotes[system.currentNote]].enabled =
                    true;
            }
        }
    }

    public void ShowNextNote()
    {
        system.GetNewNote();
        system.patternIndex++;

        if (showNotation)
        {
            notationDisplay.enabled = true;
            noteText.enabled = false;
        }
        else
        {
            notationDisplay.enabled = false;
            noteText.enabled = true;
        }

        var noteColour = Color.black;
        if (colourMode) noteColour = system.currentNoteColour;

        noteText.text = system.currentNoteString;
        noteText.color = noteColour;
        circleOutline.color = noteColour;
        circleOutline
            .GetComponent<Image>()
            .material
            .SetColor("_EmissionColor", noteColour * 2.2f);
        var circleOutlineA = circleOutline.color;
        circleOutlineA.a = .75f;
        circleOutline.color = circleOutlineA;
        notationDisplay.sprite =
            notationImages[system.allNotes[system.currentNote]];

        helmController
            .NoteOn(system.allNotes[system.currentNote] + startMIDINumber,
            1.0f);
    }

    //--Menu Controls--//
    public void UpdateNotationChoice(Toggle notationToggle)
    {
        if (showNotation)
        {
            showNotation = false;
            notationToggle.GetComponentInChildren<Text>().text = "Letters";
        }
        else
        {
            showNotation = true;
            notationToggle.GetComponentInChildren<Text>().text = "Notation";
        }
    }

    public void UpdateTimeToGuess(InputField timeToGuess)
    {
        float newTime = float.Parse(timeToGuess.text);
        if (newTime <= 0) infiniteTimeMode = true;
        endTime = newTime;
    }

    public void UpdateNotesToComplete(InputField notesToGuess)
    {
        float newGuesses = float.Parse(notesToGuess.text);
        if (newGuesses <= 0) infiniteLevelMode = true;
        notesToComplete = (int) newGuesses;
    }

    public void UpdateColourChoice(Toggle colourToggle)
    {
        if (colourMode)
        {
            colourMode = false;
            colourToggle.GetComponentInChildren<Text>().text = "No Colour";
        }
        else
        {
            colourMode = true;
            colourToggle.GetComponentInChildren<Text>().text = "Colour!";
        }
    }

    public void UpdateSurvivalToggle(Toggle survivalToggle)
    {
        if (survivalMode)
        {
            survivalMode = false;
            survivalToggle.GetComponentInChildren<Text>().color = Color.white;
        }
        else
        {
            survivalMode = true;
            survivalToggle.GetComponentInChildren<Text>().color = Color.red;
        }
    }

    //--State Control--//
    protected bool GameOver() =>
        totalLives <= 0 ||
        notesCompleted >= notesToComplete && !infiniteLevelMode;

    protected bool TimeOver() =>
        timeToAnswer > endTime && canFail && !infiniteTimeMode;
}
