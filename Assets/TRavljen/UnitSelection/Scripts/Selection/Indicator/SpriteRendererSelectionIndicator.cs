using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Base sprite renderer indicator. Provides interface for changing its
    /// color based on selection state or hiding it once its cleared.
    /// </summary>
    [ExecuteInEditMode]
    public class SpriteRendererSelectionIndicator : ASelectionIndicator
    {
        #region Serializable Fields

        [SerializeField]
        private SpriteRenderer indicatorRenderer;

        [SerializeField]
        private Sprite selectedSprite;

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Sprite highlightedSprite;

        [SerializeField]
        private Color highlightedColor = Color.white;

        #endregion

        private void Awake()
        {
            if (indicatorRenderer == null)
                indicatorRenderer = GetComponent<SpriteRenderer>();
        }

        #region ASelectionIndicator

        public override void Select()
        {
            indicatorRenderer.color = selectedColor;
            indicatorRenderer.sprite = selectedSprite;
            gameObject.SetActive(true);
        }

        public override void Highlight()
        {
            indicatorRenderer.color = highlightedColor;
            indicatorRenderer.sprite = highlightedSprite;
            gameObject.SetActive(true);
        }

        public override void Clear()
        {
            gameObject.SetActive(false);
        }

        #endregion

    }

}