using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Spawns footprint decals along a spline path, alternating left/right.
    /// </summary>
    public sealed class FootprintSpawner : MonoBehaviour
    {
        [Header("Path")]
        public List<Vector3> Waypoints = new List<Vector3>();

        [Header("Footprint Settings")]
        public Material FootprintMaterial;
        public float StepSpacing = 0.3f;
        public float StepWidth = 0.2f;
        public float RevealDelay = 0.15f;
        public float FootprintSize = 0.25f;

        [Header("Runtime")]
        public bool SpawnOnStart = true;

        public System.Action OnFootprintsComplete;

        private void Start()
        {
            if (SpawnOnStart)
                StartCoroutine(SpawnFootprints());
        }

        [ContextMenu("Spawn Footprints")]
        public void StartSpawning()
        {
            StartCoroutine(SpawnFootprints());
        }

        private IEnumerator SpawnFootprints()
        {
            if (Waypoints.Count < 2) yield break;

            float totalLength = 0f;
            for (int i = 0; i < Waypoints.Count - 1; i++)
                totalLength += Vector3.Distance(Waypoints[i], Waypoints[i + 1]);

            float distance = 0f;
            bool leftFoot = false;

            while (distance < totalLength)
            {
                Vector3 position = GetPositionOnPath(distance);
                Vector3 tangent = GetTangentOnPath(distance);

                Vector3 right = Vector3.Cross(tangent, Vector3.up).normalized;
                Vector3 footPos = position + (leftFoot ? -right : right) * StepWidth;
                footPos.y = GetGroundHeight(footPos);

                SpawnFootprintDecal(footPos, tangent);

                leftFoot = !leftFoot;
                distance += StepSpacing;

                yield return new WaitForSeconds(RevealDelay);
            }

            OnFootprintsComplete?.Invoke();
        }

        private void SpawnFootprintDecal(Vector3 position, Vector3 forward)
        {
            GameObject decalObj = new GameObject("FootprintDecal");
            decalObj.transform.position = position;
            decalObj.transform.forward = forward;
            decalObj.transform.Rotate(Vector3.up, Random.Range(-5f, 5f));

            var decal = decalObj.AddComponent<DecalProjector>();
            decal.material = FootprintMaterial;
            decal.size = new Vector3(FootprintSize, FootprintSize, 0.01f);
            decal.pivot = new Vector3(0, 0, 0);
            decal.startAngleFade = 0f;
            decal.endAngleFade = 0f;

            Destroy(decalObj, 60f);
        }

        private Vector3 GetPositionOnPath(float distance)
        {
            float accumulated = 0f;
            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                float segLen = Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
                if (accumulated + segLen >= distance)
                {
                    float t = (distance - accumulated) / segLen;
                    return Vector3.Lerp(Waypoints[i], Waypoints[i + 1], t);
                }
                accumulated += segLen;
            }
            return Waypoints[Waypoints.Count - 1];
        }

        private Vector3 GetTangentOnPath(float distance)
        {
            float accumulated = 0f;
            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                float segLen = Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
                if (accumulated + segLen >= distance || i == Waypoints.Count - 2)
                    return (Waypoints[i + 1] - Waypoints[i]).normalized;
                accumulated += segLen;
            }
            return Vector3.forward;
        }

        private float GetGroundHeight(Vector3 pos)
        {
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
                return hit.point.y;
            return pos.y;
        }

        private void OnDrawGizmos()
        {
            if (Waypoints.Count < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(Waypoints[i], Waypoints[i + 1]);
                Gizmos.DrawSphere(Waypoints[i], 0.1f);
            }
            Gizmos.DrawSphere(Waypoints[Waypoints.Count - 1], 0.1f);
        }
    }
}