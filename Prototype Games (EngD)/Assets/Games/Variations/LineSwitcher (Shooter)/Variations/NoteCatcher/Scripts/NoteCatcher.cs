using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCatcher : MonoBehaviour
{
    public NoteCatcherController controller;

    public ScoreGainEffect bonusEffect;

    AudioSource audioSource;

    SpriteRenderer spriteRenderer;

    float t = 1;

    Vector3 defaultPosition;

    float score = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultPosition = transform.localScale;
    }

    private void Update()
    {
        transform.localScale =
            Vector3.Lerp(transform.localScale, defaultPosition, t);
        if (t < 1) t += Time.deltaTime / 10;
    }

    private void OnTriggerEnter2D(Collider2D other) //needs condensing (lots of repeat)
    {
        controller.canHit = true;

        if (other.tag == "Health")
        {
            controller.AddLife();
            var scoreEffect = Instantiate(bonusEffect);
            bonusEffect.transform.position = transform.position;
            scoreEffect.SetText("+1HP!");
            other.GetComponent<SpriteRenderer>().enabled = false;
            other.GetComponent<CircleCollider2D>().enabled = false;
        }
        else if (other.tag == "Pickup")
        {
            score = 10;
            var scoreEffect = Instantiate(bonusEffect);
            bonusEffect.transform.position = transform.position;
            scoreEffect.SetText("+" + score);
            controller.AddScore (score);
            controller
                .keyConductor
                .PlayNote(other.GetComponent<EnemyNoteCatcherMovement>().note);
            other.GetComponent<SpriteRenderer>().enabled = false;
            other.GetComponent<CircleCollider2D>().enabled = false;
        }
        else if (!controller.usingTapMode)
        {
            score = 50;
            controller.AddScore (score);
            controller
                .keyConductor
                .PlayNote(other.GetComponent<EnemyNoteCatcherMovement>().note);
            transform.localScale += Vector3.one / 3.5f;
            t = 0;
            spriteRenderer.color = other.GetComponent<SpriteRenderer>().color;
            other.GetComponent<SpriteRenderer>().enabled = false;
            other.GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (controller.usingTapMode)
        {
            if (controller.canDestroy)
            {
                score = 10;
                controller.AddScore (score);
                controller
                    .keyConductor
                    .PlayNote(other
                        .GetComponent<EnemyNoteCatcherMovement>()
                        .note);
                transform.localScale += Vector3.one * 3.5f;
                t = 0;
                spriteRenderer.color =
                    other.GetComponent<SpriteRenderer>().color;
                other.GetComponent<SpriteRenderer>().enabled = false;
                other.GetComponent<CircleCollider2D>().enabled = false;
                controller.canDestroy = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        controller.canHit = false;
    }
}
