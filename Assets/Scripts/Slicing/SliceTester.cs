#nullable enable

using Model;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Slicing
{
    public class SliceTester : MonoBehaviour
    {
        [SerializeField]
        private Transform slicer = null!;

        private Material _mat = null!;
        
        private MeshRenderer _meshRenderer = null!;

        private IEnumerator _coroutine = null!;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _mat = MaterialTools.CreateTransparentMaterial();
            _meshRenderer.material = _mat;
        }

        private void OnEnable()
        {
            var routine = UpdateTextures();
            _coroutine = routine;
            StartCoroutine(routine);
        }

        private void OnDisable()
        {
            StopCoroutine(_coroutine);
        }

        private IEnumerator UpdateTextures()
        {
            while (true)
            {
                var model = ModelManager.Instance.CurrentModel;
                slicer.GetPositionAndRotation(out var position, out var rotation);
                var localPosition = model.transform.InverseTransformPoint(position);
                var localRotation = model.transform.InverseTransformVector(rotation * Vector3.back);
                Dimensions? dimensions = null;
                Color32[]? texData = null;

                var task = Task.Run(() =>
                {
                    var points = Slicer.GetIntersectionPointsFromLocal(model, localPosition, localRotation);
                    if (points == null)
                    {
                        return;
                    }

                    dimensions = Slicer.GetTextureDimension(model, points);
                    if (dimensions == null)
                    {
                        return;
                    }

                    texData = Slicer.CreateSliceTextureData(model, dimensions, points);
                });
                
                yield return new WaitUntil(() => task.IsCompleted);

                if (texData != null && dimensions != null)
                {
                    var texture = Slicer.CreateSliceTexture(dimensions, texData);
                    var oldTexture = _mat.mainTexture;
                    _mat.mainTexture = texture;
                    Destroy(oldTexture);
                }
            }
        }
    }
}
