#nullable enable

using System;
using UnityEngine;

namespace Model
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance { get; private set; } = null!;

        public Model CurrentModel { get; private set; } = null!;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                CurrentModel = GetActiveModel() ?? throw new NullReferenceException("No active Model found!");
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

        private Model? GetActiveModel()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                if (child.activeSelf)
                {
                    return child.GetComponent<Model>();
                }
            }

            return null;
        }
    }
}
