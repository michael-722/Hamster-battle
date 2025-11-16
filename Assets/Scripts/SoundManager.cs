using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        //masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXSlider.onValueChanged.AddListener(SetSFXVolume);

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadVolume();
        }
        else
        {
            //SetMasterVolume(0.75f);
            SetMusicVolume(0.75f);
            SetSFXVolume(0.75f);
        }
    }

    public void SetMasterVolume(float vol)
    {
        masterSlider.value = vol;
        myMixer.SetFloat("Master", Mathf.Log10(vol)*20);
        PlayerPrefs.SetFloat("MasterVolume", vol);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float vol)
    {
        //Debug.Log("music vol: " + vol);
        musicSlider.value = vol;
        myMixer.SetFloat("Music", Mathf.Log10(vol)*20);
        PlayerPrefs.SetFloat("MusicVolume", vol);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float vol)
    {
        //Debug.Log("SFX vol: " + vol);
        SFXSlider.value = vol;
        myMixer.SetFloat("SFX", Mathf.Log10(vol)*20);
        PlayerPrefs.SetFloat("SFXVolume", vol);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        try
        {
            //SetMasterVolume(PlayerPrefs.GetFloat("SetMasterVolume"));
            
            Debug.Log("loaded music vol: " + PlayerPrefs.GetFloat("MusicVolume"));
            Debug.Log("loaded SFX vol: " + PlayerPrefs.GetFloat("SFXVolume"));

            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
        }
        catch
        {
            //SetMasterVolume(0.75f);
            SetMusicVolume(0.75f);
            SetSFXVolume(0.75f);
        }
    }
}