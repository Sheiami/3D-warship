using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlsHelp : MonoBehaviour
{
    [Serializable]
    public class Section
    {
        public string title;
        [TextArea(2, 10)] public string lines;
    }

    [SerializeField] Button toggleButton;
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text content;
    [SerializeField]
    Section[] sections =
    {
        new Section
        {
            title = "Камера",
            lines = "WASD — рух\nQE - Вверх/Низ\nShift - Прискорення\nMouseWheel - Швидкість"
        },
        new Section
        {
            title = "Кораблі",
            lines = "Shift+ЛКМ по кораблю — вибір\nShift+ЛКМ у порожнє — зняти вибір\nЛКМ по воді — рух"
        },
        new Section
        {
            title = "Бій",
            lines = "ЛКМ по ворогу — атака\nЛКМ по воді — скасувати атаку\n1, 2 — вибір зброї"
        },
        new Section
        {
            title = "Набої та туман",
            lines = "Постріл = 1 набій зі складу\nНабої — в магазині ($)\nВорога видно лише поблизу\nсвого корабля"
        }
    };

    void Awake()
    {
        toggleButton.navigation = new Navigation { mode = Navigation.Mode.None };
        toggleButton.onClick.AddListener(Toggle);
        Rebuild();
        panel.SetActive(false);
    }

    void Toggle()
    {
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (open)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);

        EventSystem.current.SetSelectedGameObject(null);
    }

    void Rebuild()
    {
        var sb = new StringBuilder();
        foreach (var s in sections)
        {
            if (sb.Length > 0) sb.Append("\n\n");
            sb.Append("<b><color=#D4A95F>").Append(s.title).Append("</color></b>\n").Append(s.lines);
        }
        content.text = sb.ToString();
    }
}
