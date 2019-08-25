using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyController : MonoBehaviour
{
    protected new Rigidbody2D rigidbody2D;
    protected Animator animator;

    private float directionTimer = 0.0f;
    public float changeDirectionTime = 3.0f;

    protected int direction = 1;
    public bool vertical = false;

    //! Geschwindigkeit der Bewegung. Im Unity-Editor überschreibbar
    public float moveSpeed = 3.0f;

    //! Länge einer diagonalen Bewegung im Grid
    protected float fieldwidth = 1.4142f;

    //! Status ob eine Bewegung noch in der Ausführung ist
    public bool moveAllowed = true;

    public float waitTime = 1.0f;
    private float timeSinceStartMove = 0.0f;

    //! Startposition vor der aktuellen Bewegung
    protected Vector2 startPosition;

    //! Zielposition auf die sich die Bewegung zubewegen soll
    protected Vector2 endPosition;

    //! Aktuelle Position als Faktor [0-1]
    private float factorMove;

    public int maxHealth = 2;
    public int currentHealth;
    public int health { get { return currentHealth; } }

    public Vector2 currentPosition() { return GetComponent<Rigidbody2D>().position; }
    public void    setCurrentPosition(Vector2 newPos) { rigidbody2D.position = newPos; }

    public bool standing = true;

    //! Hilfsfunktion rundet Werte auf 0.5er Schritte
    protected float roundToHalf(float value)
    {
        // Beispiel gg. 4.6: 4.6 * 2 = 9.2. Gerundet 9. Mal 0.5 = 4.5
        return (float)Math.Round(value * 2.0f, MidpointRounding.AwayFromZero) * 0.5f;
    }

    private void move()
    {
        // Sind wir schon daaa?
        if (startPosition != endPosition)
        {
            Vector2 fromToVector = (endPosition - startPosition);

            // Nur ein kleines Stück bis zum Ziel? Dann direkt drauf
            if (fromToVector.magnitude < (fieldwidth * 0.25))
            {
                rigidbody2D.MovePosition(endPosition);
                endPosition = startPosition;
                factorMove = 0.0f;
                return;
            }

            // Nee
            // Geschwindigkeit * Zeit seit dem letzten Zeichnen auf unseren Faktor addieren
            // Faktor größer als 1 ist nicht erlaubt
            factorMove = Math.Min(factorMove + (Time.deltaTime * moveSpeed), 1.0f);
            
            // Neue Position auf unserem Vector von start- nach endPosition berechnen
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, factorMove);

            // Body auffordern sich dorthin zu begeben. 
            // Es ist nicht sicher, dass er diese Position auch erreichen kann (Wände etc.)
            rigidbody2D.MovePosition(newPosition);

            // Wir hätten die Position jetzt erreichen sollen. Also: Stillgestanden
            if ((factorMove >= 1.0f) ||
                (newPosition == endPosition))
            {
                endPosition = startPosition;
                factorMove = 0.0f;
            }
        }
        else
        {
            // Sind da
            factorMove = 0.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        directionTimer = changeDirectionTime;
        currentHealth = maxHealth;
    }

    public void ChangeHealth(int amount)
    {   
        // Aktuelle Gesundheit muss zwischen 0 und maxHealth bleiben
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log("Enemy Gesundheit " + currentHealth + " von " + maxHealth);

        if (currentHealth == 0)
            Destroy(gameObject);
    }

    protected virtual void computeNextEndposition()
    {
        startPosition = currentPosition();
        if (vertical)
        {
            endPosition.x = roundToHalf(startPosition.y + (fieldwidth * direction));
            endPosition.y = roundToHalf(startPosition.y + (fieldwidth * direction));
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", direction);
        }
        else
        {
            endPosition.x = roundToHalf(startPosition.x + (fieldwidth * direction));
            endPosition.y = roundToHalf(startPosition.y - (fieldwidth * direction));
            animator.SetFloat("MoveX", direction);
            animator.SetFloat("MoveY", 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        directionTimer -= Time.deltaTime;
        standing = (startPosition == endPosition);

        // Aktuell keine Bewegung in der Bearbeitung?
        if (moveAllowed)
        {
            if (directionTimer < 0)
            {
                direction = -direction;
                directionTimer = changeDirectionTime;
            }
            computeNextEndposition();
            moveAllowed = false;
            timeSinceStartMove = 0.0f;
        }

        // Sind nicht an Endposition
        if (endPosition != startPosition)
            move();
        timeSinceStartMove += Time.deltaTime;

        // Nach der vorgegebenen Wartezeit den Zug für beendet erklären und einen neuen starten
        if (timeSinceStartMove > waitTime)
            moveAllowed = true;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        SpielerController player = other.gameObject.GetComponent<SpielerController>();

        if (player != null)
        {            
            direction = -direction; // Wurde getroffen und legt den Rückwärtsgang ein
            moveAllowed = true;
        }
    }
}
