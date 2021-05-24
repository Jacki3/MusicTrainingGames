using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteShooter : MonoBehaviour
{
    public NoteProjectile noteProjectile;

    public LineSwitcherController controller;

    public Material glowMat;

    void Start()
    {
        // InvokeRepeating("FireNoteProjectile", 0, .666f);
        // glowMat = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
    }

    public void FireNoteProjectile(int note)
    {
        var projectile =
            Instantiate(noteProjectile,
            transform.position,
            Quaternion.identity);
        projectile.controller = controller;
        projectile.note = note;
        if (controller.useColour)
        {
            //sort this out
            GetComponent<SpriteRenderer>().color =
                controller.notesController.noteColours[note % 12];
            GetComponent<SpriteRenderer>()
                .material
                .SetColor("_EmissionColor",
                controller.notesController.noteColours[note % 12]);
            projectile.GetComponent<SpriteRenderer>().color =
                controller.notesController.noteColours[note % 12];
            projectile
                .GetComponent<SpriteRenderer>()
                .material
                .SetColor("_EmissionColor",
                controller.notesController.noteColours[note % 12] * 1.2f);

            // glowMat.color = controller.notesController.noteColours[note % 12];
            // glowMat
            //     .SetColor("_EmissionColor",
            //     controller.notesController.noteColours[note % 12]); //possible colour option for later/points?
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            GetComponent<SpriteRenderer>()
                .material
                .SetColor("_EmissionColor", Color.white);
            projectile.GetComponent<SpriteRenderer>().color = Color.white;
            projectile
                .GetComponent<SpriteRenderer>()
                .material
                .SetColor("_EmissionColor", Color.white);
        }
    }
}
