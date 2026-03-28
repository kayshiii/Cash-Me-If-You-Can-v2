using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI musicPercentText;
    [SerializeField] private TextMeshProUGUI sfxPercentText;

    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";

    private void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicKey, 1f);
        float savedSfx = PlayerPrefs.GetFloat(SfxKey, 1f);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSfx;

        if (musicSource != null) musicSource.volume = savedMusic;
        SetSfxVolume(savedSfx);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);

        UpdateMusicText(savedMusic);
        UpdateSfxText(savedSfx);
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;

        PlayerPrefs.SetFloat(MusicKey, value);
        UpdateMusicText(value);
    }

    public void SetSfxVolume(float value)
    {
        if (sfxSources != null)
        {
            for (int i = 0; i < sfxSources.Length; i++)
                if (sfxSources[i] != null)
                    sfxSources[i].volume = value;
        }

        PlayerPrefs.SetFloat(SfxKey, value);
        UpdateSfxText(value);
    }

    private void UpdateMusicText(float value)
    {
        if (musicPercentText != null)
            musicPercentText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
    }

    private void UpdateSfxText(float value)
    {
        if (sfxPercentText != null)
            sfxPercentText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
    }
}
