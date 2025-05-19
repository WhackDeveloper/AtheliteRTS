using System;
using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.UnitSystem
{
   
    #if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(StaticUnitSpawner))]
    public class StaticUnitSpawnerEditor : UnityEditor.Editor
    {

        private StaticUnitSpawner spawner;
        private SerializedProperty spawnPoint;
        
        private void OnEnable()
        {
            spawner = (StaticUnitSpawner)target;
            spawnPoint = serializedObject.FindProperty("spawnPoint");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Unit Spawner with predefined units, can be spawned manually or on \"Awake\". There are no other options on this component.", MessageType.None);
            base.OnInspectorGUI();
            
            if (spawnPoint.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No spawn point set yet, you can use built-in solutions here:", MessageType.Info);

                var existing = spawner.GetComponent<UnitSpawnPoint>();
                if (existing != null && GUILayout.Button("Use from game object"))
                {
                    spawnPoint.objectReferenceValue = existing;
                    Undo.RecordObject(spawnPoint.objectReferenceValue, "Spawner");
                }
                else if (GUILayout.Button("Add Spawn Point"))
                {
                    spawnPoint.objectReferenceValue = Undo.AddComponent<UnitSpawnPoint>(spawner.gameObject);
                    Undo.RecordObject(spawner.gameObject, "Spawner");
                }
                else if (GUILayout.Button("Add Spawn Radius"))
                {
                    spawnPoint.objectReferenceValue = Undo.AddComponent<UnitSpawnRadius>(spawner.gameObject);
                    Undo.RecordObject(spawner.gameObject, "Spawner");
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
    internal static class StaticUnitSpawnerEditorTools
    {
        [MenuItem("GameObject/TRavljen/UnitSystem/Static Spawner")]
        public static void CreateUnitSelectorInScene()
        {
            if (EditorTools.CreateObjectFromMenu<StaticUnitSpawner>("Static Spawner", false))
            {
                Debug.Log("New spawner created. Now set its units.");
            }
        }
    }
    #endif
    
    /// <summary>
    /// Unit Spawner with predefined units, can be spawned manually or on <see cref="Awake"/>.
    /// There are no other options on this component.
    /// </summary>
    public class StaticUnitSpawner : MonoBehaviour
    {

        [SerializeField] private UnitSpawnPoint spawnPoint;
        [SerializeField] private bool spawnOnAwake = true;
        
        public UnitQuantity[] units;
        
        private void Awake()
        {
            if (!spawnOnAwake) return;

            SpawnUnits();
        }

        public void SpawnUnits()
        {
            foreach (var unit in units)
            {
                unit.unit.LoadUnitPrefab(prefab =>
                {
                    for (int index = 0; index < unit.count; index++)
                    {
                        spawnPoint.SpawnUnit(prefab, true);
                    }
                });
            }
        }
    }
}