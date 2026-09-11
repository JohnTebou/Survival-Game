using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace John
{
    public sealed class InventoryOperationService
    {
        public void AddItem(ItemContainer target){}
        
        public void RemoveItem(ItemContainer target){}
        
        public void TransferItems(ItemContainer source, ItemContainer destination){}
    }
    
    public abstract class ItemContainer
    {
        private int slotCount;
        private List<ItemSlot> _slots;
    }

    public sealed class PlayerInventory : ItemContainer
    {
        
    }

    public sealed class HotbarInventory : ItemContainer
    {
    
    }

    public sealed class ArmorInventory : ItemContainer
    {
    
    }

    public enum SlotItemRestriction
    {
        None
    }

    public sealed class ItemSlot
    {
        private ItemStack stack;
        public bool HasMaxCapacity { get; private set; }
        public int MaxCapacity { get; private set; }

        public ItemSlot(bool hasMaxCapacity = false, int maxCapacity = 0)
        {
            HasMaxCapacity = hasMaxCapacity;
            MaxCapacity = maxCapacity;
        }
        
        
    }

    public abstract class ItemStack
    {
        public abstract ItemInstance RepresentativeItemInstance { get; } 
    }

    public sealed class SimpleStack : ItemStack
    {
        public override ItemInstance RepresentativeItemInstance { get; }

        public SimpleStack(ItemInstance instance)
        {
            RepresentativeItemInstance = instance;
        }
    }

    public sealed class TrackedStack : ItemStack
    {
        private List<ItemInstance> itemInstances = new();
        
        public override ItemInstance RepresentativeItemInstance => itemInstances.Count > 0 ? itemInstances[^1] : null;

        public TrackedStack(List<ItemInstance> itemInstances)
        {
            this.itemInstances = itemInstances;
        }
    }

    public sealed class PlayerEquipmentController : MonoBehaviour
    {
        [SerializeField] private HotbarInventory hotbarInventory;
        private int activeSlot = 0;

        void Update()
        {
            
        }
    }

    public abstract class Interactable : MonoBehaviour
    {
        public abstract void TryInteract();
        public abstract float InteractionDistance { get; }
        public abstract float InteractionTime { get; }
    }

    public class IdentifiedScriptableObject : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField, Locked] private string id;

        public string Id => id;
        
#if UNITY_EDITOR
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                string path = AssetDatabase.GetAssetPath(this);
                id = AssetDatabase.AssetPathToGUID(path);
            }
        }
#endif
    }

    public sealed class ItemProperty : ScriptableObject
    {
        [SerializeField] private string id;
    }

    public abstract class ItemPropertyInstance
    {
        public ItemPropertyDefinition Definition { get; }
        public string Id { get; }
        public string DefinitionId => Definition.Id;
        public PropertyStackingBehavior StackingBehavior => Definition.StackingBehavior;

        protected ItemPropertyInstance(ItemPropertyDefinition definition)
        {
            Definition = definition;
            Id = Guid.NewGuid().ToString();
        }

        public abstract bool Matches(ItemPropertyInstance other);
        
        
    }

    public abstract class ItemPropertyDefinition : IdentifiedScriptableObject
    {
        [SerializeField] private PropertyStackingBehavior stackingBehavior;
        
        public PropertyStackingBehavior StackingBehavior => stackingBehavior;

        public abstract ItemPropertyInstance CreateInstance();
    }

    public enum PropertyStackingBehavior
    {
        MustMatch,
        Reconcile,
        Ignore
    }

    public sealed class ItemInstance
    {
        
        private Dictionary<string, ItemPropertyInstance> properties = new();

        public ItemDefinition Definition { get; }
        public string Id { get; }
        public string DefinitionId => Definition.Id;
        public string DisplayName => Definition.DisplayName;
        public string Description => Definition.Description;
        public Sprite Icon;
        
        public IReadOnlyDictionary<string, ItemPropertyInstance> Properties => properties;
        

        public ItemInstance(ItemDefinition definition)
        {
            Definition = definition;
            Id = Guid.NewGuid().ToString();
            
            CreateProperties();
        }

        private void CreateProperties()
        {
            foreach (ItemPropertyDefinition propertyDefinition in Definition.PropertyDefinitions)
            {
                ItemPropertyInstance property = propertyDefinition.CreateInstance();
                properties.Add(property.DefinitionId, property);
            }
        }
        
        public bool TryGetProperty(string propertyId, out ItemPropertyInstance property) => properties.TryGetValue(propertyId, out property);

        // public bool TryGetProperty<T>
        
        // determines if item instances can merge for inventory system
        public bool CanMergeWith(ItemInstance other)
        {
            // null other or different definitions => instant false
            if (other == null)
                return false;

            if (other.DefinitionId != DefinitionId)
                return false;

            // handle checks for properties we know exist on this item
            foreach ((string propertyId, ItemPropertyInstance propertyInstance) in properties)
            {
                // if a reconcile or match property doesn't have its analog on the other item instance, return false; it's fine if stacking behavior is ignore
                if (!other.Properties.TryGetValue(propertyId, out ItemPropertyInstance otherPropertyInstance))
                {
                    if (propertyInstance.StackingBehavior != PropertyStackingBehavior.Ignore)
                    {
                        return false;
                    }

                    continue;
                }

                // now we know the other item instance has the corresponding property instance; if it's reconcile or ignore we know automatically they can merge

                // if the property instance's stacking behavior is must match and they don't match, early return false
                if (propertyInstance.StackingBehavior == PropertyStackingBehavior.MustMatch)
                {
                    if (!propertyInstance.Matches(otherPropertyInstance))
                        return false;
                }
            }
            
            // handle properties only found on the other item
            foreach ((string otherPropertyId, ItemPropertyInstance otherPropertyInstance) in other.Properties)
            {
                if (properties.ContainsKey(otherPropertyId))
                    continue;
                
                if (otherPropertyInstance.StackingBehavior != PropertyStackingBehavior.Ignore)
                    return false;
            }

            return true;
        }
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison
    }

    public enum PropertyType
    {
        PhysicalDamage,
        FireDamage,
        
        Durability,
    }

    public enum BlessingSource
    {
        
    }

    [System.Serializable]
    public sealed class ItemStat
    {
        
    }

    [CreateAssetMenu(fileName = "RecipeDefinition", menuName = "John/Crafting System/Recipe Definition")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Header("Input")]
        [SerializeField] List<RecipeIngredient> recipeIngredients;
    
        [Header("Output")]
        public ItemDefinition Output;
        public int Quantity { get; }
    }

    [System.Serializable]
    public sealed class RecipeIngredient
    {
        public ItemDefinition Ingredient { get; }
        public int Amount { get; }
    }

    public enum ItemStackingBehavior
    {
        Simple,
        Tracked
    }

    public sealed class ItemRules
    {
        
    }
}
