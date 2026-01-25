// ============================================================================
// DictionaryExtensions.cs
// Place in: Assets/Editor/DictionaryExtensions.cs
// Unity Editor Window for level generation
// ============================================================================

#if UNITY_EDITOR
// Extension for .ToArray() support
public static class DictionaryExtensions
{
    public static TValue[] ToArray<TKey, TValue>(this System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection values)
    {
        var result = new TValue[values.Count];
        values.CopyTo(result, 0);
        return result;
    }
}

#endif
#if UNITY_EDITOR
#endif