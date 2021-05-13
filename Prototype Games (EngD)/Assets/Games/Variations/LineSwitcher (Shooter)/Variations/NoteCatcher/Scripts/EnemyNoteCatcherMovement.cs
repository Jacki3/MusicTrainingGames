using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNoteCatcherMovement : MonoBehaviour
{
    private float speed;

    public float minusOrFoward;

    public float frequency;

    public float magnitude;

    public NoteCatcherController controller;

    public int note;

    public bool pickupBouncedOnce = false;

    int noteLengths = 4;

    Vector3 pos;

    void Start()
    {
        pos = transform.position;

        note = controller.notePositionsY.IndexOf(transform.position.y);
        note += controller.startMIDINumber;

        float doubleNoteLength = noteLengths * 2;
        float beatLinePos = controller.beatLine.position.x;
        speed =
            (controller.posX - beatLinePos) /
            (
            controller.theConductor.secPerBeat *
            doubleNoteLength /
            controller.speedMultiplier
            );
    }

    void Update()
    {
        pos += Vector3.right * Time.deltaTime * -speed * minusOrFoward;

        transform.position =
            pos + transform.up * Mathf.Sin(Time.time * frequency) * magnitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bars")
        {
            speed =
                other.transform.position.x /
                (
                controller.theConductor.secPerBeat *
                noteLengths /
                controller.speedMultiplier
                );
        }
    }
}
