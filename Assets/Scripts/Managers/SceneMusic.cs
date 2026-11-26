using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    // Nama file di Resources/Audio (tanpa ekstensi). Kosong = scene ini tanpa musik.
    [SerializeField] private string musicName;

    // Opsional: lagu pembuka yang dimainkan sekali sebelum musicName di-loop
    [SerializeField] private string introName;

    void Start()
    {
        if (string.IsNullOrEmpty(musicName))
        {
            AudioManager.Instance.StopMusic();
        }
        else
        {
            AudioManager.Instance.PlayMusic(musicName, introName);
        }
    }
}