using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeAnim : MonoBehaviour
{
    public Animator _animator;

    public Color explosionColor;

    void Start()
    {
        var child = transform.GetChild(0);

        foreach (Transform grandChild in child)
        {
            grandChild.GetComponent<SpriteRenderer>().color = explosionColor; // not quite right
        }

        Destroy(gameObject, _animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
