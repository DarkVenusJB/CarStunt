using NaughtyAttributes;
using UnityEngine;

namespace Levels.Data
{
    [CreateAssetMenu(fileName = "new Level", menuName = "Game/Level", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        public string Id;
        [ShowAssetPreview] public GameObject LevelPrefab;
        [ShowAssetPreview] public Sprite Preview;
        [ShowAssetPreview] public Sprite LockPreview;
        public int Reward;
    }
}