using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//tidy and headers
//multipliers on streaks (applies to others)
//add tooltips (applies to others)
//choice: penalty on incorrect notes too? avoids spam?
//choice: destroy missed notes (after so many misses?) and collect then show after OR allow user to try as many times as they like (add ability to do both)
public class SonarNotesController : MonoBehaviour
{
    [Header("Game Settings")]
    public bool usingNotation = true;

    public bool useColour = true;

    public bool infiniteLevelMode = false;

    public bool increaseDifficulty = false;

    public float doubleSpeedTime = 60;

    public int noteLengths = 8;

    public int totalLives;

    public int cyclesToComplete;

    [Header("UI & Objects")]
    public TextMeshProUGUI overText;

    public Rotator tickLine;

    public SonarNote noteObj;

    public GameObject mainCanvas;

    public TextMeshProUGUI cyclesToCompleteText;

    public TextMeshProUGUI totalCorrectText;

    public TextMeshProUGUI totalInCorrectText;

    public TextMeshProUGUI streakText;

    public SpawnerCollider beatIndicator;

    public Transform beatIndicatorSpawn;

    public List<Sprite> sharpNotations = new List<Sprite>();

    public List<Sprite> flatNotations = new List<Sprite>();

    public List<GameObject> hearts = new List<GameObject>();

    [Tooltip("For Reference Purpose")]
    public List<SonarNote> spawnedNotes = new List<SonarNote>();

    [Tooltip("For Reference Purpose")]
    public List<SpawnerCollider> spawnPositions = new List<SpawnerCollider>();

    [Header("System")]
    public Conductor theConductor;

    public NoteFlashView notesController;

    public int startMIDINumber = 60;

    private List<Sprite> notationImages = new List<Sprite>();

    private Color32 defaultCamColor;

    private float t = 1;

    private float elapsedGameTime = 1;

    private float speedMultiplier = 1;

    private bool gameStarted = false;

    private int totalCorrect = 0;

    private int totalInCorrect = 0;

    private int streakScore = 0;

    private void Start()
    {
        //set default cam colour here or it will be black
    }

