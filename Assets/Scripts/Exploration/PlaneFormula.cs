using System.Collections.Generic;
using UnityEngine;

namespace Exploration
{
    /// <summary>
    /// https://www.geeksforgeeks.org/program-to-find-equation-of-a-plane-passing-through-3-points/
    /// </summary>
    public class PlaneFormula
    {
        private float a;
        private float b;
        private float c;
        private float d;

        public PlaneFormula(IReadOnlyList<Vector3> planePoints)
        {
            var one = planePoints[0];
            var two = planePoints[1];
            var three = planePoints[2];
            var a1 = two.x - one.x;
            var b1 = two.y - one.y;
            var c1 = two.z - one.z;
            var a2 = three.x - one.x;
            var b2 = three.y - one.y;
            var c2 = three.z - one.z;

            a = b1 * c2 - b2 * c1;
            b = a2 * c1 - a1 * c2;
            c = a1 * b2 - b1 * a2;
            d = (-a * one.x - b * one.y - c * one.z);
            //Debug.Log("Plane formula = " + a + "x + " + b + "y + " + c + "z + " + d + " = 0");
        }

        public float GetXOnPlane(float y, float z)
        {
            var result = -1 * ((b * y + c * z + d) / a);
            return result;
        }

        public float GetYOnPlane(float x, float z)
        {
            var result = -1 * ((a * x + c * z + d) / b);
            return result;
        }

        public float GetZOnPlane(float x, float y)
        {
            var result = -1 * ((a * x + b * y + d) / c);
            return result;
        }

        public Vector3? GetValidXVectorOnPlane(float xCount, float y, float z)
        {
            var pointOnXAxis = GetXOnPlane(y, z);
            var isValid = pointOnXAxis < xCount && pointOnXAxis >= 0;
            return isValid ? new Vector3(pointOnXAxis, y, z) : (Vector3?)null;
        }
        
        public Vector3? GetValidYVectorOnPlane(float yCount, float x, float z)
        {
            var pointOnYAxis = GetYOnPlane(x, z);
            var isValid = pointOnYAxis < yCount && pointOnYAxis >= 0;
            return isValid ? new Vector3(x, pointOnYAxis, z) : (Vector3?)null;
        }        

        public Vector3? GetValidZVectorOnPlane(float zCount, float x, float y)
        {
            var pointOnZAxis = GetZOnPlane(x, y);
            var isValid = pointOnZAxis < zCount && pointOnZAxis >= 0;
            return isValid ? new Vector3(x, y, pointOnZAxis) : (Vector3?)null;
        }
    }
}