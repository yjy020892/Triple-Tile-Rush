using System;
using UnityEngine;

public sealed class SortingWallet
{
    private const string CoinKey = "SortingPuzzle_Coin";

    private int coin;
    public int Coin => coin;

    public event Action<int> OnCoinChanged;

    public SortingWallet()
    {
        coin = Mathf.Max(0, PlayerPrefs.GetInt(CoinKey, 0));
    }

    public void SetCoin(int value)
    {
        coin = Mathf.Max(0, value);
        Save();
        OnCoinChanged?.Invoke(coin);
    }

    public void Add(int amount)
    {
        if (amount == 0) return;
        coin = Mathf.Max(0, coin + amount);
        Save();
        OnCoinChanged?.Invoke(coin);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (coin < amount) return false;
        coin -= amount;
        Save();
        OnCoinChanged?.Invoke(coin);
        return true;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CoinKey, coin);
        PlayerPrefs.Save();
    }
}
