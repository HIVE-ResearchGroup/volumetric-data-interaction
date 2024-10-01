#nullable enable

using System;
using UnityEngine;

namespace Model
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance { get; private set; } = null!;

        [SerializeField]
        private GameObject cuttingPlane = null!;

        public Model CurrentModel { get; private set; } = null!;

        public GameObject CuttingPlane => cuttingPlane;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                var model = GetActiveModel();
                if (model == null)
                {
                    throw new NullReferenceException("No active Model found!");
                }
                CurrentModel = model;
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
            // reset the model beforehand, so manipulations are not replicated when switching back to the model
            CurrentModel.ResetState();

            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == nameToCheck)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
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
