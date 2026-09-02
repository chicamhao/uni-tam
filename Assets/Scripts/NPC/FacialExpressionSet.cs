using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacialExpressionSet", menuName = "ScriptableObjects/FacialExpressionSet", order = 1)]
public sealed class FacialExpressionSet : ScriptableObject
{
    [System.Serializable]
    public struct ExpressionMorphs
    {
        public FacialExpression Expression;
        public List<MorphTargetValue> MorphTargets;
    }

    public List<ExpressionMorphs> Expressions = new List<ExpressionMorphs>();

    public List<MorphTargetValue> GetMorphsForExpression(FacialExpression expression)
    {
        foreach (var entry in Expressions)
        {
            if (entry.Expression == expression)
                return entry.MorphTargets;
        }
        return null;
    }
}