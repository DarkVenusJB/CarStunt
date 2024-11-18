using NaughtyAttributes;
using UnityEngine;

namespace Views
{
    public abstract class ShopElementData : ScriptableObject
    {
        [field: SerializeField] public bool IsIgnoredInGame { get; private set; }
        
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField, ShowAssetPreview] public ShopElementObject PrefabPreview { get; private set; }
        [field: SerializeField, HideIf(nameof(IsFree))] public CurrencyType CurrencyType { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField, ShowAssetPreview] public Sprite Icon { get; private set; }

        private bool IsFree => Price == 0;
    }

    public enum CurrencyType
    {
        Dollars,
        Video,
    }
}