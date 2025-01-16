using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension 
{
    public static Transform FindDeepChild(this Transform parent, string targetName)
    {
        if( parent.name == targetName) return parent;

        foreach (Transform child in parent)
        {
            if(child.name == targetName) return child;
            
            var result = child.FindDeepChild(targetName);
            if (result != null) return result;
        }
        return null;
    }
}
