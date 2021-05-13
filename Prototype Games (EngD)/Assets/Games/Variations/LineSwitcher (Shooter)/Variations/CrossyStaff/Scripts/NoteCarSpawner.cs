using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCarSpawner : MonoBehaviour
{
    public CrossyStaffController controller;

    public NoteCar noteCar;

    public NoteCar bonusNote;

    public ScoreGainEffect scoreGainEffect;

    bool gameStarted = false;

    public float minSpawnTime;

    public float maxSpawnTime;

    float spawnerPosX;

    void Start()
    {
        spawnerPosX = controller.posX;

        if (Random.value > .5f) spawnerPosX -= spawnerPosX * 2;
    }

    void Update()
    {
        if (controller.gameStarted && !gameStarted)
        {
            gameStarted = true;
            minSpawnTime = controller.theConductor.secPerBeat;
            maxSpawnTime = controller.theConductor.secPerBeat * 2; // small values ensures car pop up immediately
            StartCoroutine(SpawnNoteCar());
        }
        else if (!controller.quickFireMode)
        {
            minSpawnTime =
                controller.theConductor.secPerBeat *
                4 /
                controller.speedMultiplier;
            maxSpawnTime =
                controller.theConductor.secPerBeat *
                8 /
                controller.speedMultiplier;
        }
    }

    public IEnumerator SpawnNoteCar()
    {
        if (!controller.quickFireMode)
            yield return new WaitForSeconds(Random
                        .Range(minSpawnTime, maxSpawnTime));
        else
            yield return new WaitForSeconds(controller.theConductor.secPerBeat *
                    8 /
                    controller.speedMultiplier);

        var car = Instantiate(noteCar);
        car.transform.position =
            new Vector3(spawnerPosX, transform.position.y, 1);
        car.controller = controller;

        //get the index of all spawnpoints based on this one (should be converted to list?)
        int index =
            System
                .Array
                .IndexOf(controller.sharpStaffPositions, this.transform);
        if (controller.useColour)
        {
            Color32 noteCarColour =
                controller.notesController.noteColours[index % 12];
            car.noteColor = noteCarColour;
        }

        StartCoroutine(SpawnNoteCar());
        if (Random.value > .95) StartCoroutine(SpawnBonusNote(bonusNote, 3));
    }

    public IEnumerator SpawnBonusNote(NoteCar bonusType, int spawnAmount)
    {
        int randNote =
            Random.Range(0, controller.notesController.allNotes.Count);

        yield return new WaitForSeconds(controller.theConductor.secPerBeat);

        for (int i = 0; i < spawnAmount; i++)
        {
            var bonusNote = Instantiate(bonusType);
            bonusNote.transform.position =
                new Vector3(spawnerPosX, transform.position.y, 1);
            bonusNote.controller = controller;
            bonusNote.noteColor = Color.white;
            bonusNote.bonusEffect = scoreGainEffect;

            yield return new WaitForSeconds(Random
                        .Range(minSpawnTime, maxSpawnTime));
        }
    }
}
