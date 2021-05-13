using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteDodger : MonoBehaviour
{
    public NoteDodgerController controller;

    public ScoreGainEffect bonusEffect;

    float score = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Bars")
        {
            if (other.tag == "Health")
            {
                controller.AddLife();
                var scoreEffect = Instantiate(bonusEffect);
                bonusEffect.transform.position = transform.position;
                scoreEffect.SetText("+1HP!");
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
                    .PlayNote(other.GetComponent<EnemyNoteDodger>().note);
            }
            else
            {
                controller.RemoveLife();
                controller
                    .keyConductor
                    .PlayNote(other.GetComponent<EnemyNoteDodger>().note);
            }

            other.GetComponent<SpriteRenderer>().enabled = false;
            other.GetComponent<CircleCollider2D>().enabled = false;
        }
    }
}
