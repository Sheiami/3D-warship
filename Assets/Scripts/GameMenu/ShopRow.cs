using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopRow : MonoBehaviour
{
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text stats;
    [SerializeField] Button buyButton;

    AmmoData ammo;

    public void Bind(AmmoData data)
    {
        ammo = data;
        title.text = data.displayName;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => PlayerStock.Instance.Buy(ammo));
        Refresh();
    }

    public void Refresh()
    {
        PlayerStock stock = PlayerStock.Instance;
        stats.text = $"x{ammo.packSize} | {ammo.price} очок | в запасі: {stock.AmmoCount(ammo)}";
        buyButton.interactable = stock.Points >= ammo.price;
    }
}
