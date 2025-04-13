using UnityEngine;

public class VolumeUIController : MonoBehaviour {
    [SerializeField] private UnityEngine.UI.Slider masterVolumeSlider;
    [SerializeField] private UnityEngine.UI.Slider sfxVolumeSlider;
    [SerializeField] private UnityEngine.UI.Slider bgmVolumeSlider;
    [SerializeField] private UnityEngine.UI.Slider uiVolumeSlider;

    private AudioManager.VolumeControl volumeControl;

    private void Start() {
        if (AudioManager.Instance == null) {
            Debug.LogError("AudioManager instance not found!");
            return;
        }

        volumeControl = AudioManager.Instance.VolumeControls;

        // Initialize sliders to current values
        if (masterVolumeSlider != null) {
            masterVolumeSlider.value = volumeControl.GetMasterVolume();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (sfxVolumeSlider != null) {
            sfxVolumeSlider.value = volumeControl.GetSFXVolume();
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (bgmVolumeSlider != null) {
            bgmVolumeSlider.value = volumeControl.GetBGMVolume();
            bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }

        if (uiVolumeSlider != null) {
            uiVolumeSlider.value = volumeControl.GetUIVolume();
            uiVolumeSlider.onValueChanged.AddListener(OnUiVolumeChanged);
        }
    }

    public void OnMasterVolumeChanged(float volume) {
        volumeControl.SetMasterVolume(volume);
    }

    public void OnSfxVolumeChanged(float volume) {
        volumeControl.SetSFXVolume(volume);
    }

    public void OnBgmVolumeChanged(float volume) {
        volumeControl.SetBGMVolume(volume);
    }

    public void OnUiVolumeChanged(float volume) {
        volumeControl.SetUIVolume(volume);
    }
}