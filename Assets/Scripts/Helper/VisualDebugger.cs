using UnityEngine;

namespace Assets.Scripts.Helper
{
    public static class VisualDebugger
    {
        public static GameObject CreateDebugPrimitive(Vector3 position, Color colour, float size = 0.01f, string name = "primitive")
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.transform.position = position;
            primitive.transform.localScale = new Vector3(size, size, size);
            primitive.GetComponent<MeshRenderer>().material.color = colour;
            primitive.name = name;
            return primitive;
        }
    }
}
