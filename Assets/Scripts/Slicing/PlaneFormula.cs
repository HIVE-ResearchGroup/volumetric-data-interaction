using UnityEngine;

namespace Slicing
{
    /// <summary>
    /// https://www.geeksforgeeks.org/program-to-find-equation-of-a-plane-passing-through-3-points/
    /// </summary>
    public class PlaneFormula
    {
        private readonly float _a;
        private readonly float _b;
        private readonly float _c;
        private readonly float _d;

        public PlaneFormula(Vector3 one, Vector3 two, Vector3 three)
        {
            var a1 = two.x - one.x;
            var b1 = two.y - one.y;
            var c1 = two.z - one.z;
            
            var a2 = three.x - one.x;
            var b2 = three.y - one.y;
            var c2 = three.z - one.z;

            _a = b1 * c2 - b2 * c1;
            _b = a2 * c1 - a1 * c2;
            _c = a1 * b2 - b1 * a2;
            _d = -_a * one.x - _b * one.y - _c * one.z;
            
            Debug.Log("Plane formula = " + _a + "x + " + _b + "y + " + _c + "z + " + _d + " = 0");
        }

        public Vector3? GetValidXVectorOnPlane(float xCount, float y, float z)
        {
            var pointOnXAxis = GetXOnPlane(y, z);
            var isValid = pointOnXAxis < xCount && pointOnXAxis >= 0;
            return isValid ? new Vector3(pointOnXAxis, y, z) : null;
        }
        
        public Vector3? GetValidYVectorOnPlane(float x, float yCount, float z)
        {
            var pointOnYAxis = GetYOnPlane(x, z);
            var isValid = pointOnYAxis < yCount && pointOnYAxis >= 0;
            return isValid ? new Vector3(x, pointOnYAxis, z) : null;
        }        

        public Vector3? GetValidZVectorOnPlane(float x, float y, float zCount)
        {
            var pointOnZAxis = GetZOnPlane(x, y);
            var isValid = pointOnZAxis < zCount && pointOnZAxis >= 0;
            return isValid ? new Vector3(x, y, pointOnZAxis) : null;
        }
        
        private float GetXOnPlane(float y, float z) => -1 * ((_b * y + _c * z + _d) / _a);

        private float GetYOnPlane(float x, float z) => -1 * ((_a * x + _c * z + _d) / _b);

        private float GetZOnPlane(float x, float y) => -1 * ((_a * x + _b * y + _d) / _c);
    }
}