using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitSelection
{

    /// <summary>
    /// Base mesh render indicator. Provides interface for changing the mesh and materials
    /// based on selection state.
    /// </summary>
    [ExecuteInEditMode]
    public class MeshRendererSelectionIndicator : ASelectionIndicator
    {

        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private Mesh highlightMesh;

        [SerializeField]
        private Material[] highlightMaterials;

        [SerializeField]
        private Mesh selectMesh;

        [SerializeField]
        private Material[] selectMaterials;

        private void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
        }

        public override void Select()
        {
            gameObject.SetActive(true);

            meshRenderer.sharedMaterials = selectMaterials;
            meshFilter.sharedMesh = selectMesh;
        }

        public override void Highlight()
        {
            gameObject.SetActive(true);

            meshRenderer.sharedMaterials = highlightMaterials;
            meshFilter.sharedMesh = highlightMesh;
        }

        public override void Clear()
        {
            gameObject.SetActive(false);
        }
    }
}