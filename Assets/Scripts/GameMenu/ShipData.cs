using UnityEngine;

[CreateAssetMenu(menuName = "Ships/Ship Data", fileName = "Ship_")]
public class ShipData : ScriptableObject
{
    public string displayName;
    public GameObject visualPrefab;
    public float maxHP = 100f;
    public float speed = 5f;
    public float turnSpeed = 60f;
    public WeaponData[] weapons;
}
