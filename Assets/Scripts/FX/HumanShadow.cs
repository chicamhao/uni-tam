using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Projects a human-shaped shadow using a decal projector.
    /// </summary>
    public sealed class HumanShadow : MonoBehaviour
    {
        public Material ShadowMaterial;
        public float ShadowSize = 2f;
        public float HeightOffset = 0.01f;

        private DecalProjector _decalProjector;

        private void Start()
        {
            GameObject decalObj = new GameObject("HumanShadowDecal");
            decalObj.transform.SetParent(transform, false);
            decalObj.transform.localPosition = new Vector3(0, HeightOffset, 0);
            decalObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);

            _decalProjector = decalObj.AddComponent<DecalProjector>();
            _decalProjector.material = ShadowMaterial;
            _decalProjector.size = new Vector3(ShadowSize, ShadowSize, 0.1f);
            _decalProjector.pivot = new Vector3(0, 0, 0);
            _decalProjector.startAngleFade = 0f;
            _decalProjector.endAngleFade = 0f;
        }
    }
}