using UnityEngine;
namespace YRA{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected bool isPersistant;
        public static T Instance { get; private set; }
        public virtual void Awake()
        {
            if (Instance == null) Instance = this as T;
            else Destroy(gameObject);

            if (isPersistant) DontDestroyOnLoad(this);
        }
    }
}