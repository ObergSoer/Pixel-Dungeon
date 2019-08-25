using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Collections;
using System;

class SpielerController : MonoBehaviour
{
    // Zählt wieviele Level hat der Spieler schon erfolgreich gespielt hat
    static public int levelsPlayed = 1;

    //! Der physikalische "Körper" unseres Objekts
    private new Rigidbody2D rigidbody2D;
    protected Animator animator;

    public bool debugOutput = false;

    //! Geschwindigkeit der Bewegung. Im Unity-Editor überschreibbar
    public float moveSpeed = 3.0f;

    //! Breite eines Feldes im Grid
    private float fieldwidth = 1.0f;

    //! Status ob eine neue Bewegung erlaubt ist oder sich noch eine in der Ausführung befindet
    public bool moveAllowed = true;

    public float waitTime = 1.0f;
    private float timeSinceStartMove = 0.0f;

    //! Startposition vor der aktuellen Bewegung
    private Vector2 startPosition;

    //! Zielposition auf die sich die Bewegung zubewegen soll
    private Vector2 endPosition;

    //! Aktuelle Position als Faktor [0-1]
    private float factorMove;

    public int maxHealth = 3;
    public int currentHealth;
    public int health { get { return currentHealth; } }

    public Vector2 currentPosition() { return rigidbody2D.position; }
    public void    setCurrentPosition(Vector2 newPos) { rigidbody2D.position = newPos; }

    public bool standing = true;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public float timeInvincible = 1.0f;
    bool isInvincible = true;
    float invincibleTimer;

