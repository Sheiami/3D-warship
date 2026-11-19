using UnityEngine;

[CreateAssetMenu(menuName = "Ships/Weapon Data", fileName = "Weapon_")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public AmmoData ammo;
    public float range = 50f;
    public float fireRate = 1f;
}