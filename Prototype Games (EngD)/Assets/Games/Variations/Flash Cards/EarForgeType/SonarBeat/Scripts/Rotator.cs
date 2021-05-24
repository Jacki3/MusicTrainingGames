using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float degrees = 90;

    Vector3 to;

    // Start is called before the first frame update
    void Start()
    {
        to = new Vector3(0, 0, degrees);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, degrees * Time.deltaTime);
    }
}
