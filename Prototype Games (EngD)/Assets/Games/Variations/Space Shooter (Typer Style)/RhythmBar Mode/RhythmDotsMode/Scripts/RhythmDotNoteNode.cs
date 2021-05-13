using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RhythmDotNoteNode : MonoBehaviour
{
    public float frequency;

    public float magnitude;

    public int note;

    public string noteName;

    public Color32 noteColour;

    public Sprite notationImage;

    public RhythmDotsController controller;

    void Start()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = noteName;
        transform.GetChild(0).GetComponent<TextMeshPro>().color = noteColour;
        transform.GetChild(2).GetComponent<SpriteRenderer>().color = noteColour;
        transform.GetChild(1).GetComponent<SpriteRenderer>().sprite =
            notationImage;
    }

    void Update()
    {
        if (controller.usingNotation)
        {
            transform.GetChild(0).GetComponent<TextMeshPro>().enabled = false;
            transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.GetChild(0).GetComponent<TextMeshPro>().enabled = true;
            transform.GetChild(1).GetComponent<SpriteRenderer>().enabled =
                false;
        }
    }
}
