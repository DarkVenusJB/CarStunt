using NaughtyAttributes;
using UnityEngine;

namespace Views
{
    [CreateAssetMenu(fileName = "new Car Configuration", menuName = "Game/Shop/Car", order = 0)]
    public class CarData : ShopElementData
    {
        [field: SerializeField, ShowAssetPreview] public CarController GamePrefab { get; private set; }
        
        private RCC_CarControllerV3 _rccCarController;

        public RCC_CarControllerV3 RccCarController
        {
            get
            {
                if (!_rccCarController) _rccCarController = GamePrefab.GetComponent<RCC_CarControllerV3>();
                return _rccCarController;
            }
        }
    }
}