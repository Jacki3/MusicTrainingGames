using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//TO DO:
//tidy
//add 'bonus' non essential notes at random
//add additional and increasingly complex notes in infinite mode?
//choice: destroy/collect notes based on error OR allow them another go (add ability to do both - implement collect notes and allow another go)
public class RhythmDotsController : MonoBehaviour
{
    [Header("Game Settings")]
    public int notesToSpawn;

    public int totalLives = 3;

    public int cyclesToComplete = 3;

    public int totalNotesToShow = 4;

    public bool usingNotation;

    public bool useColour = true;

    public bool increaseDifficulty = false;

    public float doubleSpeedTime = 60;

    //for alternative diff increase mode
    public float increaseDifficultyAmount;

    public int difficultyCycleAmount = 1; //after this many cycles increase difficulty

    [Tooltip("Reference Purposes")]
    public float speedMultiplier = 1;

    [Header("UI & Objects")]
    public TextMeshProUGUI scoreText;

    public RhythmDotNoteNode noteObj;

    public RhythmBarCollision currentNoteObj;

    public GameObject winText;

    public TextMeshProUGUI cyclesLeftText;

    public Image multiplierBar;

    public List<GameObject> hearts = new List<GameObject>();

    public List<Image> keyStickers = new List<Image>();

    public List<Sprite> sharpNotationImages = new List<Sprite>();

    public List<Sprite> flatNotationImages = new List<Sprite>();

    [Header("System")]
    public NoteFlashView notesController;

    public Conductor theConductor;

    public int currentNote = -1;

    public int startMIDINumber = 60;

    [HideInInspector]
    public bool canDestroy = true;

    public List<Transform> noteSpawnPositionsRight = new List<Transform>();

    public List<Transform> noteSpawnPositionsLeft = new List<Transform>();

    private float totalScore;

    private float scoreMultiplier;

    private float elapsedGameTime = 1;

    private List<Sprite> notationImages = new List<Sprite>();

    private Color32 defaultCamColor;

    [HideInInspector]
    public float t = 1;

    private float multiplierFillAmount;

    //for alternative diff mode
    private int defaultCycleAmount;

    private bool infiniteLevelMode;

    private bool gameStarted;

    private void Start()
    {
        defaultCamColor = Camera.main.backgroundColor;
        defaultCycleAmount = difficultyCycleAmount;
    }

