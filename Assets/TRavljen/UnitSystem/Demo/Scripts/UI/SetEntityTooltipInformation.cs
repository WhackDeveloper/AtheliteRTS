using UnityEngine;

namespace TRavljen.UnitSystem.Demo
{

    using Tooltip;

    /// <summary>
    /// Retrieves name and description from entity and provides it in a
    /// text format.
    /// </summary>
    public struct EntityTooltipInformation : ITextTooltipInformation
    {
        public string Text { get; }

        public EntityTooltipInformation(IEntity entity, string defaultValue = "Null")
        {
            if (entity as Object == null || entity.Data == null) Text = defaultValue;
            Text = entity.Data.Name + "\n \n" + entity.Data.Description;
        }
    }
    
    /// <summary>
    /// Configuration script, binds <see cref="Entity"/> and <see cref="HoverTooltip"/>
    /// by providing the entity description as <see cref="EntityTooltipInformation"/> to the tooltip component.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity), typeof(HoverTooltip))]
    public class SetEntityTooltipInformation : MonoBehaviour
    {
        private void Awake() 
        {
            var entity = GetComponent<Entity>();
            var hover = GetComponent<HoverTooltip>();
            hover.information = new EntityTooltipInformation(entity);
        }
    }

}