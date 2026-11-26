using UnityEngine;

[RequireComponent(typeof(BaseUnit))]
public class UnitBars : MonoBehaviour
{
    private const float Width = 0.8f;
    private const float Height = 0.09f;
    private const float Gap = 0.03f;
    private const float OffsetY = 0.55f;

    private static readonly Color BackColor = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color HpColor = new Color(0.3f, 0.85f, 0.35f, 1f);
    private static readonly Color ShieldColor = new Color(0.35f, 0.75f, 1f, 1f);

    private static Sprite whiteSprite;

    private BaseUnit unit;
    private BossEnemy boss;

    private Transform root;
    private Transform hpFill;
    private Transform shieldFill;

    private static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                whiteSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0f, 0.5f),
                    tex.width
                );
            }

            return whiteSprite;
        }
    }

    void Awake()
    {
        unit = GetComponent<BaseUnit>();
        boss = GetComponent<BossEnemy>();

        var rootObject = new GameObject("Bars");
        root = rootObject.transform;
        root.SetParent(transform, false);

        var scale = transform.lossyScale;
        float sx = Mathf.Approximately(scale.x, 0f) ? 1f : scale.x;
        float sy = Mathf.Approximately(scale.y, 0f) ? 1f : scale.y;
        root.localScale = new Vector3(1f / sx, 1f / sy, 1f);

        hpFill = CreateBar(Height + Gap, HpColor, 100);

        if (boss !=  null)
        {
            shieldFill = CreateBar(Height + Gap, ShieldColor, 100);
        }
    }

    private Transform CreateBar(float y, Color fillColor, int order)
    {
        CreatePart("Back", y, BackColor, order);
        return CreatePart("Fill", y, fillColor, order + 1);
    }

    private Transform CreatePart(string partName, float y, Color color, int order)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(-Width/2f, y, 0f);
        go.transform.localScale = new Vector3(Width, Height, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        sr.sortingOrder = order;

        return go.transform;
    }

    private void SetFill(Transform fill, float ratio)
    {
        fill.localScale = new Vector3(Width * Mathf.Clamp01(ratio), Height, 1f);
    }

    void LateUpdate()
    {
        root.position = transform.position + Vector3.up * OffsetY;

        float hp = unit.MaxHealth > 0 ? (float)unit.CurrentHealth/unit.MaxHealth : 0f;
        SetFill(hpFill, hp);

        if (shieldFill != null)
        {
            float shield = boss.MaxShield > 0 ? (float)boss.Shield/boss.MaxShield : 0f;
            SetFill(shieldFill, shield);
        }
    }
}