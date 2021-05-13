using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//should we do a background?
//TO DO:
//particle system update deprecated code
//TO DO LATER:
//add on screen keyboard (adjust the whole screen for this)
//add phrases and spawn in different patterns + different types of enemies including bonus ones which wont lose lives
//DECIDE:
//play negative sound on incorrect note played?
//collect notes and return them back?
//TRY OUT WHEN YOU CAN:
//split screen?
//move player ship (lerp) to the pos before firing; ensuring players SHOULD follow the pattern to execute enemies in time (bonuses will effect this though) - should enemies spawn at certain heights?
//extra weapons such as EMP
public class AsteroidSpawner : MonoBehaviour
{
    [Header("Game Settings")]
    public bool usingNotation = true;

    public bool twoHands;

    public float startWait;

    public float spawnWait;

    public float waveWait;

    public int asteroidCount;

    public float speedMultiplier = 1;

    public float difficultyIncrement = .15f;

    public int wavesToComplete;

    public int totalLives = 3;

    public bool useColour;

    public bool usingRhythmBarMode;

    public bool infiniteMode = true;

    public float scoreMultiplier = 1;

    [Header("UI & Objects")]
    public AudioClip errorSound;

    public Mover enemyScript;

    public GameObject playerShip;

    public GameObject asteroid;

    public GameObject splitLine;

    public Vector3 spawnValues;

    public Transform boltSpawn;

    public GameObject noteBolt;

    public Image mutliBar;

    public Image errorBar;

    public TextMeshProUGUI waveCountText;

    public TextMeshProUGUI waveText;

    public TextMeshProUGUI correctNotesText;

    public TextMeshProUGUI inorrectNotesText;

    public TextMeshProUGUI streakScoreText;

    public TextMeshProUGUI scoreText;

    public ScoreGainEffect scoreGainEffect;

    public TextMeshProUGUI counterText;

    public ParticleSystem explosion;

    public ParticleSystem playerExplosion;

    public SpriteRenderer flashBackground;

    public Color32 flashColour;

    public Image[] hearts;

    public List<Sprite> sharpNotationImages = new List<Sprite>();

    public List<Sprite> flatNotationImages = new List<Sprite>();

    private List<Sprite> notationImages = new List<Sprite>();

    [Header("System")]
    public NoteFlashView noteSpawner;

    public int startMIDINumber = 60;

    public Conductor theConductor;

    public float distOfBars;

    public List<GameObject> spawnedAsteroids = new List<GameObject>();

    private float spawnDistance = 2;

    private BarMovement rhythmBar;

    private float noteLengths;

    private float mutliBarFillAmount;

    private bool canPlay = false;

    private int waveCount = 1;

    private float totalScore = 0;

    private float t = 1;

    private float timer;

    private PlayerCollision collision;

    private bool splitMode = false;

    private int totalCorrectNotes;

    private int totalIncorrectNotes;

    private int streakScore;

    private float errorAmount;

    private AudioSource aSource;

    private void Start()
    {
        aSource = GetComponent<AudioSource>();
        collision = playerShip.GetComponent<PlayerCollision>();
    }

