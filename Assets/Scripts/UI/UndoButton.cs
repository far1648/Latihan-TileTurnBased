using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UndoButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    private Key shortcut = Key.Z;

    private void Awake()
    {
        button.onClick.AddListener(TryUndo);
    }

    private void Update()
    {
        var undo = UndoManager.Instance;

        if (undo == null) return;

        button.interactable = undo.CanUndo;
        if (undo.UndosLeft == 0)
        {
            label.text = "Deja Vu! (Run out)";
        }
        if (undo.UndosLeft == 1)
        {
            label.text = "Deja Vu! (One left)";
        }
        if (undo.UndosLeft == 2)
        {
            label.text = "Deja Vu! (Two left)";
        }
        if (undo.UndosLeft == 3)
        {
            label.text = "Deja Vu! (Three left)";
        }

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[shortcut].wasPressedThisFrame) TryUndo();        
    }

    private void TryUndo()
    {
        if (UndoManager.Instance == null || !UndoManager.Instance.Undo()) return;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
