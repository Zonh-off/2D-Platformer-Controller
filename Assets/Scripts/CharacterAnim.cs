using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnim : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("runSpeed", Mathf.Abs(controller.GetMovement()));
        animator.SetBool("isWallSlide", controller.IsWallSlide());
        animator.SetBool("isJumping", controller.IsJumping());
    }
}