    private void Update()
    {
        int cyclesLeft;
        if (!infiniteLevelMode)
            cyclesLeft = cyclesToComplete - theConductor.cycles;
        else
            cyclesLeft = theConductor.cycles;
        cyclesLeftText.text = cyclesLeft.ToString();

        scoreText.text = Mathf.FloorToInt(totalScore) + "x" + scoreMultiplier;

        if (notesController.usingSharpScale)
            notationImages = sharpNotationImages;
        else
            notationImages = flatNotationImages;

        Camera.main.backgroundColor =
            Color.Lerp(Camera.main.backgroundColor, defaultCamColor, t);
        if (t < 1) t += Time.deltaTime / 15;

        if (GameOver() || LevelComplete())
        {
            if (LevelComplete()) winText.SetActive(true);
            Time.timeScale = 0;
            theConductor.enabled = false;
        }
        if (Input.GetKey(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (multiplierFillAmount >= .4f && multiplierFillAmount < .8f)
            scoreMultiplier = 2;
        else if (multiplierFillAmount >= .8f)
            scoreMultiplier = 4;
        else
            scoreMultiplier = 1;

        multiplierBar.fillAmount = multiplierFillAmount;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            notesController.HideChoices();
            gameStarted = true;
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
        }
        if (increaseDifficulty && gameStarted)
        {
            elapsedGameTime += Time.deltaTime;
            speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;
        }
    }

    public void PlayNote(int note)
    {
        if (currentNote >= 0)
        {
            if (note == currentNote + startMIDINumber)
            {
                canDestroy = false;
                Destroy(currentNoteObj.gameObject);
                Camera.main.backgroundColor = Color.green;
                t = 0;
                AddScore(50);
                if (multiplierFillAmount < 1)
                {
                    multiplierFillAmount += .2f;
                }
            }
            else
            {
                StartCoroutine(ResetNoteBar(.5f));
                RemoveHeart();
                Camera.main.backgroundColor = Color.red;
                t = 0;
            }
        }
        if (theConductor.cycles % 2 == 0 && theConductor.cycles > 0)
        {
            if (notesToSpawn < noteSpawnPositionsLeft.Count) notesToSpawn++;
        }
    }

    public IEnumerator ResetNoteBar(float timeToMove)
    {
        var currentBarValue = multiplierFillAmount;
        var t = 0f;
        while (t < 1)
        {
            scoreMultiplier = 1;
            t += Time.deltaTime / timeToMove;
            multiplierFillAmount = Mathf.Lerp(currentBarValue, 0, t);
            yield return null;
        }
    }

    public void SpawnNote(bool isRight)
    {
        if (isRight)
        {
            StartCoroutine(SpawnActualNote(noteSpawnPositionsRight));
        }
        else
        {
            StartCoroutine(SpawnActualNote(noteSpawnPositionsLeft));
        }
    }

    private IEnumerator SpawnActualNote(List<Transform> noteSpawns)
    {
        Vector3 randTransform = Vector3.zero;

        int index = Random.Range(0, noteSpawns.Count);
        noteSpawns =
            noteSpawns
                .SkipWhile(x => x != noteSpawns[index])
                .Concat(noteSpawns.TakeWhile(x => x != noteSpawns[index]))
                .ToList();

        for (int i = 0; i < notesToSpawn; i++)
        {
            randTransform =
                new Vector3(noteSpawns[i].position.x,
                    noteSpawns[i].position.y,
                    0);

            var dotNote =
                Instantiate(noteObj, randTransform, Quaternion.identity);

            notesController.GetNewNote();
            notesController.patternIndex++;
            dotNote.controller = this;
            dotNote.note =
                notesController.allNotes[notesController.currentNote];
            dotNote.noteName = notesController.currentNoteString;
            if (useColour)
            {
                dotNote.noteColour =
                    notesController
                        .noteColours[notesController
                            .allNotes[notesController.currentNote] %
                        12];
            }
            else
                dotNote.noteColour = Color.white;
            dotNote.notationImage =
                notationImages[notesController
                    .allNotes[notesController.currentNote]];
            dotNote.GetComponent<RhythmBarCollision>().dotsController = this;

            yield return new WaitForSeconds(theConductor.secPerBeat / 2);
        }
    }

    public void RemoveHeart()
    {
        totalLives--;
        foreach (GameObject heart in hearts)
        if (heart.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            heart.transform.GetChild(0).gameObject.SetActive(false);
            break;
        }
    }

    public void AddScore(float scoreToAdd)
    {
        totalScore += scoreToAdd * scoreMultiplier;
    }

    public void UpdateCyclesToComplete(InputField value)
    {
        float cycles = float.Parse(value.text);
        if (cycles <= 0)
            infiniteLevelMode = true;
        else
            infiniteLevelMode = false;
        cyclesToComplete = (int) cycles; //why is this float?
    }

    public void UpdateBPM(InputField value)
    {
        float newBeat = float.Parse(value.text);
        if (newBeat > 0) theConductor.songBPM = newBeat;
        theConductor.secPerBeat = 60 / theConductor.songBPM;
    }

    public void UpdateNotesToShow(TextMeshProUGUI value)
    {
        float notes = float.Parse(value.text);
        totalNotesToShow = (int) notes;
    }

    public void MuteMetroSwitch()
    {
        if (theConductor.metronome.volume <= 0)
            theConductor.metronome.volume = .35f;
        else
            theConductor.metronome.volume = 0;
    }

    public void UpdateMusicVolume()
    {
        if (theConductor.musicSource.volume > 0)
        {
            theConductor.musicSource.volume = 0;
        }
        else
            theConductor.musicSource.volume = .55f;
    }

    public void UpdateNotationSwitch()
    {
        if (usingNotation)
            usingNotation = false;
        else
            usingNotation = true;
    }

    public void UpdateDifficultySwitch()
    {
        if (increaseDifficulty)
            increaseDifficulty = false;
        else
            increaseDifficulty = true;
    }

    public void UpdateColourSwitch()
    {
        if (useColour)
            useColour = false;
        else
            useColour = true;
    }

    protected bool GameOver() => totalLives <= 0;

    protected bool LevelComplete() =>
        theConductor.cycles > cyclesToComplete - 1 && !infiniteLevelMode;
}
