using UnityEngine;

namespace GamblingMachine
{
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
