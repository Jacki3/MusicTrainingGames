using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoteDodgerController : MonoBehaviour
{
    public NoteFlashView notesController;

    public Conductor theConductor;

    public ScaleLoop keyConductor;

    public EnemyNoteDodger enemyNote;

    public NoteDodger noteObj;

    public RhythmDetector rhythmDetector;

    public int startMIDINumber = 60;

    public float speedMultiplier;

    public float spawnWait;

    public float doubleSpeedTime = 3000;

    public bool usingRhythm;

    public bool canHit;

    public bool canDestroy;

    public bool usingTapMode;

    public int totalLives = 5;

    public float score;

    public TextMeshProUGUI scoreText;

    public List<Transform> notePositionsSharp = new List<Transform>();

    public List<Image> hearts = new List<Image>();

    public EnemyNoteDodger pointPickup;

    public EnemyNoteDodger healthPickup;

    public bool adjacentMode;

    public bool useColour = false;

    public SpriteRenderer flashBackground;

    public float maxDiffMultiplier = 4;

    public int enemiesToMultiply = 12;

    public float enemySpawnX;

    public GameObject rhythmLine;

    public float startingHandicap = 4;

    public bool differentNoteEachTime = true;

    [HideInInspector]
    public List<float> notePositionsY = new List<float>();

    float t;

    Color32 defaultCamColor;

    private int randNote;

    private int currentNote;

    private bool gameStarted = false;

    private float elapsedGameTime;

    private float timeScore;

    private int totalEnemiesSpawned;

    public void Start()
    {
        defaultCamColor = flashBackground.color;

        foreach (Transform spawnPos in notePositionsSharp)
        {
            notePositionsY.Add(spawnPos.position.y);
        }
    }

    private void Update()
    {
        if (gameStarted)
        {
            elapsedGameTime = Time.time + Time.deltaTime;
            timeScore = elapsedGameTime / speedMultiplier;
        }

        scoreText.text = Mathf.FloorToInt(timeScore + score).ToString();

        if (Input.GetKeyUp(KeyCode.Space) && notesController.allNotes.Count > 0)
        {
            currentNote = notesController.allNotes[0] + startMIDINumber;
            spawnWait = theConductor.secPerBeat * startingHandicap;

            gameStarted = true;
            theConductor.lastBeat = 0;
            theConductor.StartSong();
            var tempY = noteObj.transform.position;
            tempY.y =
                notePositionsSharp[notesController.allNotes[0]].position.y;
            noteObj.transform.position = tempY;
            StartCoroutine(SpawnEnemyNotes());

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

        if (speedMultiplier < maxDiffMultiplier && !usingRhythm)
            speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;

        if (noteObj.GetComponent<RhythmDetector>().canHit)
        {
            canHit = true;
        }
        else
            canHit = false;

        flashBackground.color =
            Color.Lerp(flashBackground.color, defaultCamColor, t);
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

        if (usingRhythm)
            rhythmLine.transform.position = new Vector3(8, 0, 1);
        else
            rhythmLine.transform.position = new Vector3(12, 0, 1);

        if (usingTapMode && speedMultiplier >= 4) speedMultiplier = 4; //stuck at quarter notes
    }

    public void PlayNote(int note)
    {
        //needed for ryhthm tap mode
        if (canHit) canDestroy = true;
        if (!usingTapMode || rhythmDetector.canHit)
        {
            if (
                !adjacentMode &&
                notesController.allNotes.Contains(note - startMIDINumber) ||
                AreNotesScaleAdjacent(note, currentNote)
            )
            {
                currentNote = note;

                var tempY = noteObj.transform.position;
                tempY.y = notePositionsSharp[note - startMIDINumber].position.y;
                noteObj.transform.position = tempY;
            }
        }
    }

    private IEnumerator SpawnEnemyNotes()
    {
        if (!differentNoteEachTime)
            randNote = Random.Range(0, notesController.allNotes.Count);
        else
            randNote = notesController.currentNote;

        var enemyObj =
            Instantiate(enemyNote,
            new Vector3(enemySpawnX,
                notePositionsSharp[notesController.allNotes[randNote]]
                    .position
                    .y,
                1),
            Quaternion.identity);
        if (useColour)
        {
            enemyObj.GetComponent<SpriteRenderer>().color =
                notesController
                    .noteColours[notesController.allNotes[randNote] % 12];
        }
        enemyObj.controller = this;
        notesController.GetNewNote();
        notesController.patternIndex++;

        totalEnemiesSpawned++;
        if (usingRhythm)
        {
            if (
                totalEnemiesSpawned >= enemiesToMultiply &&
                speedMultiplier < maxDiffMultiplier
            )
            {
                enemiesToMultiply += enemiesToMultiply;
                speedMultiplier *= 2;
            }
        }
        yield return new WaitForSeconds(spawnWait / speedMultiplier);

        // if (spawnWait > .4f && !usingRhythm) spawnWait /= speedMultiplier;
        StartCoroutine(SpawnEnemyNotes());

        if (Random.value > .95f)
            StartCoroutine(SpawnPickups(pointPickup, 4, 3));
        else if (Random.value > .7f)
            StartCoroutine(SpawnPickups(pointPickup, 2, 1));
    }

    private IEnumerator
    SpawnPickups(EnemyNoteDodger pickup, int noteLength, int spawnAmount)
    {
        randNote = Random.Range(0, notesController.allNotes.Count); //DOESNT WORK WITH PATTERNS! might be cool to spawn pickups OUTSIDE of note range (stretch pinky etc.)
        print (randNote);
        yield return new WaitForSeconds(spawnWait / noteLength);

        for (int i = 0; i < spawnAmount; i++)
        {
            var pickupObj =
                Instantiate(pickup,
                new Vector3(12,
                    notePositionsSharp[notesController.allNotes[randNote]]
                        .position
                        .y,
                    1),
                Quaternion.identity);

            pickupObj.controller = this;
            yield return new WaitForSeconds(spawnWait / noteLength);
        }
    }

    public void AddScore(float newScore)
    {
        score += newScore * speedMultiplier;
    }

    public void RemoveLife()
    {
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

    public void AddLife()
    {
        totalLives++;
        for (int i = hearts.Count; i-- > 0; )
        if (!hearts[i].enabled)
        {
            hearts[i].enabled = true;
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
        if (usingTapMode)
            usingTapMode = false;
        else
            usingTapMode = true;
    }

    public void UpdateColourSwitch()
    {
        if (useColour)
            useColour = false;
        else
            useColour = true;
    }

    public void UpdateAdjMode()
    {
        if (adjacentMode)
            adjacentMode = false;
        else
            adjacentMode = true;
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

    public void UniqueNoteSwitch()
    {
        if (differentNoteEachTime)
            differentNoteEachTime = false;
        else
            differentNoteEachTime = true;
    }
}
