using System;
using UnityEngine;

namespace Views
{
    public class CarControllerUI : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

        public static CarControllerUI Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}