using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Input = UnityEngine.Input;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu instance;

    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

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

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXSlider.onValueChanged.AddListener(SetSFXVolume);

        LoadVolume();
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
        if (Input.GetKeyDown(KeyCode.Escape) && !LeaderboardManager.instance.GetLeaderboardUIState())
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                LoadVolume();
                PauseGame();
            }
        }
    }

    public void SetMasterVolume(float vol)
    {
        //Debug.Log("master vol: " + vol);
        masterSlider.value = vol;
        myMixer.SetFloat("Master", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("MasterVolume", vol);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float vol)
    {
        //Debug.Log("music vol: " + vol);
        musicSlider.value = vol;
        myMixer.SetFloat("Music", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("MusicVolume", vol);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float vol)
    {
        //Debug.Log("SFX vol: " + vol);
        SFXSlider.value = vol;
        myMixer.SetFloat("SFX", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", vol);
        PlayerPrefs.Save();
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

        PlayerRace.instance.ResetPlayer();
        RaceHUD.Instance.ResetHUD();

        syncedCountdown.instance.StopCountdown();

        isPaused = false;
        
        LeaderboardManager.instance.OnLeaveSinglePlayerPressed();
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
        try
        {
            return sensitivitySlider.value;
        }
        catch { return 0.0f; }
    }

    private void LoadVolume()
    {
        try { SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume")); }
        catch { SetMasterVolume(0.75f); }

        try { SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume")); }
        catch { SetMusicVolume(0.75f); }

        try { SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume")); }
        catch { SetSFXVolume(0.75f); }
    }
}