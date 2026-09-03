using System.Collections.Generic;
using Assets.Scripts.Settings;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "ExpressionDefinition", menuName = "ScriptableObjects/ExpressionDefinition", order = 1)]
    public sealed class ExpressionDefinition : ScriptableObject
    {
        public List<MorphTargetValue> MorphTargets = new();
    }
}