using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MagierController : MonoBehaviour
{
    protected new Rigidbody2D rigidbody2D;
    protected Animator animator;

    public GameObject player;

    public int maxHealth = 2;
    public int currentHealth;
    public int health { get { return currentHealth; } }

    System.Random dice = new System.Random();
    public float waitTime = 20.0f;
    private float timeSinceLastSpell = 0.0f;

    public float magicRange = 21.0f;

    public Vector2 currentPosition() { return GetComponent<Rigidbody2D>().position; }

    public void ChangeHealth(int amount)
    {
        EnemyController[] foundMinions = FindObjectsOfType<EnemyController>();

        // Solange es noch Gegner gibt kann der Magier nicht getötet werden
        if (foundMinions.Length == 0)
        {
            // Aktuelle Gesundheit muss zwischen 0 und maxHealth bleiben
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            Debug.Log("Magier Gesundheit " + currentHealth + " von " + maxHealth);

            if (currentHealth == 0)
                Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        timeSinceLastSpell = 0.0f;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpell += Time.deltaTime;

        // Letzte Attacke länger als 2s her? Zurück in Idle Animation
        if (timeSinceLastSpell > 2.0f)
            animator.SetFloat("Attack", 0.0f);

        // Wartezeit um? 
        if (timeSinceLastSpell > waitTime)
        {
            // Dann Attacke
            animator.SetFloat("Attack", 1.0f);

            // Nächste Wartezeit auswürfeln
            waitTime = dice.Next(10, 30);
            timeSinceLastSpell = 0.0f;

            Debug.Log("Zauberbeginn");
            float maxDistance = 0.0f;
            EnemyController farFarAway = null;

            // Kollider innerhalb der Reichweite ermitteln
            Collider2D[] collidersInReach = Physics2D.OverlapCircleAll(rigidbody2D.position, magicRange, 1 << LayerMask.NameToLayer("Enemies"));
            Debug.Log(collidersInReach.Length + " minion gefunden ");
            foreach (Collider2D other in collidersInReach)
            {
                if (other.gameObject != player)
                {
                    // Enemy mit der größten Entfernung zum Magier suchen und merken
                    float distance = Vector2.Distance(currentPosition(), other.transform.transform.position);
                    if (distance > maxDistance)
                    {
                        Debug.Log("Teste " + other.gameObject.name + " mit Abstand " + distance);

                        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();

                        if (enemy != null)
                        {
                            farFarAway = enemy;
                            maxDistance = distance;
                        }
                    }
                }
            }

            // Position von Spieler und Gegener tauschen
            if (farFarAway != null)
            {
                Debug.Log("Tausche " + farFarAway + " mit Player.");

                SpielerController playControl = player.gameObject.GetComponent<SpielerController>();
                Vector2 temp = playControl.currentPosition();
                playControl.setCurrentPosition(farFarAway.currentPosition());
                farFarAway.setCurrentPosition(temp);
            }
        }
    }
}
