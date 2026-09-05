using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// Takes parsed YAML content data and creates/updates ScriptableObject .asset files.
    /// Creates required folders on demand.
    /// </summary>
    public sealed class ContentAssetFactory
    {
        // ---------------------------------------------------------------
        // Counts returned after an import batch
        // ---------------------------------------------------------------

        public int Created { get; private set; }
        public int Updated { get; private set; }
        public int Skipped { get; private set; }

        // ---------------------------------------------------------------
        // Card definitions
        // ---------------------------------------------------------------

        /// <summary>
        /// Import a card definition. Looks for existing .asset by CardID in
        /// Assets/Settings/Cards/. Creates a new CardDefinition if not found.
        /// </summary>
        public void ImportCard(CardEntry data)
        {
            string folder = "Assets/Settings/Cards";
            EnsureFolder(folder);

            // Look for existing asset by CardID
            string guid = FindAssetGUID<CardDefinition>(folder, data.CardId);
            CardDefinition card;

            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
                Updated++;
            }
            else
            {
                card = ScriptableObject.CreateInstance<CardDefinition>();
                string fileName = SanitizeFileName(data.CardId) + ".asset";
                string path = Path.Combine(folder, fileName);
                AssetDatabase.CreateAsset(card, path);
                Created++;
            }

            // Update fields
            SerializedObject so = new SerializedObject(card);
            so.FindProperty("CardID").stringValue = data.CardId;
            so.FindProperty("DisplayName").stringValue = data.DisplayName;
            so.FindProperty("Description").stringValue = data.Description;
            // Icon is a Texture2D reference — we skip setting from YAML (no path mapping)
            so.FindProperty("Icon").objectReferenceValue = null;

            // TargetActorIDs — List<ActorIdentifier>
            SerializedProperty targetList = so.FindProperty("TargetActorIDs");
            targetList.ClearArray();
            targetList.arraySize = data.TargetActorIds.Count;
            for (int i = 0; i < data.TargetActorIds.Count; i++)
            {
                SerializedProperty elem = targetList.GetArrayElementAtIndex(i);
                SerializedProperty actorIdProp = elem.FindPropertyRelative("_actorID");
                SerializedProperty displayNameProp = elem.FindPropertyRelative("_displayName");
                if (actorIdProp != null)
                    actorIdProp.stringValue = data.TargetActorIds[i];
                if (displayNameProp != null)
                    displayNameProp.stringValue = string.Empty;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
        }

        // ---------------------------------------------------------------
        // Dialogue entries
        // ---------------------------------------------------------------

        /// <summary>
        /// Import a dialogue entry. Creates a DialogueSettings .asset named
        /// Dialogue_{CardID}_{ActorID}.asset in Assets/Settings/Dialogue/.
        /// Updates existing asset if found.
        /// </summary>
        public void ImportDialogue(DialogueEntry data)
        {
            string path = "Assets/Settings/Dialogue.asset";

            DialogueSettings dialogue;

            if (File.Exists(Path.GetFullPath(path)))
            {
                dialogue = AssetDatabase.LoadAssetAtPath<DialogueSettings>(path);
            }
            else
            {
                dialogue = ScriptableObject.CreateInstance<DialogueSettings>();
                AssetDatabase.CreateAsset(dialogue, path);
                Created++;
            }

            if (dialogue == null)
            {
                Debug.LogError("[ContentAssetFactory] DialogueSettings is null after load/create — skipping.");
                Skipped++;
                return;
            }

            // Find or create the entry matching CardID + ActorID
            SerializedObject so = new SerializedObject(dialogue);
            SerializedProperty entriesProp = so.FindProperty("Entries");

            if (entriesProp == null)
            {
                Debug.LogError("[ContentAssetFactory] Could not find 'Entries' property on DialogueSettings.");
                Skipped++;
                return;
            }

            int existingIndex = -1;
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                SerializedProperty elem = entriesProp.GetArrayElementAtIndex(i);
                string cardId = elem.FindPropertyRelative("CardID")?.stringValue ?? string.Empty;
                string actorId = elem.FindPropertyRelative("ActorID")?.stringValue ?? string.Empty;
                if (cardId == data.CardId && actorId == data.ActorId)
                {
                    existingIndex = i;
                    break;
                }
            }

            SerializedProperty target;
            if (existingIndex >= 0)
            {
                target = entriesProp.GetArrayElementAtIndex(existingIndex);
                Updated++;
            }
            else
            {
                int newIdx = entriesProp.arraySize;
                entriesProp.arraySize = newIdx + 1;
                target = entriesProp.GetArrayElementAtIndex(newIdx);
                Created++;
            }

            target.FindPropertyRelative("CardID").stringValue = data.CardId;
            target.FindPropertyRelative("ActorID").stringValue = data.ActorId;
            target.FindPropertyRelative("ActorDisplayName").stringValue = data.ActorDisplayName;

            // Lines
            SerializedProperty linesProp = target.FindPropertyRelative("Lines");
            linesProp.ClearArray();
            linesProp.arraySize = data.Lines.Count;
            for (int i = 0; i < data.Lines.Count; i++)
            {
                SerializedProperty lineProp = linesProp.GetArrayElementAtIndex(i);
                lineProp.FindPropertyRelative("Line").stringValue = data.Lines[i].Text;
                lineProp.FindPropertyRelative("DisplayDuration").floatValue = data.Lines[i].Duration;
                lineProp.FindPropertyRelative("Expression").objectReferenceValue = null;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dialogue);
        }

        // ---------------------------------------------------------------
        // Chapter entries
        // ---------------------------------------------------------------

        /// <summary>
        /// Import a chapter entry. Loads or creates Assets/Settings/ChapterSettings.asset,
        /// then appends or replaces the entry matching (ActorID, Chapter).
        /// </summary>
        public void ImportChapter(ChapterEntry data)
        {
            if (data == null)
            {
                Debug.LogError("[ContentAssetFactory] ImportChapter received null data!");
                Skipped++;
                return;
            }

            string path = "Assets/Settings/ChapterSettings.asset";
            ChapterSettings chapters;

            if (File.Exists(Path.GetFullPath(path)))
            {
                chapters = AssetDatabase.LoadAssetAtPath<ChapterSettings>(path);
            }
            else
            {
                chapters = ScriptableObject.CreateInstance<ChapterSettings>();
                AssetDatabase.CreateAsset(chapters, path);
                Created++;
            }

            if (chapters == null)
            {
                Debug.LogError("[ContentAssetFactory] ChapterSettings is null after load/create — skipping.");
                Skipped++;
                return;
            }

            SerializedObject so = new SerializedObject(chapters);
            SerializedProperty entriesProp = so.FindProperty("Entries");

            if (entriesProp == null)
            {
                Debug.LogError("[ContentAssetFactory] Could not find 'Entries' property on ChapterSettings.");
                Skipped++;
                return;
            }

            // Look for existing entry with same ActorID + Chapter
            int existingIndex = -1;
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                SerializedProperty elem = entriesProp.GetArrayElementAtIndex(i);
                string actorId = elem.FindPropertyRelative("ActorID")?.stringValue ?? string.Empty;
                int chapterVal = elem.FindPropertyRelative("Chapter")?.intValue ?? -1;
                if (actorId == data.ActorId && chapterVal == data.Chapter)
                {
                    existingIndex = i;
                    break;
                }
            }

            SerializedProperty target;
            if (existingIndex >= 0)
            {
                target = entriesProp.GetArrayElementAtIndex(existingIndex);
                Updated++;
            }
            else
            {
                int newIdx = entriesProp.arraySize;
                entriesProp.arraySize = newIdx + 1;
                target = entriesProp.GetArrayElementAtIndex(newIdx);
                Created++;
            }

            target.FindPropertyRelative("ActorID").stringValue = data.ActorId;
            target.FindPropertyRelative("Chapter").intValue = data.Chapter;
            target.FindPropertyRelative("SpawnPointID").stringValue = data.SpawnPointId;
            target.FindPropertyRelative("IsVisible").boolValue = data.IsVisible;
            target.FindPropertyRelative("Anim").objectReferenceValue = null;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chapters);
        }

        // ---------------------------------------------------------------
        // Expression definitions
        // ---------------------------------------------------------------

        /// <summary>
        /// Import an expression definition. Creates an ExpressionDefinition .asset
        /// by id in Assets/Settings/Expressions/. Updates existing if found.
        /// </summary>
        public void ImportExpression(ExpressionEntry data)
        {
            string folder = "Assets/Settings/Expressions";
            EnsureFolder(folder);

            string fileName = SanitizeFileName(data.Id) + ".asset";
            string path = Path.Combine(folder, fileName);

            ExpressionDefinition expression;

            if (File.Exists(Path.GetFullPath(path)))
            {
                expression = AssetDatabase.LoadAssetAtPath<ExpressionDefinition>(path);
                Updated++;
            }
            else
            {
                expression = ScriptableObject.CreateInstance<ExpressionDefinition>();
                AssetDatabase.CreateAsset(expression, path);
                Created++;
            }

            SerializedObject so = new SerializedObject(expression);
            SerializedProperty morphTargetsProp = so.FindProperty("MorphTargets");
            morphTargetsProp.ClearArray();
            morphTargetsProp.arraySize = data.MorphTargets.Count;
            for (int i = 0; i < data.MorphTargets.Count; i++)
            {
                SerializedProperty elem = morphTargetsProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("name").stringValue = data.MorphTargets[i].Name;
                elem.FindPropertyRelative("value").floatValue = data.MorphTargets[i].Value;
                elem.FindPropertyRelative("blendInTime").floatValue = data.MorphTargets[i].BlendInTime;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(expression);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        /// <summary>Ensure a folder exists under Assets, creating parent folders as needed.</summary>
        private void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            // Split and create recursively
            string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
            string leaf = Path.GetFileName(folderPath);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        /// <summary>
        /// Search for an existing asset of type T under <paramref name="folder"/>
        /// whose CardID / property matches <paramref name="id"/>.
        /// Returns the asset GUID or null.
        /// </summary>
        private string FindAssetGUID<T>(string folder, string id) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    // Check if it's the one we're looking for by reading CardID via SerializedObject
                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty prop = so.FindProperty("CardID");
                    if (prop != null && prop.stringValue == id)
                        return guid;
                }
            }
            return null;
        }

        /// <summary>Replace invalid filename characters.</summary>
        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unnamed";
            char[] invalid = Path.GetInvalidFileNameChars();
            var sanitized = new System.Text.StringBuilder(name.Length);
            foreach (char c in name)
            {
                sanitized.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            }
            return sanitized.ToString();
        }
    }
}