    private void Update()
    {
        scoreText.text = Mathf.FloorToInt(totalScore) + "x" + scoreMultiplier;
        counterText.text = Mathf.FloorToInt(timer + 1).ToString();

        timer -= Time.deltaTime;

        if (timer <= 0)
            counterText.enabled = false;
        else
            counterText.enabled = true;

        if (Input.GetKeyUp(KeyCode.Space) && noteSpawner.allNotes.Count > 0)
        {
            // spawnWait = ((theConductor.secPerBeat * 4) / noteLengths) * 2f;
            // waveWait = spawnWait * 2;
            theConductor.StartSong();
            canPlay = true;
            noteSpawner.HideChoices();
            StartCoroutine(SpawnWaves());
            if (
                noteSpawner.usingScaleMode //handling octaves
            )
            {
                int endOfScale = noteSpawner.allNotes.IndexOf(11);
                if (endOfScale < 0)
                    endOfScale = noteSpawner.allNotes.IndexOf(10);
                for (int i = endOfScale + 1; i < noteSpawner.allNotes.Count; i++
                )
                {
                    noteSpawner.allNotes[i] = noteSpawner.allNotes[i] + 12;
                }
            }

            if (noteSpawner.usingSharpScale)
                notationImages = sharpNotationImages;
            else
                notationImages = flatNotationImages;
        }

        //multiplier (could potentially be more robust)
        if (mutliBarFillAmount >= .4f && mutliBarFillAmount < .8f)
            scoreMultiplier = 2;
        else if (mutliBarFillAmount >= .8f)
        {
            scoreMultiplier = 4;

            //filling up the multibar will reset user errors
            StartCoroutine(ResetErrorBar(.5f));
        }
        else
            scoreMultiplier = 1;

        //sets the two hand split if notes chosen go across octaves (either lower or higher on staff OR bass and treble chosen)
        if (
            noteSpawner.allNotes.Any(note => note > 11) &&
            noteSpawner.allNotes.Any(n => n < 11)
        )
        {
            splitLine.gameObject.SetActive(true);
            twoHands = true;
        }
        else
        {
            splitLine.gameObject.SetActive(false);
            twoHands = false;
        }

        //set constant UI stuff
        flashBackground.color =
            Color.Lerp(flashBackground.color, Color.clear, t);
        if (t < 1) t += Time.deltaTime / 100;

        mutliBar.fillAmount = mutliBarFillAmount;

        waveCountText.text = "waves" + "\n" + waveCount;
        if (!infiniteMode) waveCountText.text += "/" + wavesToComplete;

        correctNotesText.text = totalCorrectNotes.ToString();
        inorrectNotesText.text = totalIncorrectNotes.ToString();
        streakScoreText.text = streakScore.ToString();

        errorBar.fillAmount = errorAmount / 10;

        if (errorAmount >= 10)
        {
            StartCoroutine(ResetErrorBar(.5f));
            RemoveLife(null);
        }

        if (GameOver() || LevelComplete())
        {
            canPlay = false;
            StopCoroutine(SpawnWaves());
            Time.timeScale = 0;
            if (GameOver())
            {
                counterText.text = "Game Over! \nPress 'R' to try again";
                waveText.enabled = false;
            }
            else
            {
                counterText.text = "Level Complete! \nPress 'R' to continue";
                waveText.enabled = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void PlayNote(int note)
    {
        if (canPlay)
        {
            bool isCorrect = false;
            foreach (GameObject asteroid in spawnedAsteroids.ToList())
            {
                if (
                    !usingRhythmBarMode //remove this switch after GIT upload
                )
                {
                    if (
                        note ==
                        asteroid.GetComponent<Mover>().note + startMIDINumber
                    )
                    {
                        if (collision.isOnRhythm)
                        {
                            AddScore(10);
                            var bonusEffect = Instantiate(scoreGainEffect);
                            bonusEffect.transform.position = Vector3.zero;
                            bonusEffect.SetText("RHYTHM BONUS + 10!");
                            //set colour of text (edit script so it has a colour set function)
                        }

                        totalCorrectNotes++;
                        streakScore++;
                        isCorrect = true;
                        StartCoroutine(ShootAtBlob(asteroid.transform.position,
                        .2f,
                        asteroid,
                        50));
                        if (mutliBarFillAmount <= 1) mutliBarFillAmount += .1f;
                        break;
                    }
                    else
                    {
                        // mutliBarFillAmount += .33f; //HARD CODED TO THREE ERRORS BEFORE PENALTY
                        // int firstNote =
                        //     spawnedAsteroids[0].GetComponent<Mover>().note;
                        // keyboardStickers[firstNote].enabled = true;
                        // break;
                    }
                }
                else
                {
                    if (
                        note ==
                        asteroid.GetComponent<Mover>().note + startMIDINumber
                    )
                        if (
                            asteroid
                                .GetComponent<RhythmBarCollision>()
                                .isInTrigger
                        )
                        {
                            print("correct!");
                            spawnedAsteroids.Remove (asteroid);
                            Destroy (asteroid);
                            break;
                        }
                        else
                        {
                            print("right note but out of trigger!");
                            mutliBarFillAmount += .33f; //HARD CODED TO THREE ERRORS BEFORE PENALTY
                            break;
                        }
                    else
                    {
                        mutliBarFillAmount += .33f; //HARD CODED TO THREE ERRORS BEFORE PENALTY
                        break;
                    }
                }
            }

            //if NONE of the notes on screen were played, reset multiplier
            if (!isCorrect)
            {
                streakScore = 0;
                totalIncorrectNotes++;
                errorAmount += 1f;
                StartCoroutine(ResetNoteBar(.5f));
                aSource.PlayOneShot(errorSound, .5f);
            }
        }
    }

    public IEnumerator ResetNoteBar(float timeToMove)
    {
        flashBackground.color = flashColour;
        t = 0;

        var currentBarValue = mutliBarFillAmount;
        var time = 0f;
        while (time < 1)
        {
            time += Time.deltaTime / timeToMove;
            mutliBarFillAmount = Mathf.Lerp(currentBarValue, 0, time);
            yield return null;
        }
    }

    //for penalties (quite harsh method) currentlt not implemented
    IEnumerator DelayInput()
    {
        canPlay = false;
        yield return new WaitForSeconds(1);
        canPlay = true;
        StartCoroutine(ResetNoteBar(.5f));
    }

    public IEnumerator ResetErrorBar(float timeToMove)
    {
        //implement this without update
        // var bonusEffect = Instantiate(scoreGainEffect);
        // bonusEffect.transform.position = Vector3.zero;
        // bonusEffect.SetText("ERRORS RESET!");
        var currentBarValue = errorAmount;
        var time = 0f;
        while (time < 1)
        {
            time += Time.deltaTime / timeToMove;
            errorAmount = Mathf.Lerp(currentBarValue, 0, time);
            yield return null;
        }
    }

    public IEnumerator
    ShootAtBlob(
        Vector3 position,
        float timeToMove,
        GameObject currentBlob,
        float newScore
    )
    {
        AddScore (newScore);

        if (currentBlob != null)
        {
            //line player ship up with note
            var tempY = playerShip.transform.position;
            tempY.y = currentBlob.transform.position.y;
            playerShip.transform.position = tempY;

            //animate score effect
            var scoreEffect = Instantiate(scoreGainEffect);
            scoreEffect.transform.position = currentBlob.transform.position;
            scoreEffect.SetText("+" + newScore * scoreMultiplier + "!");
        }

        //shoot bolt at hazard
        GameObject bolt = Instantiate(noteBolt); //this should be done in the coroutine
        var currentPos = boltSpawn.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            bolt.transform.position = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }

        //animate explosion at note position after the bolt has reached it
        var explode =
            Instantiate(explosion,
            currentBlob.transform.position,
            currentBlob.transform.rotation);
        explode.startColor = currentBlob.GetComponent<Mover>().noteColour;
        spawnedAsteroids.Remove (currentBlob);
        Destroy (currentBlob);
        Destroy (bolt);

        //check here to see if note destroyed is last note in array
        if (spawnedAsteroids.Count < 1)
        {
            waveCount++;
            waveText.text = "WAVE:\n" + waveCount;

            //Increase speed each wave and total enemies each odd wave
            if (!usingRhythmBarMode && waveCount % 2 != 0)
            {
                asteroidCount++;
            }
            speedMultiplier += difficultyIncrement;

            StartCoroutine(SpawnWaves());
        }
    }

    IEnumerator SpawnWaves()
    {
        timer = startWait;
        yield return new WaitForSeconds(startWait);

        waveText.text = "";

        for (int i = 0; i < asteroidCount; i++)
        {
            Vector3 spawnPosition;

            var asteroidObj = Instantiate(enemyScript);
            noteSpawner.GetNewNote();
            int currentNote = noteSpawner.currentNote;

            if (twoHands)
            {
                //above or below the two hand line
                if (noteSpawner.allNotes[currentNote] > 11)
                {
                    spawnPosition =
                        new Vector3(spawnValues.x,
                            Random
                                .Range(spawnValues.y,
                                spawnValues.y + spawnValues.y),
                            spawnValues.z);
                }
                else
                {
                    spawnPosition =
                        new Vector3(spawnValues.x,
                            Random
                                .Range(-spawnValues.y,
                                -spawnValues.y - spawnValues.y),
                            spawnValues.z);
                }
            }
            else
            {
                spawnPosition =
                    new Vector3(spawnValues.x,
                        Random.Range(-spawnValues.y, spawnValues.y),
                        spawnValues.z);
            }

            asteroidObj.transform.position = spawnPosition;
            asteroidObj.controller = this;
            asteroidObj.frequency = Random.Range(.9f, 1.2f); //(should be in the actual enemy script)
            asteroidObj.note = noteSpawner.allNotes[currentNote];
            asteroidObj.noteName =
                noteSpawner
                    .noteNames[noteSpawner.allNotes[currentNote]]
                    .ToUpperInvariant();
            asteroidObj.notationImage =
                notationImages[noteSpawner.allNotes[currentNote]];
            asteroidObj.theConductor = theConductor;
            asteroidObj.distanceOfBars = distOfBars;
            if (useColour)
            {
                asteroidObj.noteColour =
                    noteSpawner
                        .noteColours[noteSpawner.allNotes[currentNote] % 12];
            }
            else
                asteroidObj.noteColour = Color.white;

            spawnedAsteroids.Add(asteroidObj.gameObject);

            noteSpawner.patternIndex++;
            float updatedSpawnWait = spawnWait / speedMultiplier;
            yield return new WaitForSeconds(Random
                        .Range(updatedSpawnWait / 2, updatedSpawnWait));
        }
    }

    public void RemoveLife(GameObject currentBlob)
    {
        flashBackground.color = flashColour;
        t = 0;

        var playerExplode =
            Instantiate(playerExplosion,
            playerShip.transform.position,
            playerShip.transform.rotation);
        if (currentBlob != null)
            playerExplode.startColor =
                currentBlob.GetComponent<Mover>().noteColour;

        spawnedAsteroids.Remove (currentBlob);
        Destroy (currentBlob);

        //if a note leaves the screen it is destroyed so check here to see if a note was destroyed and if it was last in array
        if (spawnedAsteroids.Count < 1)
        {
            waveCount++;
            waveText.text = "WAVE:\n" + waveCount;

            //Increase speed each wave and total enemies each odd wave
            if (!usingRhythmBarMode && waveCount % 2 != 0) asteroidCount++;
            speedMultiplier += difficultyIncrement;

            StartCoroutine(SpawnWaves());
        }
        foreach (Image heart in hearts)
        if (heart.enabled)
        {
            totalLives--;
            heart.enabled = false;
            break;
        }
    }

    public void AddScore(float scoreToAdd)
    {
        totalScore += scoreToAdd * scoreMultiplier;
    }

    protected bool GameOver() => totalLives <= 0;

    protected bool LevelComplete() =>
        waveCount >= wavesToComplete && !infiniteMode;

    //--Menu Controls--//
    public void UpdateBPM(InputField newBPM)
    {
        float value = float.Parse(newBPM.text);
        if (value > 0) theConductor.songBPM = value;
        theConductor.secPerBeat = 60 / theConductor.songBPM;
    }

    public void UpdateNotationSwitch()
    {
        if (usingNotation)
            usingNotation = false;
        else
            usingNotation = true;
    }

    public void UpdateMusicVolume()
    {
        if (theConductor.musicSource.volume > 0)
        {
            theConductor.musicSource.volume = 0;
        }
        else
            theConductor.musicSource.volume = .65f;
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

    public void UpdateLevelsToComplete(InputField levelsToComplete)
    {
        float value = float.Parse(levelsToComplete.text);
        if (value <= 0)
            infiniteMode = true;
        else
            infiniteMode = false;

        wavesToComplete = (int) value;
    }
}
