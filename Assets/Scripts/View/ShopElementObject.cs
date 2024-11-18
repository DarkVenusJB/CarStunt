using UnityEngine;

namespace Views
{
    public class ShopElementObject : MonoBehaviour
    {
        [SerializeField] private GameObject _lock;

        public void LockActive(bool value) => _lock.SetActive(value);
    }
}