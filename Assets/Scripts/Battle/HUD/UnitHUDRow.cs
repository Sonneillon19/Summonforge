using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace RPG.Battle
{
    public class UnitHUDRow : MonoBehaviour
    {
        public Text nameText;
        public Slider hpSlider;
        public Slider atbSlider;

        private UnitRuntime _u;
        private int _maxHp;

        public void Bind(UnitRuntime u)
        {
            _u = u;
            Assert.IsNotNull(_u, "UnitHUDRow.Bind: unit is null");

            _maxHp = Mathf.Max(1, _u.StatsBase.HP);
            if (hpSlider != null) { hpSlider.minValue = 0; hpSlider.maxValue = _maxHp; }
            if (atbSlider != null) { atbSlider.minValue = 0; atbSlider.maxValue = 1f; }

            if (nameText != null)
                nameText.text = $"{_u.Def.displayName} [{_u.Team}]";
        }

        private void Update()
        {
            if (_u == null) return;

            if (hpSlider != null)
                hpSlider.value = Mathf.Clamp(_u.StatsTotal.HP, 0, _maxHp);

            if (atbSlider != null)
                atbSlider.value = Mathf.Clamp01(_u.Atb);

            // Opcional: color rojo si muere
            if (_u.IsDead && nameText != null)
                nameText.color = Color.red;
        }
    }
}
