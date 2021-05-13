using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNoteDodger : MonoBehaviour
{
    private float speed;

    public float minusOrFoward;

    public float frequency;

    public float magnitude;

    public NoteDodgerController controller;

    public int note;

    Vector3 pos;

    void Start()
    {
        pos = transform.position;

        note = controller.notePositionsY.IndexOf(transform.position.y);
        note += controller.startMIDINumber;

        float dis = 0;
        if (controller.usingRhythm)
        {
            dis =
                controller.enemySpawnX -
                controller.rhythmLine.transform.position.x;
        }
        else
            dis = controller.enemySpawnX;

        speed =
            dis /
            (
            controller.theConductor.secPerBeat * 4 / controller.speedMultiplier
            );
    }

    void Update()
    {
        pos += Vector3.right * Time.deltaTime * -speed * minusOrFoward;

        transform.position =
            pos + transform.up * Mathf.Sin(Time.time * frequency) * magnitude;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bars" && controller.usingRhythm)
        {
            float dis = controller.rhythmLine.transform.position.x; //no need to calculate as spawn pos is 0
            speed = dis / (controller.theConductor.secPerBeat);
        }
    }
}
