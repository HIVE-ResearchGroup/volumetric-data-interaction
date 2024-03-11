using System;
using UnityEngine;

namespace Networking.screenExtension
{
    [Serializable]
    public struct Screen
    {
        public int id;
        public Transform transform;
    }
}