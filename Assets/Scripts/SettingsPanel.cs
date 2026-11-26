using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        panel.SetActive(false);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        // Isi slider dari volume tersimpan tanpa memicu event
        volumeSlider.SetValueWithoutNotify(AudioManager.Instance.Volume);
        volumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.Volume = v);
    }

    public void Open()
    {
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        AudioManager.Instance.Save();
    }
}