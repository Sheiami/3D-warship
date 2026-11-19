using UnityEngine;

public class WeaponHotkeys : MonoBehaviour
{
    static readonly KeyCode[] Keys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
    };

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        SelectionManager sm = SelectionManager.Instance;
        if (sm == null) return;

        Ship selected = sm.Selected;
        if (selected == null) return;

        for (int i = 0; i < Keys.Length; i++)
        {
            if (!Input.GetKeyDown(Keys[i])) continue;

            ShipCombat combat = selected.Combat;
            if (combat.SelectWeapon(i))
            {
                WeaponData weapon = combat.CurrentWeapon;
                int count = weapon.ammo != null ? PlayerStock.Instance.AmmoCount(weapon.ammo) : 0;
                CombatMessage.Show($"[{i + 1}] {weapon.displayName} | набоїв: {count}");
            }
            else
            {
                CombatMessage.Show($"У цього корабля немає зброї №{i + 1}");
            }

            return;
        }
    }
}
