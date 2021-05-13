using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDestroyer : MonoBehaviour
{
    Animator _animator;

    void Start()
    {
        _animator = this.GetComponent<Animator>();
        Destroy(gameObject, _animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
