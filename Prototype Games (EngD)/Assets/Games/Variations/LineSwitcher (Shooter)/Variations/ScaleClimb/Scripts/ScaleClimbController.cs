using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScaleClimbController : MonoBehaviour
{
    public NoteFlashView notesController;

    public Conductor theConductor;

    public BlockerMovement blocker;

    public NoteHero noteHero;

    public int startMIDINumber;

    public Transform[] staffPositionsSharp;

    public Transform[] staffPositionsFlat;

    public float spawnX;

    public bool restrictToScale;

    public int currentNote;

    public int timeToMove = 1;

    public float speedMultiplier = 1;

    public float doubleSpeedTime = 3000;

    public bool tapMode = false;

    public int notesToDodge;

    public bool chordMode = false;

    public NoteHero[] noteHeroes;

    public float score;

    public int totalLives = 5;

    public List<Image> hearts = new List<Image>();

    public TextMeshProUGUI scoreText;

    private Transform[] spawnPoints;

    private RhythmDetector rhythmDetector;

    private int chordIndex = 0;

    private int noteIndex = -1;

    private float t = 1;

    Color32 defaultCamColor;

    // Start is called before the first frame update
    void Start()
    {
        defaultCamColor = Camera.main.backgroundColor;

        rhythmDetector = noteHero.GetComponentInChildren<RhythmDetector>();

        if (!tapMode) timeToMove = 4;

        if (chordMode)
            foreach (NoteHero hero in noteHeroes)
            {
                var tempY = hero.transform.position;
                tempY.y = -10;
                hero.transform.position = tempY;
            }
    }

    void Update()
    {
        float elapsedGameTime = Time.time + Time.deltaTime;
        if (!tapMode) speedMultiplier = 1 + elapsedGameTime / doubleSpeedTime;

        if (notesController.usingSharpScale)
            spawnPoints = staffPositionsSharp;
        else
            spawnPoints = staffPositionsFlat;

        scoreText.text = Mathf.FloorToInt(score).ToString();

        Camera.main.backgroundColor =
            Color.Lerp(Camera.main.backgroundColor, defaultCamColor, t);
        if (t < 1) t += Time.deltaTime / 10;

        if (Input.GetKeyUp(KeyCode.Space))
        {
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
                // for (int i = 0; i < 4; i++)
                // {
                //     notesController
                //         .allNotes
                //         .Add(notesController.allNotes[i] + 12);
                // }
            }
            theConductor.StartSong();
            StartCoroutine(SpawnBlocker());

            var tempY = noteHero.transform.position;
            currentNote = notesController.allNotes[0] + startMIDINumber;
            tempY.y = spawnPoints[currentNote % 12].position.y;
            noteHero.transform.position = tempY;
        }

        if (GameOver())
        {
            Time.timeScale = 0;
            if (Input.GetKey(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (notesToDodge >= 2)
        {
            restrictToScale = false;
            chordMode = true;
        }
    }

    public void PlayNote(int note)
    {
        if (!chordMode)
            PlaySingleNote(note);
        else
            PlayChordNote(note);
    }

    private void PlaySingleNote(int note)
    {
        //needed for ryhthm tap mode
        if (!tapMode || rhythmDetector.canHit)
        {
            if (
                !restrictToScale &&
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

    private void PlayChordNote(int note)
    {
        if (!tapMode || rhythmDetector.canHit)
        {
            {
                if (noteIndex < noteHeroes.Length - 1) noteIndex++;
                var tempY = noteHeroes[noteIndex].transform.position;
                tempY.y = spawnPoints[note - startMIDINumber].position.y;
                noteHeroes[noteIndex].transform.position = tempY;
            }
        }
    }

    public void NoteOff(int note)
    {
        if (chordMode)
        {
            noteIndex--;
            if (noteIndex < -1) noteIndex = -1;
            for (int i = 0; i < noteHeroes.Length; i++)
            {
                if (
                    noteHeroes[i].transform.position.y ==
                    spawnPoints[note - startMIDINumber].position.y
                )
                {
                    var tempY = noteHeroes[i].transform.position;
                    tempY.y = -10;
                    noteHeroes[i].transform.position = tempY;
                }
            }
        }
    }

    public IEnumerator SpawnBlocker()
    {
        var blockerObj = Instantiate(blocker);

        blockerObj.theConductor = theConductor;
        blockerObj.controller = this;

        var tempBlocker = blockerObj.transform.position;
        tempBlocker.y = spawnPoints[notesController.allNotes[0]].position.y;

        blockerObj.transform.position = new Vector3(spawnX, tempBlocker.y, 1);

        for (
            int i = 0;
            i < blocker.transform.childCount - notesController.allNotes.Count;
            i++
        )
        {
            blockerObj
                .transform
                .GetChild((blockerObj.transform.childCount - 1) - i)
                .gameObject
                .SetActive(false);
        }

        notesController.GetNewNote();
        chordIndex = 0;
        bool wasMinus = false;
        for (int i = 0; i < notesToDodge; i++)
        {
            var child =
                blockerObj
                    .transform
                    .GetChild(notesController.currentNote + chordIndex);

            if (child.GetSiblingIndex() >= notesController.allNotes.Count - 2)
            {
                chordIndex -= 2;
                wasMinus = true;
            }
            else
                chordIndex += 2;

            if (wasMinus)
            {
                chordIndex -= 2;
                wasMinus = false;
            }

            child.GetComponent<Blocker>().isSafe = true;
            child.GetComponent<SpriteRenderer>().color = Color.green;
        }

        yield return new WaitForSeconds((theConductor.secPerBeat * timeToMove) /
                speedMultiplier);
        StartCoroutine(SpawnBlocker());
    }

    public void AddScore(float newScore)
    {
        Camera.main.backgroundColor = Color.green;
        t = 0;
        score += newScore * speedMultiplier;
    }

    public void RemoveLife()
    {
        Camera.main.backgroundColor = Color.red;
        t = 0;
        totalLives--;
        foreach (Image heart in hearts)
        if (heart.enabled)
        {
            heart.enabled = false;
            break;
        }
    }

    public void UpdateBPM(TMP_InputField value)
    {
        float newBPM = float.Parse(value.text);
        if (newBPM > 0) theConductor.songBPM = newBPM;
        theConductor.secPerBeat = 60 / theConductor.songBPM;
    }

    public void UpdatenNotesToDodge(Dropdown value)
    {
        int dodgeCount = value.value;
        notesToDodge = dodgeCount;
    }

    public void UpdateRestrictToScale()
    {
        if (restrictToScale)
        {
            restrictToScale = false;
            chordMode = true;
        }
        else
        {
            restrictToScale = true;
            chordMode = false;
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
    public void UpdateMetroSwitch()
    {
        if (theConductor.metronome.volume <= 0)
            theConductor.metronome.volume = .5f;
        else
            theConductor.metronome.volume = 0;
    }

    public void UpdateMusicSwitch()
    {
        if (theConductor.musicSource.volume <= 0)
            theConductor.musicSource.volume = .4f;
        else
            theConductor.musicSource.volume = 0;
    }
}
