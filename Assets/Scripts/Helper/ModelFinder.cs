using Constants;
using UnityEngine;

namespace Helper
{
    public static class ModelFinder
    {
        public static GameObject FindModelGameObject() => GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}{StringConstants.Clone}");
    }
}
