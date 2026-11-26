using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance {get; private set;}

    private const float Duration = 0.4f;
    private Image overlay;
    private Coroutine running;
    private bool isLoading;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("Screen Fader");
        DontDestroyOnLoad(go);
        go.AddComponent<ScreenFader>();
    }

    void Awake()
    {
        Instance = this;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        gameObject.AddComponent<GraphicRaycaster>();

        var overlayObject = new GameObject("Overlay", typeof(RectTransform));
        overlayObject.transform.SetParent(transform, false);

        var rect = (RectTransform)overlayObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay = overlayObject.AddComponent<Image>();
        overlay.color = Color.black;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        Fade(0f);
    }

    private void Fade(float target, System.Action onDone = null)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(target, onDone));
    }

    private IEnumerator FadeRoutine(float target, System.Action onDone)
    {
        overlay.raycastTarget = true;

        float start = overlay.color.a;
        float elapsed = 0f;

        while (elapsed < Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(start, target, elapsed / Duration));
            yield return null;
        }

        SetAlpha(target);
        overlay.raycastTarget = target > 0f;
        running = null;

        onDone?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        var c = overlay.color;
        c.a = alpha;
        overlay.color = c;
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.BeginLoad(() => SceneManager.LoadScene(sceneName));
    }

    public static void LoadScene(int buildIndex)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(buildIndex);
            return;
        }
        Instance.BeginLoad(() => SceneManager.LoadScene(buildIndex));
    }

    private void BeginLoad(System.Action load)
    {
        if (isLoading) return;

        isLoading = true;
        Fade(1f, load);
    }
}
