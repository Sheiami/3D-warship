using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ShipStatsPanel : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] Image hpFill;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Color accent = new Color(0.79f, 0.64f, 0.29f, 1f);
    [SerializeField] Color emptyAmmoColor = new Color(0.88f, 0.42f, 0.42f, 1f);
    [SerializeField] float refreshInterval = 0.1f;

    CanvasGroup group;
    Ship shown;
    float nextRefresh;
    string accentHex, emptyHex;
    readonly StringBuilder sb = new StringBuilder(256);
    readonly List<AmmoData> seenAmmo = new List<AmmoData>();

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;
        accentHex = ColorUtility.ToHtmlStringRGB(accent);
        emptyHex = ColorUtility.ToHtmlStringRGB(emptyAmmoColor);
    }

    void Update()
    {
        SelectionManager sm = SelectionManager.Instance;
        Ship ship = sm == null ? null : (sm.Hovered ? sm.Hovered : sm.Selected);

        if (ship != shown || Time.unscaledTime >= nextRefresh)
        {
            shown = ship;
            nextRefresh = Time.unscaledTime + refreshInterval;
            Render(ship);
        }
    }

    void Render(Ship ship)
    {
        if (!ship || !ship.Data) { group.alpha = 0f; return; }
        group.alpha = 1f;

        ShipData d = ship.Data;
        titleText.text = ship.Team == Team.Enemy ? d.displayName + " (ворог)" : d.displayName;

        float max = Mathf.Max(1f, ship.Health.MaxHP);
        float cur = ship.Health.CurrentHP;
        hpText.text = cur.ToString("0") + " / " + max.ToString("0");
        hpFill.fillAmount = Mathf.Clamp01(cur / max);

        sb.Clear();
        sb.Append("Швидкість: ").Append(d.speed.ToString("0.#")).Append('\n');

        if (d.weapons != null && d.weapons.Length > 0)
        {
            Header("Зброя");
            foreach (WeaponData w in d.weapons)
            {
                if (!w) continue;
                sb.Append("<b>").Append(w.displayName).Append("</b>\n")
                  .Append("дальність ").Append(w.range.ToString("0"))
                  .Append(" | шкода ").Append(w.ammo ? w.ammo.damage.ToString("0.#") : "-")
                  .Append(" | ").Append(w.fireRate.ToString("0.##")).Append("/с\n");
            }
        }


        if (ship.Team == Team.Player && d.weapons != null)
        {
            Header("Набої");
            seenAmmo.Clear();
            foreach (WeaponData w in d.weapons)
            {
                if (!w || !w.ammo || seenAmmo.Contains(w.ammo)) continue;
                seenAmmo.Add(w.ammo);

                int n = PlayerStock.Instance.AmmoCount(w.ammo);
                sb.Append(w.ammo.displayName).Append(": ");
                if (n == 0) sb.Append("<color=#").Append(emptyHex).Append(">0</color>");
                else sb.Append(n);
                sb.Append('\n');
            }
        }

        bodyText.text = sb.ToString().TrimEnd('\n');
    }

    void Header(string text)
    {
        sb.Append("\n<color=#").Append(accentHex).Append("><b>").Append(text).Append("</b></color>\n");
    }
}