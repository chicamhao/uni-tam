using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class NPC : MonoBehaviour, IPositionable, IInteractable
{
    [Header("Identity")]
    public string NPCID;
    public string DisplayName;

    [Header("References")]
    public Camera conversationCamera;
    public FacialExpressionSet expressionSet;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("Positioning")]
    private MaterialPropertyBlock mpb;
    private bool mpbInitialized;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Returns the head bone transform for conversation camera aiming.
    /// </summary>
    public Transform GetHeadTransform()
    {
        if (animator != null)
            return animator.GetBoneTransform(HumanBodyBones.Head);
        return null;
    }

    // --- IPositionable ---
    public string GetActorID() => NPCID;

    public void ApplyState(ChapterEntry state, Transform spawnPoint)
    {
        if (state.bVisible == false)
        {
            StartCoroutine(FadeOutAndTeleport(state, spawnPoint));
        }
        else
        {
            StartCoroutine(FadeOutTeleportFadeIn(state, spawnPoint));
        }
    }

    private IEnumerator FadeOutAndTeleport(ChapterEntry state, Transform spawnPoint)
    {
        // Fade out material alpha
        yield return FadeAlpha(1f, 0f, 0.5f);

        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (state.Anim != null)
        {
            var animator = GetComponent<Animator>();
            if (animator != null) animator.Play(state.Anim.name);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator FadeOutTeleportFadeIn(ChapterEntry state, Transform spawnPoint)
    {
        yield return FadeAlpha(1f, 0f, 0.5f);

        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (state.Anim != null)
        {
            var animator = GetComponent<Animator>();
            if (animator != null) animator.Play(state.Anim.name);
        }

        yield return FadeAlpha(0f, 1f, 0.5f);
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (skinnedMeshRenderer == null) yield break;

        GetMPB();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(from, to, t);
            // Use _BaseColor or _Color depending on shader
            Color c = skinnedMeshRenderer.sharedMaterial.color;
            c.a = alpha;
            mpb.SetColor("_BaseColor", c);
            // Also set _Color as fallback
            mpb.SetColor("_Color", c);
            skinnedMeshRenderer.SetPropertyBlock(mpb);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Color final = skinnedMeshRenderer.sharedMaterial.color;
        final.a = to;
        mpb.SetColor("_BaseColor", final);
        mpb.SetColor("_Color", final);
        skinnedMeshRenderer.SetPropertyBlock(mpb);
    }

    private void GetMPB()
    {
        if (!mpbInitialized)
        {
            mpb = new MaterialPropertyBlock();
            skinnedMeshRenderer.GetPropertyBlock(mpb);
            mpbInitialized = true;
        }
    }

    // --- IInteractable ---
    public void Interact()
    {
        // Open card selection for this NPC
        Dialogue.Instance?.OpenCardSelectionForNPC(this);
    }

    public string GetPrompt()
    {
        return $"Talk to {DisplayName}";
    }

    public void SetHighlight(bool highlighted)
    {
        GetMPB();
        mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
        skinnedMeshRenderer.SetPropertyBlock(mpb);
    }

    // --- Facial Expressions ---
    public void ApplyExpression(FacialExpression expr)
    {
        if (expressionSet == null || skinnedMeshRenderer == null) return;

        var morphs = expressionSet.GetMorphsForExpression(expr);
        if (morphs == null) return;

        // Reset all morphs not in the target expression to 0
        var allMorphs = expressionSet.GetMorphsForExpression(FacialExpression.Neutral);
        // Better approach: iterate blend shape names and set accordingly
        StartCoroutine(BlendToExpression(morphs));
    }

    private IEnumerator BlendToExpression(List<MorphTargetValue> morphTargets)
    {
        if (skinnedMeshRenderer == null) yield break;

        // First, zero out any morph targets not in this set
        int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
        HashSet<string> activeMorphs = new HashSet<string>();
        foreach (var mt in morphTargets)
            activeMorphs.Add(mt.name);

        for (int i = 0; i < blendShapeCount; i++)
        {
            string shapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
            if (!activeMorphs.Contains(shapeName))
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
            }
        }

        // Blend in each target morph
        foreach (var mt in morphTargets)
        {
            int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(mt.name);
            if (index < 0) continue;

            float startWeight = skinnedMeshRenderer.GetBlendShapeWeight(index);
            float targetWeight = mt.value * 100f;

            float elapsed = 0f;
            float duration = mt.blendInTime;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, targetWeight, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            skinnedMeshRenderer.SetBlendShapeWeight(index, targetWeight);
        }
    }
}