using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TJ.Scripts;
using TMPro;

namespace Assets.TJ.Scripts
{
    public class CoinsManager : Singleton<CoinsManager>
    {
        [SerializeField] TextMeshProUGUI coinTxt;

        private int totalCoins;

        private void Start()
        {
            UpdateCoinTxt();
        }
        public int GetTotalCoins()
        {
            return PlayerPrefs.GetInt(PlayerPrefsManager.TotalCoins, 0);
        }

        public void AddCoins(int amount)
        {
            int coins = GetTotalCoins();
            coins += amount;
            PlayerPrefs.SetInt(PlayerPrefsManager.TotalCoins, coins);
            UpdateCoinTxt();
        }
        public void DeductCoins(int amount)
        {
            int coins = GetTotalCoins();
            coins -= amount;
            PlayerPrefs.SetInt(PlayerPrefsManager.TotalCoins, coins);
            UpdateCoinTxt();
        }
        public void UpdateCoinTxt()
        {
            totalCoins = GetTotalCoins();
            coinTxt.text = totalCoins.ToString();
        }
    }
}