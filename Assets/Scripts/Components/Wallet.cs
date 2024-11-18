using System;
using Database;
using TMPro;
using UnityEngine;
using Views;

namespace Components
{
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dollarsTMP;
        
        private void Start()
        {
            GameDataController.DollarsUpdated += DollarsUpdate;
            DollarsUpdate(GameDataController.Dollars);
        }

        private void OnDestroy() => GameDataController.DollarsUpdated -= DollarsUpdate;

        private void DollarsUpdate(int dollars) => _dollarsTMP.text = dollars.ToString();
    }
}