    public void ChangeHealth(int amount)
    {
        if (amount < 0) // Schaden?
        {
            if (isInvincible) // Unverwundbar
                return;

            // Nachdem wir verwundet wurden sind wir einen Moment unverwundbar
            isInvincible = true;
            invincibleTimer = timeInvincible;
        }

        // Aktuelle Gesundheit muss zwischen 0 und maxHealth bleiben
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth == 0)
        {
            // Zur "GameOver"-Scene wechseln. Das ist immer die letzte Scene im SceneManager
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
        }
    }

    private void Log(object message)
    {
        if (debugOutput)
            Debug.Log(message);
    }

    //! Hilfsfunktion rundet Werte auf 0.5er Schritte
    private float roundToHalf(float value)
    {
        // Beispiel gg. 4.6: 4.6 * 2 = 9.2. Gerundet 9. Mal 0.5 = 4.5
        return (float) Math.Round(value * 2.0f, MidpointRounding.AwayFromZero) * 0.5f;
    }

    //! Ermittelt ob der User Eingaben gemacht hat und liefert einen Richtungsvector der Länge 1
    private Vector2 checkForInputVector()
    {
        // Achsen des Controllers abfragen
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Diagonale Bewegungen entfernen
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            // X-Achse überwiegt
            // Sign liefert -1/0/1 für links/nix/rechts
            input.x = System.Math.Sign(input.x);
            input.y = 0.0f;
        }
        else
        {
            // Y-Achse überwiegt
            // Sign liefert -1/0/1 für oben/nix/unten
            input.x = 0.0f;
            input.y = System.Math.Sign(input.y);
        }

        return input;
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

            // Nee. Noch weiter entfernt

            // Geschwindigkeit * Zeit seit dem letzten Zeichnen auf unseren Faktor addieren
            // Faktor größer als 1 ist nicht erlaubt
            factorMove = Math.Min(factorMove + (Time.deltaTime * moveSpeed), 1.0f);
            Log("factorMove: " + factorMove);

            // Neue Position auf unserem Vector von start- nach endPosition berechnen
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, factorMove);
            Log("newPosition: " + newPosition);

            // Body auffordern sich dorthin zu begeben. 
            // Es ist nicht sicher, dass er diese Position auch erreichen kann (Wände etc.)
            rigidbody2D.MovePosition(newPosition);

            // Wir hätten die Position jetzt erreichen sollen. Also: Nicht mehr Bewegen
            if ((factorMove >= 1.0f)      || 
                (newPosition == endPosition))
            {
                Log("Faktor >= 1");
                endPosition = startPosition;
                factorMove = 0.0f;
            }
        }
        else
        {
            // Sind da
            Log("Position erreicht");
            factorMove = 0.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Körper initialisieren
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Volle Gesundheit
        currentHealth = maxHealth;
    }

    // Update wird vor jeden Framezeichnen aufgerufen
    public void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            SceneManager.LoadScene(0);

        for (int i=0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
                hearts[i].sprite = fullHeart;

            else
                hearts[i].sprite = emptyHeart;


            if (i < maxHealth)
                hearts[i].enabled = true;
            
            else
                hearts[i].enabled = false;
        }

        standing = (endPosition == currentPosition());

        // Aktuell keine Bewegung in der Bearbeitung?
        if (moveAllowed)
        {
            // Achsen des Controllers abfragen
            Vector2 input = checkForInputVector();

            // Haben wir eine Bewegungsrichtung?
            if (input != Vector2.zero)
                Log("Input: " + input);

            // Startposition merken
            startPosition = currentPosition();

            // Endposition auf die Mitte des nächsten Feldes setzen
            endPosition = new Vector2(roundToHalf(startPosition.x + (fieldwidth * input.x)),
                                      roundToHalf(startPosition.y + (fieldwidth * input.y)));

            animator.SetFloat("MoveX", input.x);
            animator.SetFloat("MoveY", input.y);

            // Eingabe sperren
            if (endPosition != startPosition)
            {
                moveAllowed = false;
                timeSinceStartMove = 0.0f;
            }
        }

        if (isInvincible)
        {   
            if (rigidbody2D.velocity != Vector2.zero)
            {
                Debug.Log("Beschleunigung nach Stoss " + rigidbody2D.velocity);
                rigidbody2D.velocity = new Vector2(0.0f, 0.0f);
                Debug.Log("Beschleunigung auf Null gesetzt");
            }            
        }

        // Sind nicht an Endposition
        if (endPosition != startPosition)
        {
            // Bewegung einleiten
            Log("Neue Bewegung");
            Log("Startposition: " + startPosition);
            Log("Zielposition: " + endPosition);

            move();
        }

        timeSinceStartMove += Time.deltaTime;

        // Nach der vorgegebenen Wartezeit den Zug für beendet erklären und neue Eingaben zulassen
        if (timeSinceStartMove > waitTime)
            moveAllowed = true;

        // Sind wir unverwundbar?
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            // Ist die Schonfrist abgelaufen?
            if (invincibleTimer < 0)
                isInvincible = false;
        }

    }

    void OnCollisionEnter2D(Collision2D other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        MagierController magier = other.gameObject.GetComponent<MagierController>();
        bool bounceAway = false;
        Vector2 enemyPosition = currentPosition();

        if (enemy != null)
        {
            if (!standing && !enemy.standing)
            {
                Debug.Log("Beide in Bewegung");
                ChangeHealth(-1);
                enemy.ChangeHealth(-1);
            }
            else
            {
                if (standing)
                {
                    Debug.Log("Spieler getroffen");
                    ChangeHealth(-1);
                }

                if (enemy.standing)
                {
                    // Player hat gewonnen und bleibt am Feld stehen
                    enemy.ChangeHealth(-1);
                    Debug.Log("Skelett getroffen");
                }
            }

            bounceAway = true;
            enemyPosition = enemy.currentPosition();
        }
            
        if (magier != null)
        {
            magier.ChangeHealth(-1);
            bounceAway = true;
            enemyPosition = magier.currentPosition();
        }
        
        // Wurde getroffen und legt den Rückwärtsgang ein
        if (bounceAway)
        {
            Vector2 myPosition = currentPosition();

            if (enemyPosition.x < myPosition.x)
                endPosition.x = roundToHalf(myPosition.x + fieldwidth);
            else
                endPosition.x = roundToHalf(myPosition.x - fieldwidth);

            if (enemyPosition.y < myPosition.y)
                endPosition.y = roundToHalf(myPosition.y + fieldwidth);
            else
                endPosition.y = roundToHalf(myPosition.y - fieldwidth);

            factorMove = 0.0f;
            move();
        }
    }


}