using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteProjectile : MonoBehaviour
{
    private float speed;

    public float minusOrFoward;

    public int note;

    public LineSwitcherController controller;

    public ScoreGainEffect gainEffect;

    void Start()
    {
        //projectile being fired
        if (this.tag != "Enemy")
        {
            float dis =
                controller.speedLine.transform.position.x +
                controller.noteObj.transform.position.x * -1; //distance from player to enemy spawn pos
            speed = dis / (controller.theConductor.secPerBeat);
        }
        else
        //enemy note being fired
        {
            float dis = 0;
            if (controller.rhythmShootMode)
            {
                dis =
                    controller.posX - controller.speedLine.transform.position.x; //distance between spawn pos and rhythm line
            }
            else
                dis =
                    controller.speedLine.transform.position.x +
                    controller.noteObj.transform.position.x * -1;

            speed =
                dis /
                (
                controller.theConductor.secPerBeat *
                4 /
                controller.speedMultiplier
                );
        }
    }

    void Update()
    {
        transform.position +=
            Vector3.right * Time.deltaTime * -speed * minusOrFoward;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bars" && controller.rhythmShootMode)
        {
            float dis =
                dis =
                    controller.speedLine.transform.position.x +
                    controller.noteObj.transform.position.x * -1;
            speed = dis / (controller.theConductor.secPerBeat);
        }
    }
}
