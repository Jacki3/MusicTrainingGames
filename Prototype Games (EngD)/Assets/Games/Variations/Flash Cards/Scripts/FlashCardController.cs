using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlashCardController : MonoBehaviour
{
    [Header("Game Settings")]
    public bool timerMode = false;

    public float totalTime;

    public bool lineMode = false;

    public bool useColour = false;

    public float timeToGuess = .1f;

    public float doubleSpeedTime = 60;

    public int guessesToHint = 5; //total incorrect guesses required to show answer

    public bool increaseDifficulty = false; //setting to be introduced for 'scroll mode'

    [Header("UI & Objects")]
    public SpriteRenderer enharmonicPlacement;

    public TextMeshProUGUI timeText;

    public TextMeshProUGUI noteText;

    public GameObject notationDisplay;

    public Image timer;

    public TextMeshProUGUI correctNotesText;

    public TextMeshProUGUI inCorrectNotesText;

    public TextMeshProUGUI streakText;

    public Color32 successColour;

    public GameObject noteObj;

    [Header("System")]
    public NoteFlashView system;

    public int startMIDINumber;

    public List<Transform> sharpStaffPositions = new List<Transform>();

    public List<Transform> flatStaffPositions = new List<Transform>();

    public List<GameObject> sharpStaffLines = new List<GameObject>();

    public List<GameObject> flatStaffLines = new List<GameObject>();

    public List<Image> keyboardStickers = new List<Image>();

    private int totalCorrect = 0;

    private int totalIncorrect = 0;

    private int streakScore = 0;

    private float startTime;

    private bool gameStarted = false;

    Color32 defaultCamColor;

    float t = 1;

    Color32 defaultLineColor;

    float speedMultiplier = 1;

    float elapsedGameTime = 1;

    int guessesCurrentNote = 0; //guesses for each note question

    private void Start()
    {
        defaultCamColor = Camera.main.backgroundColor;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && system.allNotes.Count >= 1)
        {
            gameStarted = true;
            system.HideChoices();
            if (!timerMode)
                startTime = timeToGuess; //aka survival mode
            else
                startTime = totalTime; //aka 30 seconds to guess as many as you can

            if (!useColour)
            {
                successColour = Color.green;
            }
            ShowNextNote();
        }

        if (gameStarted)
        {
            if (!timerMode)
            {
                elapsedGameTime += Time.deltaTime;
                speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime; //implement a max speed?

                if (startTime >= 1)
                {
                    timeToGuess -= Time.deltaTime * speedMultiplier;
                    timer.fillAmount = (timeToGuess / startTime);

                    timeText.text = Mathf.FloorToInt(timeToGuess).ToString();
                }
            }
            else
            {
                totalTime -= Time.deltaTime;
                timer.fillAmount = (totalTime / startTime);
                timeText.text = Mathf.FloorToInt(totalTime).ToString();
            }
        }

        if (GameOver())
        {
            Time.timeScale = 0;
            noteText.text = "Press 'R' to Restart";
            noteText.enabled = true;
            noteObj.gameObject.SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        Camera.main.backgroundColor =
            Color.Lerp(Camera.main.backgroundColor, defaultCamColor, t);
        if (t < 1) t += Time.deltaTime / 10;

        correctNotesText.text = totalCorrect.ToString();
        inCorrectNotesText.text = totalIncorrect.ToString();
        streakText.text = streakScore.ToString();
    }

    public void PlayNote(int note)
    {
        if (system.allNotes.Count > 0 && !GameOver() && gameStarted)
        {
            int actualNote = system.allNotes[system.currentNote];

            if (note == actualNote + startMIDINumber)
            {
                Camera.main.backgroundColor = Color.green;
                t = 0; //starts lerping the colour back

                totalCorrect++;
                streakScore++;
                guessesCurrentNote = 0;

                ShowNextNote();
            }
            else
            {
                //Shows the correct answer with a set number of fails
                guessesCurrentNote++;
                if (guessesCurrentNote >= guessesToHint)
                    keyboardStickers[system.allNotes[system.currentNote]]
                        .enabled = true;

                Camera.main.backgroundColor = Color.red;
                t = 0;

                streakScore = 0;
                totalIncorrect++;
            }
        }
    }

    public void ShowNextNote()
    {
        //Resets ledger lines
        noteObj.transform.GetChild(0).gameObject.SetActive(false);
        noteObj.transform.GetChild(1).gameObject.SetActive(false);

        system.GetNewNote();

        noteText.text = system.currentNoteString;

        //UPDATE THIS: duplicate code (remove line mode after GIT upload)?
        if (system.usingSharpScale)
        {
            noteObj.transform.position =
                sharpStaffPositions[system.allNotes[system.currentNote]]
                    .position;
            if (lineMode)
            {
                defaultLineColor =
                    sharpStaffLines[system.allNotes[system.currentNote]]
                        .GetComponent<SpriteRenderer>()
                        .color;

                if (useColour)
                {
                    successColour = system.currentNoteColour;
                }
                sharpStaffLines[system.allNotes[system.currentNote]]
                    .GetComponent<SpriteRenderer>()
                    .color = successColour;
            }
        }
        else
        {
            noteObj.transform.position =
                flatStaffPositions[system.allNotes[system.currentNote]]
                    .position;
            if (lineMode)
            {
                defaultLineColor =
                    flatStaffLines[system.allNotes[system.currentNote]]
                        .GetComponent<SpriteRenderer>()
                        .color;
                if (useColour)
                {
                    successColour = system.currentNoteColour;
                }
                flatStaffLines[system.allNotes[system.currentNote]]
                    .GetComponent<SpriteRenderer>()
                    .color = successColour;
            }
        }

        if (useColour)
        {
            noteText.color = system.currentNoteColour;
            noteObj.GetComponent<SpriteRenderer>().color =
                system.currentNoteColour;
            noteObj
                .GetComponent<SpriteRenderer>()
                .material
                .SetColor("_EmissionColor", system.currentNoteColour);
        }

        var tempNoteObj = noteObj.transform.rotation; // sets the note upsidedown if need be (there is a cleaner way to do this)
        if (system.allNotes[system.currentNote] >= 11)
        {
            noteObj.GetComponent<SpriteRenderer>().flipX = true;
            noteObj.GetComponent<SpriteRenderer>().flipY = true;
        }
        else
        {
            noteObj.GetComponent<SpriteRenderer>().flipX = false;
            noteObj.GetComponent<SpriteRenderer>().flipY = false;
        }
        noteObj.transform.transform.rotation = tempNoteObj;

        var ledgerLine = noteObj.transform.GetChild(0);

        //handling simple ledger lines ~ could be cleaner
        switch (system.allNotes[system.currentNote])
        {
            case 20:
                if (!system.usingSharpScale)
                {
                    ledgerLine = noteObj.transform.GetChild(0);
                    if (ledgerLine.transform.localPosition.x < 0)
                        ledgerLine.transform.localPosition *= -1;
                    ledgerLine.gameObject.SetActive(true);
                }
                break;
            case 21:
            case 22:
                if (system.usingSharpScale)
                {
                    if (ledgerLine.transform.localPosition.x < 0)
                        ledgerLine.transform.localPosition *= -1;
                    ledgerLine.gameObject.SetActive(true);
                }
                else
                {
                    noteObj.transform.GetChild(1).gameObject.SetActive(true);
                }
                break;
            case 23:
                noteObj.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case 0:
            case 1:
                if (!system.currentNoteString.Contains("b"))
                {
                    ledgerLine = noteObj.transform.GetChild(0);
                    if (ledgerLine.transform.localPosition.x > 0)
                        ledgerLine.transform.localPosition *= -1;
                    ledgerLine.gameObject.SetActive(true);
                }
                break;
        }

        //if there is a sharp or flat then determine which one and show the enharmonic sprite
        if (
            system
                .sharpsAndFlats
                .Contains(system.allNotes[system.currentNote]) &&
            !system.usingScaleMode
        )
        {
            if (system.usingSharpScale)
            {
                enharmonicPlacement.sprite = system.sharpSymbol;
            }
            else if (!system.usingSharpScale)
            {
                enharmonicPlacement.sprite = system.flatSymbol;
            }
        }
        else
        {
            enharmonicPlacement.sprite = null;
        }

        //resets the time allowed to guess on each successful press
        if (!timerMode)
            if (startTime >= 1)
            {
                timeToGuess = startTime;
            }
    }

    //OPTIONS
    public void NotationSwitch()
    {
        if (notationDisplay.activeInHierarchy)
        {
            notationDisplay.SetActive(false);
            noteObj.SetActive(false);
            noteText.enabled = true;
        }
        else
        {
            notationDisplay.SetActive(true);
            noteObj.SetActive(true);
            noteText.enabled = false;
        }
    }

    public void ColourSwitch()
    {
        if (useColour)
        {
            useColour = false;
        }
        else
            useColour = true;
    }

    public void TimerSwitch()
    {
        if (timerMode)
        {
            timerMode = false;
        }
        else
            timerMode = true;
    }

    public void UpdateTimeToGuess(InputField guessTime)
    {
        float time = float.Parse(guessTime.text);
        timeToGuess = time;
        totalTime = time;
    }

    protected bool GameOver() => timeToGuess < 0 || totalTime < 0;
}
