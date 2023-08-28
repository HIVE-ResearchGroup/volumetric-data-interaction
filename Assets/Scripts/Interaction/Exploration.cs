using Constants;
using Exploration;
using Helper;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Interactions class has to be attached to gameobject holding the tracked device
    /// </summary>
    public class Exploration : MonoBehaviour
    {
        public GameObject tracker;

        [SerializeField]
        private GameObject host;
        [SerializeField]
        private GameObject cuttingPlane;
        [SerializeField]
        private GameObject modelPrefab;

        /// <summary>
        /// e.g. 5 cm before HHD
        /// Do not allow multiple models!
        /// </summary>
        public GameObject CreateModel()
        {       
            var currTrackingPosition = tracker.transform.position;
            currTrackingPosition.z += 5;

            return CreateModel(currTrackingPosition, Quaternion.identity);
        }

        private GameObject CreateModel(Vector3 currPosition, Quaternion rotation)
        {
            var currModel = ModelFinder.FindModelGameObject();
            if (currModel)
            {
                DeleteModel(currModel);
            }

            Debug.Log($"** Create model with name {modelPrefab.name}.");
            var newModel = Instantiate(modelPrefab, currPosition, rotation);
            if (!cuttingPlane.TryGetComponent(out Slicer slicer))
            {
                slicer = cuttingPlane.AddComponent<Slicer>();
            }
            slicer.RegisterListener(newModel.GetComponent<CollisionListener>());
            newModel.name = StringConstants.ModelName;
            return newModel;
        }

        private void DeleteModel(GameObject currModel)
        {
            if (!currModel)
            {
                return;
            }

            if (host.TryGetComponent(out SnapshotInteraction si))
            {
                si.DeleteAllSnapshots();
            }

            var modelName = currModel.name;
            Destroy(currModel);
            Debug.Log($"** Model with name {modelName} destroyed.");
        }

        public GameObject ResetModel()
        {
            var currModel = ModelFinder.FindModelGameObject();
            if (!currModel)
            {
                Debug.Log("** There is no model to be reset!");
                return currModel;
            }

            var position = currModel.transform.position;
            var rotation = currModel.transform.rotation;
            var currPosition = new Vector3(position.x, position.y, position.z);
            var currRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            return CreateModel(currPosition, currRotation);
        }
    }
}
