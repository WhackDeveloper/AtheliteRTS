using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Base GameObject indicator, provides interface for toggling between
    /// two different game objects for select and highlight states, disabling
    /// them both when no selection.
    /// </summary>
    public class GameObjectSelectionIndicator : ASelectionIndicator
    {

        [SerializeField]
        private GameObject selectGameObject;

        [SerializeField]
        private GameObject highlightGameObject;

        public override void Select()
        {
            selectGameObject.SetActive(true);
            highlightGameObject.SetActive(false);
        }

        public override void Highlight()
        {
            selectGameObject.SetActive(false);
            highlightGameObject.SetActive(true);
        }

        public override void Clear()
        {
            selectGameObject.SetActive(false);
            highlightGameObject.SetActive(false);
        }

    }
}
