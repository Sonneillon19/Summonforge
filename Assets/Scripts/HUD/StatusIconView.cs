using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.HUD
{
    public class StatusIconView : MonoBehaviour
    {
        [Header("Refs UI")]
        public Image icon;
        public TMP_Text stackText;
        public TMP_Text durationText;
        public GameObject highlight; // opcional

        public void Bind(Sprite sprite, int stacks, int remainingTurns)
        {
            if (icon)
            {
                icon.sprite = sprite;
                icon.enabled = (sprite != null);
                icon.preserveAspect = true;

                var c = icon.color;
                c.a = 1f;                 // fuerza alpha visible
                icon.color = c;
            }

            if (stackText)
            {
                stackText.enableAutoSizing = false;
                stackText.text = stacks > 1 ? stacks.ToString() : "";
#if TMP_PRESENT
                stackText.textWrappingMode = TextWrappingModes.Normal;
#endif
            }

            if (durationText)
            {
                durationText.enableAutoSizing = false;
                durationText.text = remainingTurns > 0 ? $"{remainingTurns}T" : "";
#if TMP_PRESENT
                durationText.textWrappingMode = TextWrappingModes.Normal;
#endif
            }
        }
    }
}