    private void Update()
    {
        int cyclesLeft;
        if (!infiniteLevelMode)
            cyclesLeft = cyclesToComplete - theConductor.cycles;
        else
            cyclesLeft = cyclesToComplete + theConductor.cycles;

        cyclesToCompleteText.text = cyclesLeft.ToString();

        if (notesController.usingSharpScale)
            notationImages = sharpNotations;
        else
            notationImages = flatNotations;

        //constantly checking to see if user missed a note
        foreach (SonarNote spawnedNote in spawnedNotes)
        {
            if (spawnedNote.tickLeft == true)
            {
                spawnedNote.tickLeft = false;

                spawnedNote
                    .transform
                    .parent
                    .GetComponent<SpawnerCollider>()
                    .canSpawn = false;
                totalLives--;
                RemoveHeart();

                //--see choice above--
                // spawnedNotes.Remove (spawnedNote);
                // Destroy(spawnedNote.gameObject);
            }
        }

        //start game if there are notes to show
        if (Input.GetKeyUp(KeyCode.Space) && notesController.allNotes.Count > 0)
        {
            // organise scales based on octaves
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

            theConductor.StartSong();

            //sets the spawns
            InvokeRepeating("HitBeat",
            0,
            theConductor.secPerBeat / (noteLengths / 4));

            tickLine.degrees =
                -360 / (theConductor.secPerBeat * 4 / speedMultiplier);

            notesController.HideChoices();
            gameStarted = true;
        }

        if (gameStarted && increaseDifficulty)
        {
            elapsedGameTime += Time.deltaTime;
            speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;
        }

        if (LevelComplete())
        {
            tickLine.degrees = 0;
            foreach (SonarNote spawnedNote in spawnedNotes)
            {
                spawnedNote.gameObject.SetActive(false);
            }
            overText.text = "winner! \n press 'r' to restart";
        }

        if (GameOver())
        {
            tickLine.degrees = 0;
            foreach (SonarNote spawnedNote in spawnedNotes)
            {
                spawnedNote.gameObject.SetActive(false);
            }
            overText.color = Color.red;
            overText.text = "game over... \n press 'r' to restart";

            //show restart text
            Time.timeScale = 0;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        Camera.main.backgroundColor =
            Color.Lerp(Camera.main.backgroundColor, defaultCamColor, t);
        if (t < 1) t += Time.deltaTime / 10;

        totalCorrectText.text = totalCorrect.ToString();
        totalInCorrectText.text = totalInCorrect.ToString();
        streakText.text = streakScore.ToString();
    }

    public void RemoveHeart()
    {
        foreach (GameObject heart in hearts)
        if (heart.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            Camera.main.backgroundColor = Color.red;
            t = 0;
            heart.transform.GetChild(0).gameObject.SetActive(false);
            break;
        }
    }

    public void HitBeat()
    {
        var beatObj =
            Instantiate(beatIndicator,
            beatIndicatorSpawn.transform.position,
            Quaternion.identity);

        beatObj.controller = this;
        beatObj.conductor = theConductor;

        beatObj.transform.SetParent(mainCanvas.transform);
        spawnPositions.Add (beatObj);

        if (--noteLengths == 0) CancelInvoke("HitBeat");
    }

    public void PlayNote(int note)
    {
        foreach (SonarNote spawnedNote in spawnedNotes)
        {
            if (spawnedNote.note == note && spawnedNote.isPlayable)
            {
                totalCorrect++;
                streakScore++;
                spawnedNotes.Remove (spawnedNote);
                Destroy(spawnedNote.gameObject);

                spawnedNote
                    .transform
                    .parent
                    .GetComponent<SpawnerCollider>()
                    .canSpawn = true;

                break;
            }
            else if (!GameOver() && !LevelComplete())
            {
                //add penalty?
                totalInCorrect++;
                streakScore = 0;
            }
        }
    }

    public void SpawnNote(SpawnerCollider spawnerCollider)
    {
        int index = spawnPositions.IndexOf(spawnerCollider);

        spawnPositions =
            spawnPositions
                .SkipWhile(x => x != spawnPositions[index])
                .Concat(spawnPositions
                    .TakeWhile(x => x != spawnPositions[index]))
                .ToList();

        if (spawnerCollider.canSpawn)
        {
            var sonarNote =
                Instantiate(noteObj,
                spawnPositions[index + 1].transform.position,
                Quaternion.identity);

            sonarNote.transform.SetParent(spawnerCollider.transform);
            spawnedNotes.Add (sonarNote);

            notesController.GetNewNote();
            notesController.patternIndex++;

            sonarNote.controller = this;
            sonarNote.noteString = notesController.currentNoteString;
            if (useColour)
                sonarNote.noteColour = notesController.currentNoteColour;

            sonarNote.notationSprite =
                notationImages[notesController
                    .allNotes[notesController.currentNote]];
            sonarNote.note =
                notesController.allNotes[notesController.currentNote] +
                startMIDINumber;
        }
    }

    //--Menu Controls--//
    public void SetNoteLengths(TextMeshProUGUI noteLength)
    {
        float newLengths = float.Parse(noteLength.text);
        noteLengths = (int) newLengths;
    }

    public void SetBPM(InputField value)
    {
        float newBPM = float.Parse(value.text);
        if (newBPM > 0) theConductor.songBPM = newBPM;
    }

    public void SetCyclesToComplete(InputField value)
    {
        float newCycles = float.Parse(value.text);
        if (newCycles <= 0)
            infiniteLevelMode = true;
        else
            infiniteLevelMode = false;
        cyclesToComplete = (int) newCycles;
    }

    public void SetNotationSwitch()
    {
        if (usingNotation)
            usingNotation = false;
        else
            usingNotation = true;
    }

    public void UpdateMetroSwitch()
    {
        if (theConductor.metronome.volume > 0)
            theConductor.metronome.volume = 0;
        else
            theConductor.metronome.volume = .4f;
    }

    public void UpdateMusicSwitch()
    {
        if (theConductor.musicSource.volume > 0)
            theConductor.musicSource.volume = 0f;
        else
            theConductor.musicSource.volume = .6f;
    }

    public void UpdateColourSwitch(Toggle colourToggle)
    {
        if (useColour)
        {
            useColour = false;
            colourToggle.GetComponentInChildren<Text>().text = "No Colour";
        }
        else
        {
            useColour = true;
            colourToggle.GetComponentInChildren<Text>().text = "Colour!";
        }
    }

    public void UpdateSpeedToggle()
    {
        if (increaseDifficulty)
            increaseDifficulty = false;
        else
            increaseDifficulty = true;
    }

    protected bool GameOver() => totalLives <= 0;

    protected bool LevelComplete() =>
        theConductor.cycles > cyclesToComplete - 1 && !infiniteLevelMode;
}
