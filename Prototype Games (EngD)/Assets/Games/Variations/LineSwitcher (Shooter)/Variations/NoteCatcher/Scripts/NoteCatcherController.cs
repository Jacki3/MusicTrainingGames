using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoteCatcherController : MonoBehaviour
{
    public NoteFlashView notesController;

    public Conductor theConductor;

    public ScaleLoop keyConductor;

    public EnemyNoteCatcherMovement enemyNote;

    public NoteCatcher noteObj;

    public int startMIDINumber = 60;

    public float speedMultiplier;

    public float spawnWait;

    public float doubleSpeedTime = 3000;

    public bool usingRhythm;

    public bool canHit;

    public bool canDestroy;

    public bool usingTapMode = false;

    public int totalLives = 5;

    public float score;

    public TextMeshProUGUI scoreText;

    public List<Transform> notePositionsSharp = new List<Transform>();

    public List<Image> hearts = new List<Image>();

    public EnemyNoteCatcherMovement pointPickup;

    public EnemyNoteCatcherMovement healthPickup;

    public bool adjacentMode;

    public bool useColour = false;

    public SpriteRenderer flashBack;

    public float posX = 12;

    public float difficultyTime = 30;

    public Transform beatLine;

    public int enemiesToIncreaseSpawn;

    [HideInInspector]
    public List<float> notePositionsY = new List<float>();

    float t;

    Color32 defaultCamColor;

    private int randNote;

    private int currentNote;

    private bool gameStarted = false;

    public float elapsedGameTime = 1;

    float defaultDifficultyTime;

    bool firstShot = true;

    public List<int> currentNotes = new List<int>();

    int totalEnemiesSpawned;

    int defaultEnemiesToIncreaseSpawn;

    public void Start()
    {
        defaultCamColor = flashBack.color;
        defaultDifficultyTime = difficultyTime;
        defaultEnemiesToIncreaseSpawn = enemiesToIncreaseSpawn;

        foreach (Transform spawnPos in notePositionsSharp)
        {
            notePositionsY.Add(spawnPos.position.y);
        }
    }

    private void Update()
    {
        scoreText.text = Mathf.FloorToInt(score).ToString();

        if (Input.GetKeyUp(KeyCode.Space) && notesController.allNotes.Count >= 1
        )
        {
            gameStarted = true;
            randNote = Random.Range(0, notesController.allNotes.Count);
            currentNotes.Add(notesController.allNotes[randNote]);
            notesController.allNotes.Remove(notesController.allNotes[randNote]);
            currentNote = notesController.allNotes[0] + startMIDINumber;
            spawnWait = theConductor.secPerBeat * 4;

            theConductor.lastBeat = 0;
            theConductor.StartSong();
            var tempY = noteObj.transform.position;
            tempY.y =
                notePositionsSharp[notesController.allNotes[0]].position.y;
            noteObj.transform.position = tempY;
            StartCoroutine(SpawnEnemyNotes());
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
        }

        // if (spawnWait > .4f && !usingRhythm)
        //     speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;
        // if (noteObj.GetComponent<RhythmDetector>().canHit)
        // {
        //     canHit = true;
        // }
        // else
        //     canHit = false;
        flashBack.color = Color.Lerp(flashBack.color, defaultCamColor, t);
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

        if (gameStarted)
        {
            elapsedGameTime += Time.deltaTime;
        }

        if (
            elapsedGameTime >= difficultyTime //switch to total enemies spawned (Vs time)?
        )
        {
            if (speedMultiplier < 4)
                StartCoroutine(IncreaseDifficultySlowly(10,
                speedMultiplier * 2));

            // speedMultiplier *= 2;
            // if (difficultyTime < defaultDifficultyTime * 2)
            difficultyTime += defaultDifficultyTime;
        }
        if (
            totalEnemiesSpawned >= enemiesToIncreaseSpawn &&
            notesController.allNotes.Count >= 1
        )
        {
            print("chucking another enemy into the frey");
            enemiesToIncreaseSpawn += defaultEnemiesToIncreaseSpawn;
            randNote = Random.Range(0, notesController.allNotes.Count);
            currentNotes.Add(notesController.allNotes[randNote]);
            notesController.allNotes.Remove(notesController.allNotes[randNote]);
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

    public void PlayNote(int note)
    {
        //needed for ryhthm tap mode
        if (canHit) canDestroy = true;

        if (!adjacentMode || AreNotesScaleAdjacent(note, currentNote))
        {
            currentNote = note;

            var tempY = noteObj.transform.position;
            tempY.y = notePositionsSharp[note - startMIDINumber].position.y;
            noteObj.transform.position = tempY;
        }
    }

    private IEnumerator SpawnEnemyNotes()
    {
        randNote = Random.Range(0, currentNotes.Count);

        var enemyObj =
            Instantiate(enemyNote,
            new Vector3(posX,
                notePositionsSharp[currentNotes[randNote]].position.y,
                1),
            Quaternion.identity);
        if (useColour)
        {
            enemyObj.GetComponent<SpriteRenderer>().color =
                notesController.noteColours[currentNotes[randNote] % 12];
        }
        enemyObj.controller = this;

        // notesController.GetNewNote();
        notesController.patternIndex++;
        totalEnemiesSpawned++;
        yield return new WaitForSeconds(spawnWait / speedMultiplier);

        // if (!usingRhythm) spawnWait /= speedMultiplier;
        // spawnWait > .4f &&
        StartCoroutine(SpawnEnemyNotes());

        {
            if (Random.value > .95f)
                StartCoroutine(SpawnPickups(pointPickup, 4, 3));
            else if (Random.value > .7f)
                StartCoroutine(SpawnPickups(pointPickup, 2, 1));
        }
    }

    private IEnumerator
    SpawnPickups(
        EnemyNoteCatcherMovement pickup,
        int noteLength,
        int spawnAmount
    )
    {
        randNote = Random.Range(0, currentNotes.Count); //DOESNT WORK WITH PATTERNS! might be cool to spawn pickups OUTSIDE of note range (stretch pinky etc.)
        yield return new WaitForSeconds(spawnWait / noteLength);

        for (int i = 0; i < spawnAmount; i++)
        {
            var pickupObj =
                Instantiate(pickup,
                new Vector3(12,
                    notePositionsSharp[currentNotes[randNote]].position.y,
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
        flashBack.color = Color.red;
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

    protected bool GameOver() => totalLives <= 0;
}
