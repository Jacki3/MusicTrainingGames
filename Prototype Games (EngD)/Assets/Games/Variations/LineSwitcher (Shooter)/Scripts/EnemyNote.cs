using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNote : MonoBehaviour
{
    //TO DO: ensure all components are got at start
    public float score = 10;

    public GameObject explodeAnim;

    int note;

    AudioSource audioSource;

    [HideInInspector]
    public LineSwitcherController controller;

    [HideInInspector]
    public ScoreGainEffect gainEffect;

    public static LineSwitcherController.IntEvent
        noteOn = new LineSwitcherController.IntEvent();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<NoteProjectile>().controller;
        gainEffect = GetComponent<NoteProjectile>().gainEffect;
        note = GetComponent<NoteProjectile>().note;
    }

    private void Update()
    {
        transform
            .Rotate(0, 0, controller.speedMultiplier * 50 * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Projectile")
        {
            //ONLY allow sucess IF the projectile has the same note as this one; removing any problems faced with sharps/flats
            if (
                other.GetComponent<NoteProjectile>().note -
                controller.startMIDINumber ==
                note
            )
            {
                if (controller.rhythmShootMode)
                    noteOn.Invoke(note + controller.startMIDINumber, 2.0f);
                float newScore = score * controller.speedMultiplier;
                float basicScore = Mathf.FloorToInt(newScore);

                var effect = Instantiate(gainEffect);
                effect.transform.position = transform.position;
                effect.SetText("+" + basicScore + "!");

                controller.score += newScore;
                controller.totalCorrect++;
                controller.streakScore++;

                //decide if users would prefer to change the colour of notes and let them go OR blow them up (currently, notes come out COLOURED and change to WHITE)
                var color = Color.white; // change this to colour of the projectile if you would prefer to have notes come out white and be coloured in
                if (controller.useColour)
                {
                    GetComponent<SpriteRenderer>().color = color;
                    GetComponent<SpriteRenderer>()
                        .material
                        .SetColor("_EmissionColor", color * 1.2f);
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                    GetComponent<SpriteRenderer>()
                        .material
                        .SetColor("_EmissionColor", Color.white * 1.2f);
                }
                GetComponent<CircleCollider2D>().enabled = false;

                //FOR EXPLODING NOTES
                // var anim =
                //     Instantiate(explodeAnim,
                //     transform.position,
                //     transform.rotation);
                // Destroy (gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}
