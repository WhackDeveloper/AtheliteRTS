using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// A prefab asset preview visual element. Loads the asset and it's preview
    /// based on provided path.
    /// </summary>
    public class PrefabPreviewElement : VisualElement
    {
        private readonly string prefabPath;

        private Texture2D previewTexture;
        private GameObject prefab;

        private bool isPreviewLoaded = false;

        public PrefabPreviewElement(string path)
        {
            prefabPath = path;

            LoadPrefabAsync();
        }

        private void LoadPrefabAsync()
        {
            // Load the prefab asset from the path
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab at {prefabPath} could not be loaded.");
                return;
            }

            // Use EditorApplication.update for async loading
            EditorApplication.update += CheckPreviewLoaded;
        }

        private void CheckPreviewLoaded()
        {
            if (isPreviewLoaded) return;

            if (EditorApplication.isUpdating || AssetPreview.IsLoadingAssetPreviews())
                return;

            // Try to load the asset preview
            previewTexture = AssetPreview.GetAssetPreview(prefab);

            if (previewTexture != null)
            {
                isPreviewLoaded = true;
                EditorApplication.update -= CheckPreviewLoaded;
                UpdatePreviewElement();
            }
        }

        private void UpdatePreviewElement()
        {
            if (previewTexture != null)
            {
                style.backgroundImage = new StyleBackground(previewTexture);
            }
        }
    }

}