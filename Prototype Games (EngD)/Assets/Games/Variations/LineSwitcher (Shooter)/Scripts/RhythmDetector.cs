using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmDetector : MonoBehaviour
{
    public bool canHit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        canHit = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        canHit = false;
    }
}
