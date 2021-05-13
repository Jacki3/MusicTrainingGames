using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmBarCollision : MonoBehaviour
{
    public bool isInTrigger;

    public RhythmDotsController dotsController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        isInTrigger = true;
        if (dotsController)
        {
            dotsController
                .keyStickers[GetComponent<RhythmDotNoteNode>().note]
                .enabled = true;
            dotsController.currentNote =
                gameObject.GetComponent<RhythmDotNoteNode>().note;
            dotsController.currentNoteObj = this;

            dotsController.canDestroy = true;
        }

        transform.GetChild(3).gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isInTrigger = false;
        if (dotsController && dotsController.canDestroy)
        {
            dotsController
                .keyStickers[GetComponent<RhythmDotNoteNode>().note]
                .enabled = false;
            Destroy (gameObject);
            dotsController.currentNote = -1;
            dotsController.currentNoteObj = null;

            Camera.main.backgroundColor = Color.red;
            dotsController.t = 0;
            dotsController.RemoveHeart();
            StartCoroutine(dotsController.ResetNoteBar(.5f));
        }
        transform.GetChild(3).gameObject.SetActive(false);
    }
}
