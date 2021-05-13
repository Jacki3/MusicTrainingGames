using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public CrossyStaffController controller;

    public ScoreGainEffect bonusEffect;

    public float scoreToAdd = 50;

    static int levelCount = 0;

    static bool roundComplete = true;

    public int index = 0;

    static int previousIndex = 0;

    private void Start()
    {
        levelCount = 0;
    }

    private void Update()
    {
        controller.roundText.text = levelCount.ToString();

        // if (levelCount >= controller.roundsToMultiplier)
        // {
        //     controller.roundsToMultiplier += controller.roundsToMultiplier;
        //     if (controller.scoreMultiplier < controller.maxMultiplier)
        //         controller.scoreMultiplier += 1;
        // }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && index != previousIndex)
        {
            levelCount++;
            controller.AddScore (scoreToAdd);
            controller.scoreMultiplier++;
            var scoreEffect = Instantiate(bonusEffect);
            bonusEffect.transform.position = Vector3.zero;
            scoreEffect.SetText(levelCount + "!");
            if (controller.usingRhythm)
            {
                if (levelCount >= controller.goalsToMultiplier)
                {
                    controller.goalsToMultiplier +=
                        controller.goalsToMultiplier;
                    controller.speedMultiplier *= 2;
                }
            }
            else
                controller.speedMultiplier += controller.speedIncrease;

            roundComplete = true;
            previousIndex = index;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            roundComplete = false;
        }
    }
}
