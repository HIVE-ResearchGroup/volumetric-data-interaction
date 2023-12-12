using Constants;
using UnityEngine;

namespace Extensions
{
    public static class GameObjectExtensions
    {
        public static bool IsClone(this GameObject go) => go.name.EndsWith(StringConstants.Clone);
    }
}