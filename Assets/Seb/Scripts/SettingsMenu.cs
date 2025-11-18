using UnityEngine;
using Input = UnityEngine.Input;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu instance;

    [SerializeField] private Slider sensitivitySlider;

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

        try
        {
            SetSenVal(PlayerPrefs.GetFloat("sensitivity"));
        }
        catch
        {
            SetSenVal(0.5f);
        }
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

    public void SetSenVal(float val)
    {
        //Debug.Log("SFX vol: " + vol);
        sensitivitySlider.value = val;

        PlayerPrefs.SetFloat("sensitivity", val);
        PlayerPrefs.Save();
    }

    public float GetSenVal()
    {
        return sensitivitySlider.value;
    }
}