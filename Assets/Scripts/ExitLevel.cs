using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ExitLevel : MonoBehaviour
{
    System.Random myRandom = new System.Random();

    void OnTriggerEnter2D(Collider2D other)
    {
        SpielerController player = other.gameObject.GetComponent<SpielerController>();

        if (player != null)
        {
            SpielerController.levelsPlayed++;

            Debug.Log("Spieler spielt sein " + SpielerController.levelsPlayed + " Level");

            int anzahlLevel = SceneManager.sceneCountInBuildSettings;
            int indexCurrentScene = SceneManager.GetActiveScene().buildIndex;
            int indexNextScene = indexCurrentScene;
            Debug.Log("Aktuelles Level: " + indexCurrentScene);
            Debug.Log("Anzahl Level: " + anzahlLevel);

            if (SpielerController.levelsPlayed == 5)
            {
                // Player hat 5 Level geschafft also gewonnen
                // Ab zur vorletzten Scene: Unserer "Win"-Scene
                indexNextScene = anzahlLevel - 2;
            }
            else
            {
                while (indexCurrentScene == indexNextScene)
                {
                    indexNextScene = myRandom.Next(2, anzahlLevel - 1);
                }
            }
            

            Debug.Log("Neues Level: " + indexNextScene);
            SceneManager.LoadScene(indexNextScene);
        }
    }

}
