using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Simple research managment module, responsible for keeping track of currently
    /// completed researches for a player. These are generally less complex than for
    /// resource management.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResearchModule: APlayerModule
    {

        [SerializeField, Tooltip("List of completed researches. Add through " +
            "inspector for default values.")]
        protected List<ResearchSO> completedResearches = new List<ResearchSO>();

        [Header("Events")]
        /// <summary>
        /// Event invoked when research is added to the collection via
        /// <see cref="AddFinishedResearch(ResearchSO)"/>.
        /// </summary>
        public UnityEvent<ResearchSO> OnResearchFinished = new();

        private void OnEnable()
        {
            player.OnRegisterProducible.AddListener(HandleProducibleRegister);
        }

        private void OnDisable()
        {
            player.OnRegisterProducible.RemoveListener(HandleProducibleRegister);
        }

        private void HandleProducibleRegister(AProducibleSO producible, long quantity)
        {
            if (producible is not ResearchSO research)
                return;

            for (int index = 0; index < quantity; index++)
                AddFinishedResearch(research);
        }

        #region Interface

        /// <summary>
        /// Adds new research to the <see cref="completedResearches"/> collection
        /// and invokes the <see cref="OnResearchFinished"/> event.
        /// </summary>
        /// <param name="research">Research to be added.</param>
        public void AddFinishedResearch(ResearchSO research)
        {
            completedResearches.Add(research);
            OnResearchFinished?.Invoke(research);
        }

        /// <summary>
        /// Checks if the <see cref="completedResearches"/> collection contains
        /// a research with the same UUID.
        /// </summary>
        /// <param name="research">Research used for matching UUIDs.</param>
        /// <returns>Returns 'true' if collection contains research.</returns>
        public bool IsResearchComplete(ResearchSO research)
        {
            for (int index = 0; index < completedResearches.Count; index++)
            {
                if (completedResearches[index].ID == research.ID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes research from <see cref="completedResearches"/> collection
        /// if it contains one. Matching is done with UUIDs.
        /// </summary>
        /// <param name="research">Research to be removed.</param>
        /// <returns>
        /// Returns 'true' when research is removed.
        /// Returns 'false' if collection does not contain the research
        /// and therefore could not be removed.
        /// Only produced/completed are present.
        /// </returns>
        public bool RemoveCompletedResearch(ResearchSO research)
        {
            bool isPresent = false;
            for (int index = completedResearches.Count; index >= 0; index--)
            {
                if (completedResearches[index].ID != research.ID)
                {
                    completedResearches.RemoveAt(index);
                    isPresent = true;
                }
            }

            return isPresent;
        }

        #endregion

    }

}
