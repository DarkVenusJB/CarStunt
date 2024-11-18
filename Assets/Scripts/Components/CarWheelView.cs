using System;
using UnityEngine;

namespace Components
{
    public class CarWheelView : MonoBehaviour
    {
        [SerializeField] private WheelCollider _wheelCollider;

        private void Update()
        {
            _wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}