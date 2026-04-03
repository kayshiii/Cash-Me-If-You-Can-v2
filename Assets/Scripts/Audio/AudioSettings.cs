using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    private AudioSource musicSource;
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshAllReferences();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshAllReferences();
    }

    public void RefreshAllReferences()
    {
        GameObject musicObj = GameObject.FindWithTag("Music");
        if (musicObj != null) musicSource = musicObj.GetComponent<AudioSource>();

        Slider[] allSliders = Resources.FindObjectsOfTypeAll<Slider>();
        foreach (Slider s in allSliders)
        {
            if (s.name == "MusicSlider")
            {
                s.onValueChanged.RemoveAllListeners();
                s.value = PlayerPrefs.GetFloat(MusicKey, 0.75f);
                s.onValueChanged.AddListener(SetMusicVolume);
            }
            else if (s.name == "SFXSlider")
            {
                s.onValueChanged.RemoveAllListeners();
                s.value = PlayerPrefs.GetFloat(SfxKey, 0.75f);
                s.onValueChanged.AddListener(SetSfxVolume);
            }
        }

        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        float mVol = PlayerPrefs.GetFloat(MusicKey, 0.75f);
        float sVol = PlayerPrefs.GetFloat(SfxKey, 0.75f);

        if (musicSource != null) musicSource.volume = mVol;

        GameObject[] sfxObjects = GameObject.FindGameObjectsWithTag("SFX");
        foreach (GameObject obj in sfxObjects)
        {
            AudioSource source = obj.GetComponent<AudioSource>();
            if (source != null) source.volume = sVol;
        }

        UpdateTextUI(mVol, sVol);
    }

    private void UpdateTextUI(float m, float s)
    {
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var t in allTexts)
        {
            if (t.name == "Music Text") t.text = Mathf.RoundToInt(m * 100) + "%";
            if (t.name == "SFX Text") t.text = Mathf.RoundToInt(s * 100) + "%";
        }
    }

    public void SetMusicVolume(float val)
    {
        PlayerPrefs.SetFloat(MusicKey, val);
        if (musicSource != null) musicSource.volume = val;
        UpdateTextUI(val, PlayerPrefs.GetFloat(SfxKey, 0.75f));
    }

    public void SetSfxVolume(float val)
    {
        PlayerPrefs.SetFloat(SfxKey, val);
        ApplyVolumes();
    }
}