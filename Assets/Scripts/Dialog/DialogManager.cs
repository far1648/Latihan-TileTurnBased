using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private DialogData dialog;
    [SerializeField] private TextMeshProUGUI speakerText, bodyText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private float charPerSecond = 40f;

    private int index;
    private bool isTyping;
    private Coroutine typing;

    private void Start()
    {
        if (dialog == null || dialog.Lines.Count == 0)
        {
            Finish();
            return;
        }

        index = 0;
        ShowLine(dialog.Lines[index]);
    }

    private void Update() 
    {
        if (!WasAdvancePressed()) return;

        if (isTyping)
        {
            CompleteLine();
            return;
        }

        index++;
        if (index >= dialog.Lines.Count)
        {
            Finish();
            return;
        }
        ShowLine(dialog.Lines[index]);
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
        SceneManager.LoadScene(dialog != null ? dialog.NextScene : "TutorialGameplay");
    }
}