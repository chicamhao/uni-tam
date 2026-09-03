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
        public List<Vector3> waypoints = new List<Vector3>();

        [Header("Footprint Settings")]
        public Material footprintMaterial;
        public float stepSpacing = 0.3f;
        public float stepWidth = 0.2f;
        public float revealDelay = 0.15f;
        public float footprintSize = 0.25f;

        [Header("Runtime")]
        public bool spawnOnStart = true;

        public System.Action OnFootprintsComplete;

        private void Start()
        {
            if (spawnOnStart)
                StartCoroutine(SpawnFootprints());
        }

        [ContextMenu("Spawn Footprints")]
        public void StartSpawning()
        {
            StartCoroutine(SpawnFootprints());
        }

        private IEnumerator SpawnFootprints()
        {
            if (waypoints.Count < 2) yield break;

            float totalLength = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
                totalLength += Vector3.Distance(waypoints[i], waypoints[i + 1]);

            float distance = 0f;
            bool leftFoot = false;

            while (distance < totalLength)
            {
                Vector3 position = GetPositionOnPath(distance);
                Vector3 tangent = GetTangentOnPath(distance);

                Vector3 right = Vector3.Cross(tangent, Vector3.up).normalized;
                Vector3 footPos = position + (leftFoot ? -right : right) * stepWidth;
                footPos.y = GetGroundHeight(footPos);

                SpawnFootprintDecal(footPos, tangent);

                leftFoot = !leftFoot;
                distance += stepSpacing;

                yield return new WaitForSeconds(revealDelay);
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
            decal.material = footprintMaterial;
            decal.size = new Vector3(footprintSize, footprintSize, 0.01f);
            decal.pivot = new Vector3(0, 0, 0);
            decal.startAngleFade = 0f;
            decal.endAngleFade = 0f;

            Destroy(decalObj, 60f);
        }

        private Vector3 GetPositionOnPath(float distance)
        {
            float accumulated = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                float segLen = Vector3.Distance(waypoints[i], waypoints[i + 1]);
                if (accumulated + segLen >= distance)
                {
                    float t = (distance - accumulated) / segLen;
                    return Vector3.Lerp(waypoints[i], waypoints[i + 1], t);
                }
                accumulated += segLen;
            }
            return waypoints[waypoints.Count - 1];
        }

        private Vector3 GetTangentOnPath(float distance)
        {
            float accumulated = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                float segLen = Vector3.Distance(waypoints[i], waypoints[i + 1]);
                if (accumulated + segLen >= distance || i == waypoints.Count - 2)
                    return (waypoints[i + 1] - waypoints[i]).normalized;
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
            if (waypoints.Count < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                Gizmos.DrawSphere(waypoints[i], 0.1f);
            }
            Gizmos.DrawSphere(waypoints[waypoints.Count - 1], 0.1f);
        }
    }
}