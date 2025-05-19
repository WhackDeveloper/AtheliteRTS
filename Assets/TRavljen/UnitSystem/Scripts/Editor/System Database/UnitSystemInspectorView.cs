using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace TRavljen.UnitSystem.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine.UIElements;
    using Collection;

    public interface IUnitSystemInspectorViewListener
    {
        void AddNewAsset(Type type);
        void DuplicateSelectedAsset();
        void DeleteSelectedAsset();
        void ItemSelected(int assetID);
    }

    public class UnitSystemInspectorView : VisualElement
    {
        private const float padding = 8;
        
        private static readonly List<Type> ScriptableObjectTypes = FindScriptableObjectTypes(typeof(ManagedSO));

        private Button addButton;
        private Button deleteButton;
        private Button duplicateButton;
        private VisualElement listContainer;
        private PopupField<Type> typeDropdown;
        private PopupField<(string title, UnitTypeSO type)> unitTypeDropdown;
        
        private int SelectedTypeIndex
        {
            get => EditorPrefs.GetInt("unitsystem.manager.selectedTypeIndex", 0);
            set => EditorPrefs.SetInt("unitsystem.manager.selectedTypeIndex", value);
        }
        
        private UnitTypeSO selectedUnitType;
        private string searchText;
        private Type selectedType;
        private Type selectedSubtype;
        
        public ScriptableObject SelectedAsset
        {
            get => selectedAsset;
            set
            {
                selectedAsset = value;
                // Update buttons on left pane
                bool isAssetSelected = selectedAsset != null;
                duplicateButton.SetEnabled(isAssetSelected);
                deleteButton.SetEnabled(isAssetSelected);
            }
        }
        
        private ScriptableObject selectedAsset;

        private readonly List<(string name, int ID)> namedIds = new();
        private List<string> selectedItems = new();

        public IUnitSystemInspectorViewListener listener;
        
        public UnitSystemInspectorView(ScriptableObject selectedAsset)
        {
            searchText = EditorPrefs.GetString("unitsystem.manager.searchText", "");
            
            this.selectedAsset = selectedAsset;
            
            selectedSubtype = null;
            listContainer = new VisualElement();
            
            // Setup index
            var index = SelectedTypeIndex;
            if (index >= ScriptableObjectTypes.Count)
            {
                index = 0;
                SelectedTypeIndex = 0;
            }

            if (index < ScriptableObjectTypes.Count)
                selectedType = ScriptableObjectTypes[index];

        }

        public bool SetDefaultSelection()
        {
            // Select first if there is anything to select
            if (namedIds.Count > 0)
            {
                selectedItems = new() { namedIds[0].name };
                return true;
            }

            return false;
        }

        public void Refresh()
        {
            Clear();
            
            // Dropdown for type selection
            Add(GetTypeDropdown());

            // Search field
            var searchField = new TextField("Search")
            {
                value = searchText
            };
            
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue.ToLowerInvariant();
                EditorPrefs.SetString("unitsystem.manager.searchText", searchText);
                RefreshItemList();
            });
            Add(searchField);

            if (typeof(AUnitSO) == selectedType || selectedType.IsSubclassOf(typeof(AUnitSO)))
            {
                unitTypeDropdown = CreateUnitTypeDropdown();
                Add(unitTypeDropdown);
            }

            var horizontal = new VisualElement();
            horizontal.style.flexDirection = FlexDirection.Row;
            horizontal.style.marginTop = 4;

            // Show class selection if there are more options in the project.
            var selectedTypeSubtypes = GetAllSubclassesOf(selectedType);
            if (selectedTypeSubtypes != null && selectedTypeSubtypes.Count > 0)
            {
                // IF selected type is not abstract, show selection option
                if (!selectedType.IsAbstract)
                    selectedTypeSubtypes.Insert(0, selectedType);

                List<(string title, Type type)> creationType = selectedTypeSubtypes.Select(type => (type.Name, type)).ToList();

                // Determine selected index, excluding abstract types in the popup selection
                int selectedIndex = selectedTypeSubtypes.IndexOf(selectedType);

                int index = 0;
                var dropDown = new PopupField<(string title, Type type)>(
                    "Creation Type",
                    creationType,
                    index,
                    e => e.title,
                    e => e.title);

                // Set default value if not set yet.
                selectedSubtype = creationType[index].type;

                dropDown.RegisterValueChangedCallback(evt =>
                {
                    selectedSubtype = evt.newValue.type;
                    RefreshItemList();
                });

                Add(dropDown);
            }

            addButton = new Button(() =>
            {
                listener.AddNewAsset(selectedSubtype ?? selectedType);
            })
            {
                text = "Create new",
                style = {
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };

            duplicateButton = new Button(() =>
            {
                listener.DuplicateSelectedAsset();
            })
            {
                text = "Duplicate",
                style = { width = 80 }
            };

            deleteButton = new Button(() =>
            {
                listener.DeleteSelectedAsset();
            })
            {
                text = "Delete",
                style = { width = 60 }
            };

            bool isAssetSelected = selectedAsset != null;
            duplicateButton.SetEnabled(isAssetSelected);
            deleteButton.SetEnabled(isAssetSelected);

            horizontal.Add(addButton);
            horizontal.Add(duplicateButton);
            horizontal.Add(deleteButton);

            Add(horizontal);

            InitializeTreeView();

            RefreshItemList();
        }

        private void InitializeTreeView()
        {
            listContainer.style.flexDirection = FlexDirection.Column;
            listContainer.style.borderBottomWidth = 1;
            listContainer.style.borderBottomColor = new UnityEngine.Color(0.2f, 0.2f, 0.2f);
            
            VisualElement spacer = new VisualElement();
            spacer.style.height = 10;
            spacer.style.flexShrink = 0;

            Add(spacer);

            var scrollView = new ScrollView()
            {
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            
            scrollView.Add(listContainer);
            Add(scrollView);
        }

        public void RefreshItemList()
        {
            listContainer.Clear();

            if (selectedType == null) return;

            namedIds.Clear();

            // Gather filtered assets of the selected type
            var assets = SystemDatabase.GetInstance().GetAllAssets();

            foreach (ManagedSO asset in assets)
            {
                if (asset == null || !selectedType.IsAssignableFrom(asset.GetType()))
                    continue;

                string assetName = asset.name;

                // Check if asset matches the search text
                if (!string.IsNullOrEmpty(searchText) && !assetName.ToLowerInvariant().Contains(searchText))
                    continue;

                // Check if asset needs to be loaded and compare with unit type.
                if (typeof(AUnitSO) == selectedType && selectedUnitType.IsNotNull())
                {
                    AUnitSO unit = (AUnitSO)asset;
                    if (!unit.UnitTypes.Contains(selectedUnitType))
                        continue;
                }

                namedIds.Add((assetName, asset.ID));
            }
            
            namedIds.Sort((first, second) => 
                string.Compare(first.name, second.name, StringComparison.Ordinal));

            Color selectedColor = new Color(0.22f, 0.36f,  0.51f, 1);
                
            for (int i = 0; i < namedIds.Count; i++)
            {
                var (name, id)  = namedIds[i];
                var label = new Label(name);
                label.style.paddingLeft = 8;
                label.style.paddingTop = 4;
                label.style.paddingBottom = 4;

                label.style.backgroundColor =
                    selectedItems.Contains(name) ? selectedColor : Color.clear;
                label.RegisterCallback<ClickEvent>(evt =>
                {
                    OnItemSelected(name, id);
                });
                label.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    if (selectedItems.Contains(name))
                    {
                        label.style.backgroundColor = selectedColor;
                    }
                    else
                    {
                        label.style.backgroundColor = new Color(1,1,1, 0.05f);
                    }
                });
                label.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    label.style.backgroundColor = selectedItems.Contains(name) ? selectedColor : Color.clear;
                });
                listContainer.Add(label);
            }

            if (selectedAsset == null && namedIds.Count > 0)
            {
                selectedItems = new() { namedIds[0].name };
            }
        }

        private PopupField<(string title, UnitTypeSO type)> CreateUnitTypeDropdown()
        {
            // Create types, with first entry being clear selection.
            List<UnitTypeSO> unitTypes = new();
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(UnitTypeSO)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

                // Check if asset matches the search text
                if (!string.IsNullOrEmpty(searchText) && !assetName.ToLowerInvariant().Contains(searchText))
                    continue;

                UnitTypeSO type = (UnitTypeSO)AssetDatabase.LoadAssetAtPath(path, typeof(UnitTypeSO));
                unitTypes.Add(type);
            }
            
            var options = unitTypes.ConvertAll(type => (type.TypeName, type));
            options.Insert(0, ("None", null));
            var dropDown = new PopupField<(string title, UnitTypeSO type)>(
                "Filter Unit Type",
                options,
                Math.Max(0, unitTypes.IndexOf(selectedUnitType)),
                type => type.title,
                type => type.title);

            dropDown.RegisterValueChangedCallback(evt =>
            {
                selectedUnitType = evt.newValue.type;
                RefreshItemList();
            });

            return dropDown;
        }

        private PopupField<Type> GetTypeDropdown()
        {
            if (typeDropdown != null) return typeDropdown;

            typeDropdown = new PopupField<Type>(
                "Select Type",
                new List<Type>(ScriptableObjectTypes),
                SelectedTypeIndex,
                type => type.Name.Replace("SO", ""),
                type => type.Name.Replace("SO", ""));

            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                selectedType = evt.newValue;
                SelectedTypeIndex = ScriptableObjectTypes.IndexOf(selectedType);

                selectedSubtype = null;
                Refresh();

                if (namedIds.Count > 0)
                    selectedItems = new() { namedIds[0].name };
            });

            return typeDropdown;
        }

        private List<Type> GetAllSubclassesOf(Type parent)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(parent))
                .ToList();
        }

        private void OnItemSelected(string name, int id)
        {
            selectedItems.Clear();
            selectedItems.Add(name);
            listener.ItemSelected(id);

            RefreshItemList();
        }
        
        private static List<Type> FindScriptableObjectTypes(Type parentType, bool keepAbstract = true)
        {
            try
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(parentType) && (keepAbstract || !type.IsAbstract))
                    .ToList();

                types.Sort((typeA, typeB) => typeA.FullName.CompareTo(typeB.FullName));

                if (!keepAbstract) return types;
                
                var abstractTypes = new List<Type>();
                for (var i = types.Count - 1; i >= 0; i--)
                {
                    var currentType = types[i];
                    if (!currentType.IsAbstract) continue;
                    
                    abstractTypes.Add(currentType);
                    types.RemoveAt(i);
                }

                types.InsertRange(0, abstractTypes);
                return types;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error finding Scriptable Objects: " + ex.Message);
                return null;
            }
        }

    }

}