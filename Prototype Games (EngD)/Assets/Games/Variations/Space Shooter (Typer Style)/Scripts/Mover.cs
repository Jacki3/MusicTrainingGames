using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//TO DO
//move towards player not in single direction
public class Mover : MonoBehaviour
{
    public float speed;

    public float frequency;

    public float magnitude;

    public int note;

    public string noteName;

    public Color32 noteColour;

    public Sprite notationImage;

    public AsteroidSpawner controller;

    public Conductor theConductor;

    private Vector3 pos;

    public float distanceOfBars = 40;

    private AudioSource audioSource;

    private TextMeshPro noteText;

    private SpriteRenderer notationRenderer;

    private SpriteRenderer ringRenderer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        noteText = transform.GetChild(0).GetComponent<TextMeshPro>();
        notationRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
        ringRenderer = transform.GetChild(2).GetComponent<SpriteRenderer>();

        pos = transform.position;

        noteText.text = noteName;
        noteText.color = noteColour;

        notationRenderer.sprite = notationImage;

        ringRenderer.color = noteColour;

        if (controller.useColour)
            ringRenderer
                .material
                .SetColor("_EmissionColor",
                controller
                    .noteSpawner
                    .noteColours[controller
                        .noteSpawner
                        .allNotes[controller.noteSpawner.currentNote] %
                    12] *
                4.5f);
    }

    private void Update()
    {
        if (controller.usingNotation)
        {
            noteText.enabled = false;
            notationRenderer.enabled = true;
        }
        else
        {
            noteText.enabled = true;
            notationRenderer.enabled = false;
        }
        if (!controller.usingRhythmBarMode)
        {
            speed =
                distanceOfBars /
                (theConductor.secPerBeat * 12) *
                controller.speedMultiplier;
        }
        else
        {
            speed = distanceOfBars / (theConductor.secPerBeat * 4);
        }

        transform.position =
            Vector3
                .MoveTowards(transform.position,
                controller.playerShip.transform.position,
                speed * Time.deltaTime);

        // pos += transform.right * Time.deltaTime * -speed;
        // transform.position =
        //     pos + transform.up * Mathf.Sin(Time.time * frequency) * magnitude;
        if (transform.position == controller.playerShip.transform.position)
        {
            controller.RemoveLife(this.gameObject);
            controller.spawnedAsteroids.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
