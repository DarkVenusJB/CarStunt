using UnityEngine;

namespace Code
{
    public abstract class ComponentController : MonoBehaviour
    {
        public abstract void SetActiveComponents(params string[] names);
    }
}