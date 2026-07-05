using System;
using UnityEngine;

[Serializable]
public struct AnimatorStateReference
{
#if UNITY_EDITOR
    [SerializeField] private UnityEditor.Animations.AnimatorController m_Controller;
#endif
    [SerializeField] private string m_StateName;
    [SerializeField, HideInInspector] private int m_LayerID;
    
    public string StateName => m_StateName;
    public int LayerID => string.IsNullOrEmpty(m_StateName) ? -1 : m_LayerID;
    
    public static implicit operator string(AnimatorStateReference a_StateReference) => a_StateReference.m_StateName;

    public override string ToString() => m_StateName ?? string.Empty;
}
