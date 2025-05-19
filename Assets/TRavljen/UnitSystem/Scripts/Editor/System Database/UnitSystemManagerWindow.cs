namespace TRavljen.UnitSystem.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System;
    
    public class UnitSystemManagerWindow : EditorWindow, IUnitSystemInspectorViewListener, IUnitSystemDetailsViewListener
    {
        private const float padding = 8;

        private UnitSystemInspectorView inspectorPane;
        private UnitSystemDetailsView detailsPane;
    
        [SerializeField] private ScriptableObject selectedAsset;

        [MenuItem("Tools/TRavljen/UnitSystem Manager")]
        public static void ShowWindow()
        {
            UnitSystemManagerWindow wnd = GetWindow<UnitSystemManagerWindow>();
            wnd.titleContent = new GUIContent("UnitSystem Manager");
        }

        private void OnEnable()
        {
            SystemDatabase.OnDatabaseChange += OnDatabaseChange;
        }

        private void OnDisable()
        {
            SystemDatabase.OnDatabaseChange -= OnDatabaseChange;
        }

        private void OnDatabaseChange()
        {
            if (inspectorPane != null)
            {
                inspectorPane.Refresh();
                detailsPane?.DisplaySelectedAssetObject(inspectorPane.SelectedAsset);
            }
        }

        public void CreateGUI()
        {
            // Split View
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

            // Left Panel (Tree View with Search)
            inspectorPane = new(selectedAsset) { listener = this };
            inspectorPane.style.flexGrow = 1;
            inspectorPane.style.paddingTop = padding;

            splitView.Add(inspectorPane);

            // Right Panel (Details View)
            detailsPane = new() { listener = this };

            detailsPane.style.paddingRight = detailsPane.style.paddingLeft = detailsPane.style.paddingTop = padding;
            detailsPane.style.flexGrow = 1;
            splitView.Add(detailsPane);

            //RefreshLeftPane();
            inspectorPane.Refresh();

            if (inspectorPane.SelectedAsset == null && !inspectorPane.SetDefaultSelection())
            {
                // Initial call, to invoke placeholder.
                detailsPane.DisplaySelectedAssetObject(inspectorPane.SelectedAsset);
            }
        }

        private void DisplaySelectedAssetObject(ScriptableObject asset)
        {
            inspectorPane.SelectedAsset = asset;
            detailsPane.DisplaySelectedAssetObject(asset);
        }

        // Displays the inspector of the selected asset on the right pane
        private void DisplaySelectedAsset(int assetID)
        {
            ScriptableObject asset = SystemDatabase.GetInstance().FindAssetById(assetID);
            DisplaySelectedAssetObject(asset);
        }

        private void SaveCreatedAsset(string name, ScriptableObject assetInCreation)
        {
            if (assetInCreation == null)
            {
                Debug.LogError("Unexpected issue. No asset to create.");
                return;
            }

            string folder = UnitSystemConfig.GetOrCreate().GetAssetsFolderPath();
            AssetDatabaseHelper.CreateFolders(folder);

            AssetDatabase.CreateAsset(assetInCreation, $"{folder}/{name}.asset");

            AssetDatabase.SaveAssetIfDirty(assetInCreation);
            inspectorPane.SelectedAsset = assetInCreation;

            DisplaySelectedAssetObject(assetInCreation);
            inspectorPane.RefreshItemList();
        }

        #region IUnitSystemInspectorViewListener

        void IUnitSystemInspectorViewListener.AddNewAsset(Type type)
        {
            // Show the name input popup
            EnterAssetNamePopup.ShowPopup(assetName =>
            {
                CreateNewAsset(assetName, type);
            },
            fileName: type.Name);
        }

        void IUnitSystemInspectorViewListener.DuplicateSelectedAsset()
        {
            if (inspectorPane.SelectedAsset == null) return;
            var asset = inspectorPane.SelectedAsset;

            EnterAssetNamePopup.ShowPopup(assetName =>
            {
                if (asset == null) return;

                ScriptableObject duplicate = Instantiate(asset);

                // Register new duplicate to the database to provide a new ID
                if (duplicate is ManagedSO managed)
                {
                    managed.InvalidateID();
                    SystemDatabase database = SystemDatabase.GetInstance();
                    database.Register(managed);
                }

                DisplaySelectedAssetObject(duplicate);
                SaveCreatedAsset(assetName, duplicate);
                Debug.Log($"New asset created (duplicate): {assetName}");
            },
            asset.name + " copy");
        }

        void IUnitSystemInspectorViewListener.DeleteSelectedAsset()
        {
            var selectedAsset = inspectorPane.SelectedAsset;
            if (selectedAsset == null) return;

            ScriptableObject clear = null;

            string path = AssetDatabase.GetAssetPath(selectedAsset.GetInstanceID());

            bool hasPrefab = selectedAsset is IProvidesEntityPrefab providesPrefab &&
                providesPrefab.IsPrefabSet;

            void DeleteAsset()
            {
                AssetDatabase.DeleteAsset(path);
                inspectorPane.Refresh();
                DisplaySelectedAssetObject(clear);
            }

            if (hasPrefab)
            {
                int result = EditorUtility.DisplayDialogComplex("Delete Asset",
                    $"Are you sure you want to delete '{selectedAsset.name}' and its prefab?",
                    "Yes",
                    "No",
                    "Asset Only");

                if (result == 1) return;

                DeleteAsset();

                if (result == 0)
                {
                    RemoveAssetPrefab(selectedAsset, false);
                }
            }
            else
            {
                // Show a confirmation dialog
                bool deleteConfirmed = EditorUtility.DisplayDialog(
                    "Delete Asset",
                    $"Are you sure you want to delete '{selectedAsset.name}'?",
                    "Yes",
                    "No"
                );

                if (deleteConfirmed)
                    DeleteAsset();
            }
        }

        void IUnitSystemInspectorViewListener.ItemSelected(int assetID)
        {
            // Clear all previous content from the pane
            detailsPane.Clear();

            DisplaySelectedAsset(assetID);
        }

        #endregion

        #region IUnitSystemDetailsViewListener

        void IUnitSystemDetailsViewListener.UpdateAssetPrefab(ScriptableObject asset)
        {
            if (asset is not IProvidesEntityPrefab providesPrefab)
                throw new ArgumentException("Expected asset that implements IProvidesPrefab, but received: " + asset);

            // Configure prefab by adding or modifying required components.
            providesPrefab.ConfigurePrefab(providesPrefab.GetAssociatedPrefab().gameObject);

            // Save changes on the prefab reference itself.
            AssetDatabase.SaveAssets();

            Debug.Log($"Updated prefab for asset {asset.name}");
            DisplaySelectedAssetObject(asset);
        }

        void IUnitSystemDetailsViewListener.CreateNewPrefab(AEntitySO entity)
        {
            var config = UnitSystemConfig.GetOrCreate();
            var prefabsFolder = config.GetsPrefabsFolderPath();

            if (entity.Name == null || entity.Name.Length == 0)
            {
                Debug.LogError("Cannot create prefab as Entity does not have a name yet.");
                return;
            }

            // Creates new folders if needed
            AssetDatabaseHelper.CreateFolders(prefabsFolder);

            if (PrefabCreator.CreateAndSavePrefab(entity, prefabsFolder, entity.Name, out GameObject prefab) &&
                entity is IProvidesEntityPrefab providesPrefab)
            {
                providesPrefab.SetAssociatedPrefab(prefab.GetComponent<Entity>());

                // Reload UI
                DisplaySelectedAssetObject(entity);
            }
            else
            {
                Debug.LogError("No data found for prefab creation.");
            }
        }

        void IUnitSystemDetailsViewListener.RemoveAssetPrefab(ScriptableObject asset)
            => RemoveAssetPrefab(asset, true);

        void IUnitSystemDetailsViewListener.OnNameUpdated()
            => inspectorPane.Refresh();

        #endregion

        private void RemoveAssetPrefab(ScriptableObject asset, bool requiresConfirmation)
        {
            if (asset is not IProvidesEntityPrefab providesPrefab)
                throw new ArgumentException($"Expected asset that implements IProvidesPrefab, instead received: {asset}");

            var prefab = providesPrefab.GetAssociatedPrefab();

            if (requiresConfirmation)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Delete prefab",
                    $"Are you sure you want to delete '{prefab.name}'?",
                    "Yes",
                    "No"
                );

                // Exit on confirm
                if (!confirmed) return;
            }

            var path = AssetDatabase.GetAssetPath(prefab);

            // Clear prefab on the asset and save it
            providesPrefab.SetAssociatedPrefab(null);

            if (AssetDatabase.DeleteAsset(path))
            {
                Debug.Log($"Prefab deleted on path {path}");
            }
            else
            {
                Debug.LogError($"Failed to delete the prefab at path: {path}");
            }

            // Update project inspector, to hide the removed file
            AssetDatabase.Refresh();
            DisplaySelectedAssetObject(asset);
        }

        private void CreateNewAsset(string name, Type type)
        {
            if (type == null)
            {
                Debug.LogError("No type selected.");
                return;
            }

            // Instantiate or create the new asset here based on selectedType
            ScriptableObject newAsset = ScriptableObject.CreateInstance(type);

            // Copy name by default
            if (newAsset is AProducibleSO producible)
            {
                producible.Name = name;
            }
            else if (newAsset is AFactionSO faction)
            {
                faction.SetFactionName(name);
            }

            try
            {
                SaveCreatedAsset(name, newAsset);
            }
            catch
            {
                Debug.LogError($"Failed to save asset: " + name);
                return;
            }

            if (newAsset is ManagedSO managed)
                SystemDatabase.GetInstance().Register(managed);

            DisplaySelectedAssetObject(newAsset);


            Debug.Log($"{type.Name} asset created: " + name);
        }

    }

}
