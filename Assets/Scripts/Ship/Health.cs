using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float MaxHP { get; private set; } = 100f;
    public float CurrentHP { get; private set; } = 100f;
    public bool IsDead => CurrentHP <= 0f;

    public event Action<float, float> OnChanged;
    public event Action OnDeath;

    public void Init(float maxHP)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
        OnChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        OnChanged?.Invoke(CurrentHP, MaxHP);

        if (IsDead) OnDeath?.Invoke();
    }
}
