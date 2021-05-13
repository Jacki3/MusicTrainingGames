using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCar : MonoBehaviour
{
    public float speed = 2;

    public Color32 noteColor;

    public CrossyStaffController controller;

    public ScoreGainEffect bonusEffect;

    BoxCollider2D boxCollider2D;

    SpriteRenderer spriteRenderer;

    Color32 defaultColour;

    Color32 flashColour = Color.white;

    float distanceToNoteObj;

    float noteLength = 4;

    void Start()
    {
        distanceToNoteObj = controller.posX;

        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!controller.quickFireMode)
        {
            speed =
                distanceToNoteObj /
                (
                controller.theConductor.secPerBeat *
                noteLength /
                controller.speedMultiplier
                );
        }
        else
        {
            speed =
                distanceToNoteObj /
                (
                controller.theConductor.secPerBeat / controller.speedMultiplier
                );
        }

        if (transform.position.x > 0)
        {
            speed = -speed;
        }
        spriteRenderer.color = noteColor;
        defaultColour = spriteRenderer.color;
    }

    void Update()
    {
        transform.position += Vector3.right * Time.deltaTime * speed;

        spriteRenderer.color =
            Color.Lerp(spriteRenderer.color, defaultColour, controller.carT);

        if (controller.canFlash)
        {
            spriteRenderer.color = flashColour;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (this.tag != "Pickup")
            {
                controller.scoreMultiplier = 1;

                controller.LoseLife();
                boxCollider2D.enabled = false;
                spriteRenderer.enabled = false;
            }
            else
            {
                controller.AddScore(100);
                var scoreEffect = Instantiate(bonusEffect);
                bonusEffect.transform.position = transform.position;
                scoreEffect.SetText("BONUS!");
                boxCollider2D.enabled = false;
                spriteRenderer.enabled = false;
            }
        }
    }
}
