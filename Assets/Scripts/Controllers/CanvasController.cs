using System;
using System.Linq;
using KimicuUtility;
using UnityEditor;
using UnityEngine;

namespace Code
{
    public class CanvasController : ComponentController
    {
        [SerializeField] private CanvasData _canvasData;

        public static CanvasController Instance { get; private set; }
        
        private void Awake() => Instance = this;
        
        public override void SetActiveComponents(params string[] names)
        {
            foreach (var canvasPair in _canvasData)
            {
                canvasPair.Value.gameObject.SetActive(names.Contains(canvasPair.Key));
            }
        }
    }
}

[Serializable]
public class CanvasData : SerializableDictionary<string, Canvas> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CanvasData))]
internal class CanvasDataDrawer : DictionaryDrawer<string, Canvas> { }
#endif