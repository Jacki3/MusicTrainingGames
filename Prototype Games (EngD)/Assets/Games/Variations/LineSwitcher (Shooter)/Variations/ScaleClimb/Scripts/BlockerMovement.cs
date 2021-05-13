using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockerMovement : MonoBehaviour
{
    public float speed;

    public Conductor theConductor;

    public ScaleClimbController controller;

    float distance;

    void Start()
    {
        distance = transform.position.x;
    }

    void Update()
    {
        speed = distance / (theConductor.secPerBeat * controller.timeToMove);
        transform.position +=
            Vector3.left * Time.deltaTime * speed * controller.speedMultiplier;

        if (CheckToDestroy()) Destroy(this.gameObject);

        if (transform.position.x < -15.5f)
        {
            Destroy(this.gameObject);
            controller.RemoveLife();
        }
    }

    public bool CheckToDestroy()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Blocker>().isSafe) return false;
        }
        return true;
    }
}
