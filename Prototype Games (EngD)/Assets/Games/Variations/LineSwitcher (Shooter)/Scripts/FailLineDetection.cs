using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailLineDetection : MonoBehaviour
{
    public LineSwitcherController controller;

    public NoteCatcherController catcherController;

    public Color32 flashColour;

    public bool catcherShouldDestroy;

    SpriteRenderer spriteRenderer;

    Color32 defaultColour;

    float t = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColour = spriteRenderer.color;
    }

    private void Update()
    {
        spriteRenderer.color =
            Color.Lerp(spriteRenderer.color, defaultColour, t);
        if (catcherController)
        {
            if (t <= catcherController.theConductor.secPerBeat)
                t += Time.deltaTime / catcherController.theConductor.secPerBeat;

            if (t >= catcherController.theConductor.secPerBeat)
            {
                spriteRenderer.color = flashColour;
                t = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller && controller.gameStarted)
            if (
                transform.position.x > 0 && other.tag == "Projectile" ||
                transform.position.x < 0 && other.tag == "Enemy"
            )
            {
                if (controller) controller.RemoveLife();
            }
        if (catcherController && transform.position.x > 0)
        {
            if (other.tag == "Pickup")
            {
                if (
                    other
                        .GetComponent<EnemyNoteCatcherMovement>()
                        .pickupBouncedOnce
                )
                {
                    Destroy(other.gameObject);
                }
            }
        }
        if (catcherController && transform.position.x < 0)
        {
            if (other.tag == "Enemy")
            {
                catcherController.RemoveLife();
                if (catcherShouldDestroy) Destroy(other.gameObject);
            }
            if (other.tag == "Pickup")
            {
                EnemyNoteCatcherMovement tempMovement =
                    other.GetComponent<EnemyNoteCatcherMovement>();

                if (!tempMovement.pickupBouncedOnce)
                {
                    tempMovement.pickupBouncedOnce = true;
                }
            }
        }
    }
}
