using UnityEngine;
using Input = UnityEngine.Input;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu instance;

    public GameObject pauseMenu;
    public bool isPaused;

    private void Start()
    {
        if (instance != null)
        {
            Debug.LogWarning("more than one settingsMenu instance - destroying duplicate");
            Destroy(gameObject);
            return;
        }

        instance = this;
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);

        //Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        
        //Time.timeScale = 1f;
        isPaused = false;
    }

    public void MainMenu()
    {
        pauseMenu.SetActive(false);

        HamsterBallController hamsterBall = FindFirstObjectByType<HamsterBallController>();
        Destroy(hamsterBall.gameObject);

        LobbySetUp.instance.dissconnect();
        LobbySetUp.instance.showStartUI();
    }
}