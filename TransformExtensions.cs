using UnityEngine;

namespace GamblingMachine
{
    /*
    Adds a new extension method to [gameObject].transform to find a GameObject among the children of a parent GameObject.

    Usage:
    [gameObject].transform.FindDeepInChild("{your_game_object_name}"); -> returns a Transform
    GameObject yourGameObject = [gameObject].transform.FindDeepInChild("{your_game_object_name}").gameObject;
    -> returns a GameObject
    */

    public static class TransformExtensions
    {
        public static Transform FindDeepInChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = child.FindDeepInChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
