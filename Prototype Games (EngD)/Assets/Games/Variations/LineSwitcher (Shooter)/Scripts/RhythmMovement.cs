using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmMovement : MonoBehaviour
{
    public Conductor theConductor;

    public float distanceToNoteObj;

    public float originalPos;

    public float difference;

    public bool Ymode = false;

    Vector3 target;

    float speed;

    void Update()
    {
        target = transform.parent.position;

        if (Ymode)
        {
            if (transform.position.y == target.y)
            {
                originalPos = transform.parent.position.y + difference;
                var tempY = transform.position;
                tempY.y = originalPos;
                transform.position = tempY;
            }
        }
        else
        {
            if (transform.position.x == 0)
            {
                var tempX = transform.position;
                tempX.x = originalPos;
                transform.position = tempX;
            }
        }

        speed = distanceToNoteObj / (theConductor.secPerBeat);

        transform.position =
            Vector3
                .MoveTowards(transform.position,
                target,
                speed * Time.deltaTime);
    }
}
