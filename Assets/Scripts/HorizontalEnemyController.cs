using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HorizontalEnemyController : EnemyController
{   
    protected override void computeNextEndposition()
    {
        startPosition = currentPosition();
        if (vertical)
        {
            endPosition.x = startPosition.x;
            endPosition.y = roundToHalf(startPosition.y + (fieldwidth * direction));
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", direction);
        }
        else
        {
            endPosition.x = roundToHalf(startPosition.x + (fieldwidth * direction));
            endPosition.y = startPosition.y;
            animator.SetFloat("MoveX", direction);
            animator.SetFloat("MoveY", 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        fieldwidth = 1.0f;
    }

}
