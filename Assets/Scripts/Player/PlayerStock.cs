using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStock : MonoBehaviour
{
    static PlayerStock instance;

    public static PlayerStock Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject(nameof(PlayerStock)).AddComponent<PlayerStock>();
            return instance;
        }
    }

    [SerializeField] int startPoints = 100;
    [SerializeField] int pointsPerSecond = 3;

    readonly Dictionary<AmmoData, int> ammo = new Dictionary<AmmoData, int>();
    readonly HashSet<AmmoData> starterGranted = new HashSet<AmmoData>();
    float timer;

    public int Points { get; private set; }
    public event Action OnChanged;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Points = startPoints;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f) return;

        int ticks = (int)timer;
        timer -= ticks;
        Points += ticks * pointsPerSecond;
        OnChanged?.Invoke();
    }

    public void ResetState()
    {
        ammo.Clear();
        starterGranted.Clear();
        Points = startPoints;
        timer = 0f;
    }

    public int AmmoCount(AmmoData a) => ammo.TryGetValue(a, out int n) ? n : 0;

    public bool Buy(AmmoData a)
    {
        if (a == null || Points < a.price) return false;

        Points -= a.price;
        ammo[a] = AmmoCount(a) + a.packSize;
        OnChanged?.Invoke();
        return true;
    }

    public void GrantStarter(AmmoData a, int amount)
    {
        if (a == null || amount <= 0 || !starterGranted.Add(a)) return;

        ammo[a] = AmmoCount(a) + amount;
        OnChanged?.Invoke();
    }

    public bool TryConsume(AmmoData a)
    {
        int n = AmmoCount(a);
        if (n <= 0) return false;

        ammo[a] = n - 1;
        OnChanged?.Invoke();
        return true;
    }
}