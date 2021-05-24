using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//idea: should firing off rhythm (in rhythm mode) make the enemies harder to destroy? i.e. i shot on time so they get 1 shot otherwise it is 3 shots?
//make enemies and next enemies much more obvious
//different types of enemies and enemies that spawn in patterns + weapon types or powerups that help to defeat them (this can be how levels vary) - the loop must get more complex over time
//items which make it easier; shields, add letters to notes etc. this is meta ^
//slow mo rewinds to see where you went wrong or stop and highlight etc.
//animate the colour change MORE
//if they start to lag behind how do we keep em up? checkpoints in levels help
//SHOULD we allow users to play with no increase in speed?
//better/less hard coded system of increasing song speed over time?
public class LineSwitcherController : MonoBehaviour
{
    [Header("Audio Helm")]
    public AudioHelm.AudioHelmClock audioHelmClock;

    public AudioHelm.SequenceGenerator sequenceGenerator;

    public AudioHelm.Sequencer helmSequencer;

    public GameObject[] drumMachines;

    [Header("System")]
    public NoteFlashView notesController;

    public Conductor theConductor;

    public ScaleLoop keyConductor;

    public int startMIDINumber = 60;

    public float score = 0;

    [Header("Game Settings")]
    public bool usingRhythm = false; //AKA tap mode

    public bool useColour = true;

    public bool rhythmShootMode;

    public float spawnWait;

    public float speedMultiplier;

    public float doubleSpeedTime = 60;

    public float elapsedGameTime = 1;

    public int totalLives = 3;

    public float startingHandicap = 4;

    public float maxDifficultyMultiplier = 4;

    public int enemyNotesToMultiply = 4;

    public float posX;

    [Header("UI & Objects")]
    public NoteShooter noteObj;

    public NoteProjectile enemyNote;

    public ScoreGainEffect gainEffect;

    public TextMeshProUGUI scoreText;

    public SpriteRenderer flashback;

    public GameObject speedLine;

    public TextMeshProUGUI correctNotesText;

    public TextMeshProUGUI inCorrectNotesText;

    public TextMeshProUGUI streakText;

    public List<Image> hearts = new List<Image>();

    public List<Transform> notePositionsSharp = new List<Transform>();

    public List<Transform> notePositionsFlat = new List<Transform>();

    public bool canHit = false;

    [HideInInspector]
    public bool gameStarted = false;

    private Color32 defaultCamColor;

    private float t = 1;

    private int totalEnemiesSpawned;

    [HideInInspector]
    public int totalCorrect = 0;

    [HideInInspector]
    private int totalIncorrect = 0;

    [HideInInspector]
    public int streakScore = 0;

    public class IntEvent : UnityEvent<int, float> { }

    public IntEvent noteOn = new IntEvent();

    private GameObject currentDrumMachine;

    public List<Transform> currentNotePositions = new List<Transform>();

    public void Start()
    {
        defaultCamColor = flashback.color;
        currentNotePositions = notePositionsSharp;

        StartCoroutine(StartAnimation());

        audioHelmClock.bpm = theConductor.songBPM;
    }

