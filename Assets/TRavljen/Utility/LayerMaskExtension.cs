using UnityEngine;

public static class LayerMaskExtension
{

    /// <summary>
    /// Checks if the layer mask A contains layer mask B.
    /// </summary>
    /// <param name="maskA">Owned mask</param>
    /// <param name="maskB">Containing mask</param>
    /// <returns></returns>
    public static bool ContainsLayer(this LayerMask maskA, LayerMask maskB)
    {
        return ((1 << maskB) & maskA.value) != 0;
    }

}