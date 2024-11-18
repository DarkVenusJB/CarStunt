using System;
using UnityEngine;
using UnityEngine.Events;

namespace Views.Managers
{
    public class ManagerInitializer : MonoBehaviour
    {
        public UnityEvent OnEnabled = new();

        private void OnEnable()
        {
            OnEnabled?.Invoke();
        }
    }
}