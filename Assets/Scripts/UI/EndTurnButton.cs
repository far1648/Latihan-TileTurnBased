using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndTurnButton : MonoBehaviour, IDeselectHandler
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string normalText = "End Move";
    [SerializeField] private string confirmText = "Sure? Click Again";

    private bool armed;

    void OnEnable()
    {
        Disarm();
    }

    public void OnClick()
    {
        var state = GameManager.Instance.GameState;
        bool isTurn = state == GameState.HeroesTurn || state == GameState.EnemiesTurn;

        if (!isTurn || TurnManager.Instance.InputBlocked) return;

        if (!armed)
        {
            Arm();
            return;
        }

        Disarm();
        TurnManager.Instance.EndTurnEarly();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Disarm();
    }

    public void Disarm()
    {
        armed = false;
        label.text = normalText;
    }

    public void Arm()
    {
        armed = true;
        label.text = confirmText;
    }
}
