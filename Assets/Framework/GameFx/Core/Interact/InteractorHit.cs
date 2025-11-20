using UnityEngine;

namespace GameFx.Core.Interact
{
    public struct InteractorHit
    {
        public GameObject gameObject;
        public Transform transform;
        public Vector3 worldPoint;
        public Vector3 worldNormal;
        public RaycastHit rawHit;

        public T Get<T>() where T : class => gameObject ? gameObject.GetComponent<T>() : null;
    }

}