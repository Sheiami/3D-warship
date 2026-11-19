using UnityEngine;

[CreateAssetMenu(menuName = "Ships/Ammo Data", fileName = "Ammo_")]
public class AmmoData : ScriptableObject
{
    public string displayName;
    public GameObject projectilePrefab;
    public float damage = 10f;
    public float speed = 40f;
    [Min(0)] public int price = 10;
    [Min(1)] public int packSize = 10;
}