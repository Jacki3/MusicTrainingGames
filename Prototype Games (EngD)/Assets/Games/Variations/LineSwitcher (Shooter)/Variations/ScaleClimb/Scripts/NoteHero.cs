using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHero : MonoBehaviour
{
    public ScaleClimbController controller;

    public float scoreToAdd = 30;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Blocker")
        {
            if (other.GetComponent<Blocker>().isSafe)
            {
                other.GetComponent<Blocker>().isSafe = false;
                other.GetComponent<SpriteRenderer>().color = Color.white;
                other
                    .transform
                    .parent
                    .GetComponent<BlockerMovement>()
                    .CheckToDestroy();

                controller.AddScore (scoreToAdd);
            }
            else
            {
                controller.RemoveLife();
            }
        }
    }
}
