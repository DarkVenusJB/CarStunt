using UnityEngine;

namespace Components
{
    public class PlayerRigidbody : MonoBehaviour
    {
        [field: SerializeField] public Rigidbody Rigidbody;
        [field: SerializeField] public CharacterController CharacterController;
    }
}