    IEnumerator StartAnimation()
    {
        int startNote = startMIDINumber + 23;

        for (int i = 0; i < 24; i++)
        {
            PlayNote (startNote);
            noteOn.Invoke(startNote, .1f);
            startNote--;
            yield return new WaitForSeconds(.03f);
        }

        yield return null;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F)) StartCoroutine(StartAnimation());

        if (gameStarted)
        {
            elapsedGameTime += Time.deltaTime;
            if (!rhythmShootMode)
                audioHelmClock.bpm = theConductor.songBPM + elapsedGameTime / 5;
        }

        scoreText.text = Mathf.FloorToInt(score).ToString();

        if (speedMultiplier < maxDifficultyMultiplier && !rhythmShootMode)
            speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;

        if (Input.GetKeyUp(KeyCode.Space) && notesController.allNotes.Count > 0)
        {
            sequenceGenerator.scale = notesController.allNotes.ToArray();
            sequenceGenerator.Generate();
            gameStarted = true;
            if (rhythmShootMode)
                startingHandicap = 4; //This sets the rhythm shoot mode to WHOLE notes to begin with - setting it to = 1/2; 4 = 1/4 etc. this is controlled by a global difficulty
            else
                startingHandicap = 1.5f; // difficulty could change this?
            notesController.HideChoices();
            spawnWait = theConductor.secPerBeat * startingHandicap;
            theConductor.lastBeat = 0;
            theConductor.StartSong();
            StartCoroutine(SpawnEnemyNotes(1));
        }

        if (noteObj.transform.GetChild(0).GetComponent<RhythmDetector>().canHit)
        {
            canHit = true;
        }
        else
            canHit = false;

        flashback.color = Color.Lerp(flashback.color, defaultCamColor, t);
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

        if (rhythmShootMode)
            speedLine.transform.position = new Vector3(10, 0, 1);
        else
            speedLine.transform.position = new Vector3(14.2f, 0, 1);

        if (usingRhythm && speedMultiplier >= 4) speedMultiplier = 4; //stuck at quarter notes if players want to tap; game is impossible otherwise

        correctNotesText.text = totalCorrect.ToString();
        inCorrectNotesText.text = totalIncorrect.ToString();
        streakText.text = streakScore.ToString();

        if (notesController.usingSharpScale)
            currentNotePositions = notePositionsSharp;
        else
            currentNotePositions = notePositionsFlat;
    }

    public void PlayNote(int note)
    {
        var tempY = noteObj.transform.position;
        tempY.y = currentNotePositions[note - startMIDINumber].position.y;
        noteObj.transform.position = tempY;
        noteObj.FireNoteProjectile (note);
    }

    private IEnumerator SpawnEnemyNotes(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int randIndex = Random.Range(0, notesController.allNotes.Count);

            notesController.GetNewNote();
            int patternIndex = notesController.currentNote;

            int currentIndex = 0;

            if (notesController.usePatterns || enemiesToSpawn > 1)
                currentIndex = patternIndex;
            else
                currentIndex = randIndex;

            var newEnemy =
                Instantiate(enemyNote,
                new Vector3(posX,
                    currentNotePositions[notesController.allNotes[currentIndex]]
                        .position
                        .y,
                    1),
                Quaternion.identity);
            newEnemy.controller = this;
            newEnemy.gainEffect = gainEffect;
            newEnemy.note = notesController.allNotes[currentIndex];

            if (useColour)
            {
                var color =
                    notesController
                        .noteColours[notesController.allNotes[currentIndex] %
                        12]; //change this if you want notes to come out WHITE or COLOURED

                newEnemy.GetComponent<SpriteRenderer>().color = color;
                newEnemy
                    .GetComponent<SpriteRenderer>()
                    .material
                    .SetColor("_EmissionColor", color * 1.2f);
            }
            else
                newEnemy.GetComponent<SpriteRenderer>().color = Color.black;
        }

        totalEnemiesSpawned++;
        if (rhythmShootMode)
        {
            if (
                totalEnemiesSpawned >= enemyNotesToMultiply &&
                speedMultiplier < maxDifficultyMultiplier
            )
            {
                enemyNotesToMultiply += enemyNotesToMultiply;
                speedMultiplier *= 2;
                // StartCoroutine(IncreaseDifficultySlowly(10, speedMultiplier * 2)); -- could still be useful
            }
        }
        notesController.patternIndex++;

        yield return new WaitForSeconds(spawnWait / speedMultiplier);

        if (totalEnemiesSpawned < 12)
            StartCoroutine(SpawnEnemyNotes(1));
        else
        {
            if (Random.value > .85f && notesController.allNotes.Count >= 2)
            {
                StartCoroutine(SpawnEnemyNotes(2));
            }
            else if (Random.value > .98f && notesController.allNotes.Count >= 3)
                StartCoroutine(SpawnEnemyNotes(3));
            else
                StartCoroutine(SpawnEnemyNotes(1));
        }
    }

    IEnumerator IncreaseDifficultySlowly(float timeToIncrease, float distance)
    {
        float timeToTakePerIncrease =
            (distance - speedMultiplier) / timeToIncrease;
        float waitTime = timeToTakePerIncrease * timeToIncrease;

        while (speedMultiplier < distance)
        {
            speedMultiplier += timeToTakePerIncrease;
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void RemoveLife()
    {
        totalIncorrect++;
        streakScore = 0;

        flashback.color = Color.red;
        t = 0;
        foreach (Image heart in hearts)
        if (heart.enabled)
        {
            totalLives--;
            heart.enabled = false;
            break;
        }
    }

    public void UpdateBPM(InputField value)
    {
        float newBPM = float.Parse(value.text);
        if (newBPM > 0) theConductor.songBPM = newBPM;
        theConductor.secPerBeat = 60 / theConductor.songBPM;
        audioHelmClock.bpm = newBPM;
    }

    public void UpdateRhythmToggle()
    {
        if (usingRhythm)
            usingRhythm = false;
        else
            usingRhythm = true;
    }

    public void UpdateMusicVolume()
    {
        if (helmSequencer.enabled)
        {
            helmSequencer.enabled = false;
            foreach (GameObject drumMachine in drumMachines)
            {
                if (drumMachine.activeSelf)
                {
                    drumMachine.SetActive(false);
                    currentDrumMachine = drumMachine;
                }
            }
        }
        else
        {
            helmSequencer.enabled = true;
            currentDrumMachine.SetActive(true);
        }
    }

    public void UpdateMetroVolume()
    {
        if (theConductor.metronome.volume > 0)
        {
            theConductor.metronome.volume = 0;
        }
        else
            theConductor.metronome.volume = .35f;
    }

    public void UpdateColourSwitch()
    {
        if (useColour)
            useColour = false;
        else
            useColour = true;
    }

    public void UpdateRhythmShootMode()
    {
        if (rhythmShootMode)
            rhythmShootMode = false;
        else
            rhythmShootMode = true;
    }

    public void RegenerateMusic()
    {
        if (notesController.allNotes.Count > 0)
            sequenceGenerator.scale = notesController.allNotes.ToArray();
        sequenceGenerator.Generate();
    }

    protected bool GameOver() => totalLives <= 0;
}
