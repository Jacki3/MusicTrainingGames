using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarMovement : MonoBehaviour
{
    public Conductor theConductor;

    public RhythmDotsController controller;

    public GameObject rhythmNode;

    public int noteLengths;

    public bool dotsMode = false;

    float timeToTake;

    bool dirRight = true;

    float distanceOfBars = 30;

    bool canSpawnRight = true;

    bool canSpawnLeft = false;

    private void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            noteLengths = controller.totalNotesToShow;

            //calculate start line so notes appear central
            float percent = (theConductor.secPerBeat / noteLengths);
            float difference = percent * transform.position.x;
            var tempX = transform.position;
            tempX.x -= difference;
            transform.position = tempX;

            theConductor.StartSong();
            InvokeRepeating("SpawnRhythmNode",
            0,
            theConductor.secPerBeat / (noteLengths / 4));
            timeToTake = distanceOfBars / (theConductor.secPerBeat * 4);
            // transform.x = Mathf.Sin(Time.time * 5) * 3;
        }
        if (dirRight)
            transform
                .Translate(Vector2.down *
                timeToTake *
                Time.deltaTime *
                controller.speedMultiplier);
        else
            transform
                .Translate(-Vector2.down *
                timeToTake *
                Time.deltaTime *
                controller.speedMultiplier);

        if (transform.position.x > 15) dirRight = false;
        if (transform.position.x < -15) dirRight = true;

        if (dotsMode)
        {
            if (theConductor.oneCycleComplete)
            {
                if (transform.position.x > 0 && canSpawnRight)
                {
                    canSpawnRight = false;
                    canSpawnLeft = true;
                    if (dotsMode) controller.SpawnNote(false);
                }
                else if (transform.position.x < 0 && canSpawnLeft)
                {
                    canSpawnLeft = false;
                    canSpawnRight = true;
                    if (dotsMode) controller.SpawnNote(true);
                }
            }
        }
    }

    public void SpawnRhythmNode()
    {
        var node =
            Instantiate(rhythmNode, transform.position, Quaternion.identity);

        if (dotsMode)
        {
            if (node.transform.position.x > 0)
                controller.noteSpawnPositionsRight.Add(node.transform);
            else
                controller.noteSpawnPositionsLeft.Add(node.transform);
        }

        if (--noteLengths == 0) CancelInvoke("SpawnRhythmNode");
    }
}
