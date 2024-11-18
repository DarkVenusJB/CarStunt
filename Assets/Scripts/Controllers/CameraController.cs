using System;
using System.Linq;
using KimicuUtility;
using UnityEditor;
using UnityEngine;

namespace Code
{
    public class CameraController : ComponentController
    {
        [SerializeField] private CameraData _cameraData;

        public static CameraController Instance { get; private set; }
        
        private void Awake() => Instance = this;

        public override void SetActiveComponents(params string[] names)
        {
            foreach (var cameraPair in _cameraData)
            {
                cameraPair.Value.gameObject.SetActive(names.Contains(cameraPair.Key));
            }
        }
    }
}

[Serializable]
public class CameraData : SerializableDictionary<string, RCC_Camera> { }
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CameraData))]
internal class CameraDataDrawer : DictionaryDrawer<string, RCC_Camera> { }
#endif