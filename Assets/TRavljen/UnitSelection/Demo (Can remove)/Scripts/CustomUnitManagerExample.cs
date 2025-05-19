using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection.Demo
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(CustomUnitManagerExample))]
    internal class CustomUnitManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "To use this custom behaviour, selectable units should not have 'ManageUnitObject' " +
                "component on them, remove it from the example prefab units to use this.\n" +
                "And ACTIVATE this game object.",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
#endif

    /// <summary>
    /// Simple example of a custom <see cref="IUnitManager"/> implementation,
    /// which only holds a list of units. Keep in mind if you are to
    /// manage this on your own and not let the <see cref="ManageUnitObject"/>
    /// component handle this, then you should notify the selection system
    /// if and when any of the selectables are removed from the list with
    /// <see cref="ActiveSelections.CleanUpAfterUnit(ISelectable)"/>.
    /// </summary>
    public class CustomUnitManagerExample : MonoBehaviour, IUnitManager
    {

        [SerializeField]
        private List<GameObject> allUnits = new List<GameObject>();

        /// <summary>
        /// All units owned by the local player. This list may contain
        /// any units, static or dynamic (buildings, vehicles).
        /// </summary>
        public List<ISelectable> SelectableUnits { get; protected set; }
            = new List<ISelectable>();

        private void Start()
        {
            // Get component that implements 'ISelectable' from game objects.
            foreach (var unit in allUnits)
            {
                if (unit.TryGetComponent(out ISelectable selectable))
                    SelectableUnits.Add(selectable);
            }


            UnitSelector.Instance.UnitManager = this;
        }

    }

}