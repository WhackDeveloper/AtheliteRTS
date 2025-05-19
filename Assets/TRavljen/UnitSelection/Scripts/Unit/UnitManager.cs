using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Component for managing selectable unit references. Standalone singleton
    /// that should be accessed for adding new units through <see cref="GetOrCreate"/>
    /// method, removal can be done on <see cref="Instance"/>. It should not be
    /// null if any of the selectable units are active. It will self destruct
    /// if there are no units to manage.
    /// </summary>
    public class UnitManager : MonoBehaviour, IUnitManager
    {

        #region Properties

        private readonly List<ISelectable> managedUnits = new List<ISelectable>();
        private static UnitManager instance;

        /// <summary>List of managed selectable units.</summary>
        public List<ISelectable> SelectableUnits => managedUnits;

        /// <summary>Self-managed singleton instance.</summary>
        public static UnitManager Instance => instance;

        #endregion

        #region Lifecycle

        /// <returns>
        /// Retrieves or creates management singleton.
        /// </returns>
        public static UnitManager GetOrCreate()
        {
            if (instance != null) return instance;

            var singleton = new GameObject("Selection Unit Manager");
            instance = singleton.AddComponent<UnitManager>();

            // Prevent destruction. There should be only one, if units are
            // destroyed they will be removed from the list.
            DontDestroyOnLoad(singleton);
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForUnitSelector());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Adds unit to list of selectable objects.
        /// </summary>
        /// <param name="unit">Unit to add</param>
        public void AddUnit(ISelectable unit)
        {
            // Prevent duplicates
            if (!managedUnits.Contains(unit))
            {
                managedUnits.Add(unit);
            }
        }

        /// <summary>
        /// Removes unit from the list of selectable objects.
        /// </summary>
        /// <param name="unit">Unit to remove</param>
        public void RemoveUnit(ISelectable unit)
        {
            if (managedUnits.Remove(unit))
            {
                UnitManagementEvents.OnUnitRemoved.Invoke(unit);
            }

            if (managedUnits.Count == 0)
            {
                DestroyManager();
            }
        }

        /// <summary>
        /// Remove all managed units and remove the manager.
        /// </summary>
        public void ClearUnits()
        {
            for (int index = managedUnits.Count - 1; index >= 0; index--)
            {
                var unit = managedUnits[index];
                managedUnits.RemoveAt(index);
                UnitManagementEvents.OnUnitRemoved.Invoke(unit);
            }

            DestroyManager();
        }

        /// <summary>
        /// Adds units for management.
        /// </summary>
        public void AddUnits(List<ISelectable> units)
        {
            // Temp HashSet to prevent duplicates.
            HashSet<ISelectable> existingUnits = new HashSet<ISelectable>(managedUnits);

            foreach (var unit in units)
            {
                // Returns false if item is already in HashSet
                if (existingUnits.Add(unit))
                {
                    managedUnits.Add(unit);
                }
            }
        }

        /// <summary>
        /// Removes units from management.
        /// </summary>
        public void RemoveUnits(List<ISelectable> units)
        {
            HashSet<ISelectable> unitsSet = new HashSet<ISelectable>(units);

            for (int index = managedUnits.Count - 1; index >= 0; index--)
            {
                ISelectable unit = managedUnits[index];
                if (unitsSet.Contains(unit))
                {
                    managedUnits.RemoveAt(index);
                    UnitManagementEvents.OnUnitRemoved.Invoke(unit);
                }
            }

            if (managedUnits.Count == 0)
            {
                DestroyManager();
            }
        }

        #endregion

        #region Convenience

        /// <summary>
        /// Waits until selector instances is available.
        /// </summary>
        private IEnumerator WaitForUnitSelector()
        {
            while (UnitSelector.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            UnitSelector.Instance.UnitManager = this;
        }

        /// <summary>
        /// Destroy and clean up the singleton.
        /// </summary>
        private void DestroyManager()
        {
            Destroy(gameObject);
            instance = null;
        }

        #endregion
    }

}