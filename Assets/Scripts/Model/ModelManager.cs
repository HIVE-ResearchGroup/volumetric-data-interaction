using System.Threading.Tasks;
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

        public bool ModelExists(string nameToCheck)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == nameToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public void ChangeModel(string nameToCheck)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                gameObject.SetActive(false);
                if (transform.GetChild(i).name == nameToCheck)
                {
                    gameObject.SetActive(true);
                }
            }
        }

        public void ResetState()
        {
            // TODO
        }
    }
}
