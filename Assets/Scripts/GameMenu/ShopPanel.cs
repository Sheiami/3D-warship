using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;
    [SerializeField] TMP_Text pointsLabel;
    [SerializeField] Transform listRoot;
    [SerializeField] ShopRow rowPrefab;
    [SerializeField] AmmoData[] catalog;

    readonly List<ShopRow> rows = new List<ShopRow>();
    PlayerStock stock;

    void Start()
    {
        stock = PlayerStock.Instance;
        stock.OnChanged += Refresh;

        foreach (AmmoData ammo in catalog)
        {
            ShopRow row = Instantiate(rowPrefab, listRoot);
            row.Bind(ammo);
            rows.Add(row);
        }

        openButton.onClick.AddListener(() => SetOpen(!window.activeSelf));
        closeButton.onClick.AddListener(() => SetOpen(false));
        SetOpen(false);
    }

    void OnDestroy()
    {
        if (stock != null) stock.OnChanged -= Refresh;
    }

    void SetOpen(bool open)
    {
        window.SetActive(open);
        if (open) Refresh();
    }

    void Refresh()
    {
        if (!window.activeSelf) return;

        pointsLabel.text = $"Очки: {stock.Points}";
        foreach (ShopRow row in rows) row.Refresh();
    }
}
