using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asyncoroutine;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

namespace Crafting
{
    public class Crafter : MonoBehaviour
    {
        public SerializedDictionary<string, Recipe> recipes = new();

        public Slot[] slots;
        
        public Slot output;

        [SerializeField] private GameObject craftingSlotPrefab;

        [SerializeField] private Transform craftingSectionParent;

        [SerializeField] private PlayerInventory inventory;

        [SerializeField] private Transform craftingInventorySection;

        private Dictionary<string, IngredientItem> cache = new();
        
        [Header("UI")]

        [SerializeField]
        [Tooltip("Success Panel")]
        private SuccessPanel correctCraftSprite;
        [SerializeField]
        [Tooltip("The Pot")]
        private Image wrongCraftSprite;

        [SerializeField] private Sprite wrongSprite;

        [Header("Logbook")]
        [SerializeField] private Transform logbookParent;

        [SerializeField] private GameObject logEntryPrefab;

        private readonly List<string> unlockedRecipes = new();

        private void PutToLogs(Recipe rec)
        {
            if (unlockedRecipes.Contains(rec.output.Name)) return;
            
            unlockedRecipes.Add(rec.output.Name);
            
            Instantiate(logEntryPrefab, logbookParent).GetComponent<LogEntry>().Initialize(rec);
        }
        
        public void Listen()
        {
            Debug.Log(GetOutput().Item1.itemName != "None");
        }

        public void CraftToOutput()
        {
            var (ing, rec) = GetOutput();

            if (ing.itemName == InventorySlot.Empty.itemName)
            {
                ChangeOutput();
                return;
            }
            
            PutToLogs(rec);

            ChangeOutput(ing);

            RemoveFromInventoryFromSlots();
            ClearSlots();
            
            inventory.AddItem(ing.itemName, ing.itemBuffData);
            
            UIManager.instance.UpdateCraftingInventoryUI();
        }

        public (InventorySlot, Recipe) GetOutput()
        {
            var inSlots = new List<string>(slots.Length);

            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                
                inSlots.Add(s.GetIngredientInSlot());
            }

            foreach (var (name, recipe) in recipes)
            {
                if (CompareSlots(recipe.slots.Select(data =>
                    {
                        if (data == null) return "None";
                        return data.Name;
                    }).ToArray(), inSlots.ToArray()))
                {
                    return (new InventorySlot(recipe.output), recipe);
                }
            }
            
            return (InventorySlot.Empty, null);
        }

        private static bool CompareSlots(string[] a, string[] b)
        {
            if (a == b) return true;
            
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                var aa = a[i];
                var bb = b[i];

                if (aa != bb) return false;
            }

            return true;
        }

        private void ClearSlots()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }
        }

        private void RemoveFromInventoryFromSlots()
        {
            foreach (var slot in slots)
            {
                if (slot.GetIngredientInSlot() == "None") continue;
                inventory.RemoveItem(slot.GetIngredientInSlot());
            }
        }

        public IngredientItem SpawnInventorySlot(string name, int amount, Sprite img)
        {
            var o = Instantiate(craftingSlotPrefab).GetComponent<IngredientItem>();

            o.SetData(name, amount, img);
            o.craftingSectionParent = craftingSectionParent;

            o.crafter = this;

            o.InventoryAddCloneEvent += AddQuantityToItem;

            o.baseParent = craftingInventorySection;
            o.transform.SetParent(craftingInventorySection);
            return o;
        }

        private async void ChangeOutput()
        {
            var old = wrongCraftSprite.sprite;
            wrongCraftSprite.sprite = wrongSprite;
            await new WaitForSeconds(1f);
            wrongCraftSprite.sprite = old;
        }

        private void ChangeOutput(InventorySlot item)
        {
            var sb = new StringBuilder();
            
            var whole = item.itemBuffData.PermanentBuffs.ToList();
            var ll = PartitionList(whole, 2);

            foreach (var l in ll)
            {
                foreach (var (stat, val) in l)
                {
                    sb.Append($"+ {val} {stat.ToString().Replace("Stat", "").ToUpper()} ");
                }

                sb.Append("\n");
            }
            
            correctCraftSprite.Show(item.itemSprite, item.itemName, sb.ToString());
        }
        
        // Source: bing; prompt: how to split a list in c# to a list of lists
        private static List<List<T>> PartitionList<T>(List<T> items, int size)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < items.Count; i += size)
            {
                list.Add(items.GetRange(i, Math.Min(size, items.Count - i)));
            }
            return list;
        }

        public void AddQuantityToItem(IngredientItem clone, string name, int amount)
        {
            // Changing Values
            if (cache.TryGetValue(clone.itemName, out var item))
            {
                item.UpdateAmount(item.amount + 1);
                
                Destroy(clone.gameObject);
            }
        }

        public void CacheItemsToDict()
        {
            // Clears old cache
            cache = new();
            
            var cCount = craftingInventorySection.transform.childCount;
            for (int i = 0; i < cCount; i++)
            {
                var child = craftingInventorySection.transform.GetChild(i);

                var ingItem = child.GetComponent<IngredientItem>();
                
                cache.TryAdd(ingItem.itemName, ingItem);
            }
        }
        
    }
}
