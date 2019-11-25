// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/#post-2904879
using UnityEngine;

public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public NamedArrayAttribute(string[] names) { this.names = names; }
}