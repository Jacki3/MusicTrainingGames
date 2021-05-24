using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SonarNote : MonoBehaviour
{
    public int note;

    public bool isPlayable = false;

    public bool tickLeft = false;

    public string noteString;

    public Color32 noteColour;

    public Sprite notationSprite;

    public SonarNotesController controller;

    TextMeshProUGUI noteText;

    Image noteImage;

    Image notation;

    private void Start()
    {
        noteText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        noteImage = transform.GetChild(1).GetComponent<Image>();
        notation = transform.GetChild(4).GetComponent<Image>();
    }

    private void Update()
    {
        noteText.text = noteString;
        noteImage.color = noteColour;
        noteText.color = noteColour;
        notation.sprite = notationSprite;

        if (controller.usingNotation)
        {
            notation.enabled = true;
            noteText.enabled = false;
        }
        else
        {
            notation.enabled = false;
            noteText.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        isPlayable = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isPlayable = false;
        tickLeft = true;
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
