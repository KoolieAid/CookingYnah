using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Text = TMPro.TextMeshProUGUI;

namespace Crafting
{
    public class LogEntry : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Image outImg;
        [SerializeField] private Image[] slots;

        public void Initialize(Recipe recipe)
        {
            text.text = recipe.output.Name;
            outImg.sprite = recipe.output.SpriteToRender;

            for (int i = 0; i < recipe.slots.Length; i++)
            {
                 var rec = recipe.slots[i];
                 var slot = slots[i];
                 
                 if (rec == null) continue;
                 
                 slot.sprite = rec.SpriteToRender;
            }
        }
    }

}
