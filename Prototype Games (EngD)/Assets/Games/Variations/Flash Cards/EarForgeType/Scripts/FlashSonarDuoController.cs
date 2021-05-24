using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlashSonarDuoController : MonoBehaviour
{
    public GameObject[] hearts;

    public TextMeshProUGUI totalCorrectText;

    public TextMeshProUGUI totalInCorrectText;

    public TextMeshProUGUI streakText;

    public TextMeshProUGUI endGameText;

    public GameObject sonarChoices;

    public GameObject tickLine;

    public bool usingSonarMode = false;

    private SonarNotesController sonarNotesController;

    private EarForgeType earForgeController;

    void Start()
    {
        sonarNotesController = GetComponent<SonarNotesController>();
        earForgeController = GetComponent<EarForgeType>();

        if (sonarNotesController && earForgeController)
        {
            sonarNotesController.flatNotations =
                earForgeController.flatNotationImages;
            sonarNotesController.sharpNotations =
                earForgeController.sharpNotationImages;
        }
        else
            Debug.LogError("No Controllers Attached!");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) RestartGame();
        //if esc is pressed, quit
    }

    public void SwitchGameType()
    {
        if (usingSonarMode)
        {
            usingSonarMode = false;
            sonarChoices.SetActive(false);
            tickLine.SetActive(false);
            sonarNotesController.enabled = false;
            earForgeController.enabled = true;
        }
        else
        {
            usingSonarMode = true;
            sonarChoices.SetActive(true);
            tickLine.SetActive(true);
            sonarNotesController.enabled = true;
            earForgeController.enabled = false;
        }
    }

    public void RemoveLife()
    {
        foreach (GameObject heart in hearts)
        if (heart.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            Camera.main.backgroundColor = Color.red;
            heart.transform.GetChild(0).gameObject.SetActive(false);
            break;
        }
    }

    public void LevelComplete()
    {
        endGameText.text = "Level Complete!\nPress 'R' to Restart";
        Time.timeScale = 0;
        //save scores etc.
        //play sounds
    }

    public void GameOver()
    {
        endGameText.text = "Game Over!\nPress 'R' to Restart";
        Time.timeScale = 0;
        //show scores etc.
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //some menu controls
    //colour
    //notation
    //survival mode?
}
