using System;
using Database;
using UnityEngine;

namespace Code
{
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            GameDataController.Initialize();
        }
    }
}