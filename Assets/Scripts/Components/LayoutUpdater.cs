using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    public class LayoutUpdater : MonoBehaviour
    {
        [SerializeField] private LayoutGroup _layout;

        private RectTransform _rectTransform;
        private IEnumerator _routine;

        private RectTransform Layout
        {
            get
            {
                _rectTransform ??= _layout.GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private void OnValidate() => _layout ??= GetComponent<LayoutGroup>();
        private void Awake() => _routine = UpdateRoutine();
        private void OnEnable() => StartCoroutine(_routine);
        private void OnDisable() => StopCoroutine(_routine);

        private IEnumerator UpdateRoutine()
        {
            while (true)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Layout);
                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }
}