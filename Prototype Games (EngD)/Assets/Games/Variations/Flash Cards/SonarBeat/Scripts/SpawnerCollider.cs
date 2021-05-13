using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCollider : MonoBehaviour
{
    public SonarNotesController controller;

    public Conductor conductor;

    public bool canSpawn = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (conductor.oneCycleComplete)
        {
            // if(Random.value > .5f)
            controller.SpawnNote(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }
}
