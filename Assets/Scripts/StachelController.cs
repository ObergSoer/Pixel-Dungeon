using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StachelController : MonoBehaviour
{
    protected Animator animator;

    public bool triggerActivated = false;
    public float waitTime = 5.0f;
    private float timeSinceLastActivated = 0.0f;
    private bool steppedOn = false;

    private bool attacking = false;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        timeSinceLastActivated = 0.0f;
    }

    void TimedUpdate()
    {
        timeSinceLastActivated += Time.deltaTime;

        // Letzte Attacke länger als 2s her? Zurück in Idle Animation
        if (timeSinceLastActivated > 1.0f)
        {
            animator.SetBool("Attack", false);
            attacking = false;
        }

        // Wartezeit ist rum 
        if (timeSinceLastActivated > waitTime)
        {
            // Dann Attacke
            animator.SetBool("Attack", true);
            attacking = true;
            timeSinceLastActivated = 0.0f;
        }
    }

    void TriggerUpdate()
    {
        timeSinceLastActivated += Time.deltaTime;

        // Eine Sekunde nach dem Trigger auslösen
        if (steppedOn && 
            !attacking &&
            (timeSinceLastActivated > 1.0f))
        {
            animator.SetBool("Attack", true);
            attacking = true;            
        }

        // Nach zwei Sekunden wieder in den Ruhezustand 
        if (attacking && (timeSinceLastActivated > 2.0f))
        {
            // Dann Attacke
            animator.SetBool("Attack", false);
            attacking = false;
            steppedOn = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (triggerActivated)
            TriggerUpdate();
        else
            TimedUpdate();        
    }

    void OnTriggerStay2D(Collider2D other)
    {
        SpielerController player = other.gameObject.GetComponent<SpielerController>();
        if (player != null)
        {
            if (attacking)
                player.ChangeHealth(-1);
            else
            {
                if (triggerActivated && !steppedOn)
                {
                    steppedOn = true;
                    timeSinceLastActivated = 0.0f;
                }
            }
            
        }
    }
}
