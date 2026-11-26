using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private DialogData dialog;
    [SerializeField] private DialogData weaponTriangleDialog;
    [SerializeField] private bool overlayMode;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI speakerText, bodyText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private float charPerSecond = 40f;

    private static readonly HashSet<string> played = new HashSet<string>();

    private readonly Queue<DialogData> pending = new Queue<DialogData>();
    private DialogData current;
    private int index;
    private int startFrame;
    private bool isTyping;
    private Coroutine typing;

    public bool IsPlaying => current != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatistics()
    {
        played.Clear();
    }

    private void Awake()
    {
        Instance = this;

        if (overlayMode && panelRoot != null) panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (overlayMode)
        {
            if (dialog != null) Play(dialog);
            return;
        }
        
        if (dialog == null || dialog.Lines.Count == 0)
        {
            LoadNextScene(null);
            return;
        } 

        Begin(dialog);
    }

    public void Play(DialogData data)
    {
        if (!overlayMode) return;
        if (data == null || data.Lines.Count == 0) return;
        if (data.PlayOncePerSession && played.Contains(data.name)) return;

        if (current != null)
        {
            pending.Enqueue(data);
            return;
        }

        Begin(data);
    }

    public void PlayWeaponTriangleHint()
    {
        Play(weaponTriangleDialog);
    }

    private void Begin(DialogData data)
    {
        current = data;
        index = 0;
        startFrame = Time.frameCount;

        if (overlayMode)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            TurnManager.Instance.DialogOpen = true;
        }

        ShowLine(current.Lines[0]);
    }

    private void Update() 
    {
        if (current == null) return;
        if (Time.frameCount == startFrame) return;
        if (!WasAdvancePressed()) return;

        if (isTyping)
        {
            CompleteLine();
            return;
        }

        index++;
        
        if (index >= current.Lines.Count)
        {
            Finish();
            return;
        }
        ShowLine(current.Lines[index]);
    }

    private bool WasAdvancePressed()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool space = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;

        return click || space;
    }

    private void ShowLine(DialogData.Line line)
    {
        speakerText.text = line.Speaker;

        portraitImage.sprite = line.portrait;
        portraitImage.enabled = line.portrait != null;

        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(TypeRoutine(line.Text));
    }

    private IEnumerator TypeRoutine(string text)
    {
        isTyping = true;
        bodyText.text = text;
        bodyText.maxVisibleCharacters = 0;

        float shown = 0f;

        while (bodyText.maxVisibleCharacters < text.Length)
        {
            shown += charPerSecond * Time.deltaTime;
            bodyText.maxVisibleCharacters = Mathf.Min(text.Length, Mathf.FloorToInt(shown));
            yield return null;
        }

        isTyping = false;
        typing = null;
    }

    private void CompleteLine()
    {
        if (typing != null) StopCoroutine(typing);

        bodyText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        typing = null;
    }
    
    private void Finish()
    {
        var finished = current;
        current = null;

        if (!overlayMode)
        {
            LoadNextScene(finished);
            return;
        }

        played.Add(finished.name);

        if (pending.Count>0)
        {
            Begin(pending.Dequeue());
            return;
        }

        if (panelRoot != null) panelRoot.SetActive(false);
        StartCoroutine(ReleaseInput());
    }

    private IEnumerator ReleaseInput()
    {
        yield return null;

        if (current == null) TurnManager.Instance.DialogOpen = false;
    }

    private void LoadNextScene(DialogData finished)
    {
        ScreenFader.LoadScene(finished != null ? finished.NextScene : "TutorialGameplay");
    }
}