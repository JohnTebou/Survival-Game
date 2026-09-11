using System.Collections.Generic;
using John;
using UnityEditor;
using UnityEngine;

namespace John
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "John/Item System/Item Definition")]
    public sealed class ItemDefinition : IdentifiedScriptableObject
    {
        #region identification
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        #endregion
    
        #region functionality
        [Header("Functionality")]
        [SerializeField] private List<ItemPropertyDefinition> propertyDefinitions = new();
        #endregion
    
        #region stacking
        [Header("Stacking")]
        [SerializeField] private ItemStackingBehavior stackingBehavior;
        [SerializeField] private int maximumStackSize;
        #endregion

        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
    
        public int MaximumStackSize => maximumStackSize;
        
        public IReadOnlyList<ItemPropertyDefinition> PropertyDefinitions => propertyDefinitions;
    }
}