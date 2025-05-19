using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.UIElements;

    public interface IUnitSystemDetailsViewListener
    {
        public void CreateNewPrefab(AEntitySO template);
        public void UpdateAssetPrefab(ScriptableObject asset);
        public void RemoveAssetPrefab(ScriptableObject asset);
        public void OnNameUpdated();
    }

    public class UnitSystemDetailsView : VisualElement
    {

        public IUnitSystemDetailsViewListener listener;

        public void DisplaySelectedAssetObject(ScriptableObject asset)
        {
            Clear();

            if (asset != null)
            {
                AddRightPaneHeader(asset);

                var scrollView = new ScrollView()
                {
                    verticalScrollerVisibility = ScrollerVisibility.Auto
                };

                Editor editor = Editor.CreateEditor(asset);
                var container = new IMGUIContainer(() => editor.OnInspectorGUI());
                container.style.paddingLeft = 16;
                container.style.paddingRight = 16;

                scrollView.contentContainer.Add(container);
                Add(scrollView);
            }
            else
            {
                Label label = new Label("No asset is selected.");
                label.style.fontSize = 14;
                label.style.flexGrow = 1;
                label.style.alignContent = Align.Center;
                label.style.paddingLeft = 20;
                label.style.paddingTop = 20;

                Add(label);
            }
        }

        private void AddRightPaneHeader(ScriptableObject asset)
        {
            var mainContainer = new VisualElement();
            mainContainer.style.flexShrink = 0;
            mainContainer.style.flexDirection = FlexDirection.Row;

            var assetAndPrefabDataContainer = new VisualElement();

            assetAndPrefabDataContainer.style.flexGrow = 1;

            var fileName = new TextField()
            {
                style =
                    {
                        fontSize = 15,
                        unityFontStyleAndWeight= FontStyle.Bold,
                        flexGrow = 1,
                        alignContent = Align.FlexStart,
                    }
            };

            fileName.value = asset.name;

            var updateNameButton = new Button(() =>
            {
                string newName = fileName.value.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != asset.name)
                {
                    string assetPath = AssetDatabase.GetAssetPath(asset);

                    // Rename the asset
                    AssetDatabase.RenameAsset(assetPath, newName);
                    AssetDatabase.SaveAssets();

                    listener.OnNameUpdated();
                    Debug.Log($"Renamed asset to: {newName}");
                }
            })
            {
                text = "Update File Name",
            };

            var horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.marginTop = 4;
            horizontalContainer.style.height = 22;
            horizontalContainer.style.flexShrink = 0;

            horizontalContainer.Add(updateNameButton);
            horizontalContainer.Add(fileName);
            assetAndPrefabDataContainer.Add(horizontalContainer);

            if (asset is IProvidesEntityPrefab providesPrefab &&
                asset is AEntitySO entity)
            {
                horizontalContainer = new VisualElement();
                horizontalContainer.style.flexDirection = FlexDirection.Row;
                horizontalContainer.style.height = 28;
                horizontalContainer.style.flexShrink = 0;
                horizontalContainer.style.paddingBottom = 4;
                horizontalContainer.style.paddingTop = 4;

                var prefabAsset = providesPrefab.GetAssociatedPrefab();

                if (!providesPrefab.IsPrefabSet)
                {
                    horizontalContainer.Add(new Button(() =>
                    {
                        listener.CreateNewPrefab(entity);
                    })
                    {
                        text = "Create Prefab",
                        style = { width = 150 }
                    });
                }
                else
                {
                    horizontalContainer.Add(new Button(() =>
                    {
                        string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
                        PrefabStageUtility.OpenPrefab(prefabPath);
                    })
                    {
                        text = "Open Prefab"
                    });

                    horizontalContainer.Add(new Button(() =>
                    {
                        listener.UpdateAssetPrefab(asset);
                    })
                    {
                        text = "Update Prefab"
                    });

                    horizontalContainer.Add(new Button(() =>
                    {
                        listener.RemoveAssetPrefab(asset);
                    })
                    {
                        text = "Remove Prefab"
                    });
                }

                assetAndPrefabDataContainer.Add(horizontalContainer);

                if (prefabAsset != null)
                {
                    var previewContainer = new VisualElement();
                    previewContainer.style.flexShrink = 1;
                    previewContainer.style.height = 100;
                    previewContainer.style.alignContent = Align.Center;

                    string path = AssetDatabase.GetAssetPath(prefabAsset.GetInstanceID());
                    var preview = new PrefabPreviewElement(path)
                    {
                        style = {
                            width = 100,
                            height = 100,
                            flexGrow = 0,
                            flexShrink = 1
                        }
                    };

                    previewContainer.Add(preview);
                    previewContainer.style.paddingRight = 16;
                    previewContainer.style.paddingBottom = 16;
                    mainContainer.Add(previewContainer);
                }
            }

            mainContainer.Add(assetAndPrefabDataContainer);
            Add(mainContainer);
        }
    }

}