using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public bool isOnRhythm = false;

    private void OnTriggerEnter(Collider other)
    {
        isOnRhythm = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isOnRhythm = false;
    }
}
