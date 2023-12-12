using UnityEngine;

namespace Model
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance { get; private set; }

        [SerializeField]
        private Model model;

        public Model CurrentModel => model;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
