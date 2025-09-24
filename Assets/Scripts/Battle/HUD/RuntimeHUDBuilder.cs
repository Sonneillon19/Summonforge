using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace RPG.Battle
{
    /// HUD runtime sin prefabs:
    ///   - Nombre (TMP) con outline
    ///   - Barras HP/ATB animadas
    ///   - Selección de objetivos clicando filas (resaltado amarillo)
    public class RuntimeHUDBuilder : MonoBehaviour
    {
        public TurnManager turnManager;

        [Header("Posición y ancho")]
        public Vector2 startPos = new Vector2(16, -16);
        public float rowWidth   = 520f;
        public float rowSpacing = 10f;

        [Header("Colores")]
        public Color playerColor  = new Color(0.3f, 1f, 0.3f, 1f);
        public Color enemyColor   = new Color(1f, 0.4f, 0.8f, 1f);
        public Color hpFillColor  = new Color(0.25f, 0.8f, 0.25f, 1f);
        public Color atbFillColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color barBgColor   = new Color(1f, 1f, 1f, 0.15f);

        // Layout métricas
        const float NAME_H   = 28f;
        const float PAD_TOP  = 2f;
        const float GAP      = 6f;
        const float HP_H     = 18f;
        const float ATB_H    = 12f;

        private RectTransform _root;
        private BattleInput _input;

        private void Awake()
        {
            // Canvas
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = cgo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = cgo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // HUD root
            var rootGo = new GameObject("HUD_RuntimeRoot", typeof(RectTransform));
            rootGo.transform.SetParent(canvas.transform, false);
            _root = rootGo.GetComponent<RectTransform>();
            _root.anchorMin = new Vector2(0, 1);
            _root.anchorMax = new Vector2(0, 1);
            _root.pivot     = new Vector2(0, 1);
            _root.anchoredPosition = startPos;

            // Asegura BattleInput
            _input = Object.FindFirstObjectByType<BattleInput>();
            if (_input == null)
            {
                var go = new GameObject("BattleInput");
                _input = go.AddComponent<BattleInput>();
            }

            if (turnManager != null) turnManager.UnitsChanged += Rebuild;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (turnManager != null) turnManager.UnitsChanged -= Rebuild;
        }

        private void ClearChildren()
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }

        private void Rebuild()
        {
            if (turnManager == null || _root == null) return;

            ClearChildren();

            var ordered = turnManager.Units
                .OrderBy(u => u.Team == Team.Enemy)
                .ToList();

            float y = 0f;
            foreach (var u in ordered)
            {
                float usedHeight = CreateRow(_root, u, new Vector2(0, -y));
                y += usedHeight + rowSpacing;
            }
        }

        private float CreateRow(RectTransform parent, UnitRuntime u, Vector2 offset)
        {
            float rowH = PAD_TOP + NAME_H + GAP + HP_H + GAP + ATB_H;

            // Row container
            var row = new GameObject($"Row_{u.Def.displayName}", typeof(RectTransform));
            var rt = row.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(rowWidth, rowH);

            // --- Barras ---
            float yHP  = -(PAD_TOP + NAME_H + GAP);
            float yATB = -(PAD_TOP + NAME_H + GAP + HP_H + GAP);

            var hpBg   = CreateBar(rt, "HP_BG",  new Vector2(0, yHP),  rowWidth, HP_H,  barBgColor,  false);
            var hpFill = CreateBar(hpBg, "HP_Fill", Vector2.zero,      rowWidth, HP_H,  hpFillColor, true);

            var atbBg   = CreateBar(rt, "ATB_BG", new Vector2(0, yATB), rowWidth, ATB_H, barBgColor,  false);
            var atbFill = CreateBar(atbBg, "ATB_Fill", Vector2.zero,    rowWidth, ATB_H, atbFillColor, true);

            // --- Nombre ---
            var nameGo = new GameObject("Name", typeof(RectTransform));
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.SetParent(rt, false);
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot     = new Vector2(0, 1);
            nameRt.anchoredPosition = new Vector2(0, -PAD_TOP);
            nameRt.sizeDelta = new Vector2(rowWidth, NAME_H);

            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.enableAutoSizing = false;
            nameText.fontSize = 28;
            nameText.alignment = TextAlignmentOptions.TopLeft;
            nameText.color = (u.Team == Team.Player) ? playerColor : enemyColor;
            nameText.text  = $"{u.Def.displayName} [{u.Team}]";
            nameText.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            nameText.outlineWidth = 0.2f;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;
            nameRt.SetAsLastSibling();

            // --- Hit-area clicable + highlight ---
            var hitGo = new GameObject("HitArea", typeof(RectTransform), typeof(Image));
            var hitRt = hitGo.GetComponent<RectTransform>();
            hitRt.SetParent(rt, false);
            hitRt.anchorMin = new Vector2(0, 1);
            hitRt.anchorMax = new Vector2(1, 1);
            hitRt.pivot     = new Vector2(0, 1);
            hitRt.anchoredPosition = Vector2.zero;
            hitRt.sizeDelta = new Vector2(rowWidth, rowH);

            var hitImg = hitGo.GetComponent<Image>();
            hitImg.color = new Color(1, 1, 1, 0.001f);
            hitImg.raycastTarget = true;

            var hlGo = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            var hlRt = hlGo.GetComponent<RectTransform>();
            hlRt.SetParent(rt, false);
            hlRt.anchorMin = new Vector2(0, 1);
            hlRt.anchorMax = new Vector2(1, 1);
            hlRt.pivot     = new Vector2(0, 1);
            hlRt.anchoredPosition = Vector2.zero;
            hlRt.sizeDelta = new Vector2(rowWidth, rowH);
            var hlImg = hlGo.GetComponent<Image>();
            hlImg.color = new Color(1f, 0.92f, 0.2f, 0.15f);
            hlGo.transform.SetSiblingIndex(nameRt.GetSiblingIndex());

            // Updater de barras
            var updater = row.AddComponent<RowUpdater>();
            updater.Bind(u, hpFill.GetComponent<Image>(), atbFill.GetComponent<Image>());

            // Click handler
            var click = hitGo.AddComponent<RowClickHandler>();
            click.Init(u, _input, hlImg);

            return rowH;
        }

        // ===== Helpers =====
        private static Sprite _whiteSprite;
        private static Sprite WhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            return _whiteSprite;
        }

        private RectTransform CreateBar(Transform parent, string name, Vector2 offset, float width, float height, Color color, bool isFill)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(width, height);

            var img = go.GetComponent<Image>();
            img.sprite = WhiteSprite();
            img.color  = color;
            img.type   = isFill ? Image.Type.Filled : Image.Type.Simple;
            if (isFill)
            {
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillOrigin = (int)Image.OriginHorizontal.Left;
                img.fillAmount = 1f;
            }
            return rt;
        }

        // ===== Updater (HP/ATB interpolados) =====
        private class RowUpdater : MonoBehaviour
        {
            private UnitRuntime _u;
            private Image _hp;
            private Image _atb;
            private int _maxHp;
            private float _hpShown01;
            private float _atbShown01;

            private const float HP_SPEED  = 8f;
            private const float ATB_SPEED = 14f;

            public void Bind(UnitRuntime u, Image hpFill, Image atbFill)
            {
                _u = u; _hp = hpFill; _atb = atbFill;

                _maxHp = Mathf.Max(1, _u.StatsBase.HP);

                _hpShown01  = Mathf.Clamp01(_u.StatsTotal.HP / (float)_maxHp);
                _atbShown01 = Mathf.Clamp01(_u.Atb);
                _hp.fillAmount  = _hpShown01;
                _atb.fillAmount = _atbShown01;
            }

            void Update()
            {
                if (_u == null) return;

                float hpTarget01  = Mathf.Clamp01(_u.StatsTotal.HP / (float)_maxHp);
                float atbTarget01 = Mathf.Clamp01(_u.Atb);

                _hpShown01  = SmoothLerp(_hpShown01,  hpTarget01,  HP_SPEED);
                _atbShown01 = SmoothLerp(_atbShown01, atbTarget01, ATB_SPEED);

                _hp.fillAmount  = _hpShown01;
                _atb.fillAmount = _atbShown01;

                if (_u.IsDead) _hp.color = Color.red;
            }

            private float SmoothLerp(float current, float target, float speed)
            {
                return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.deltaTime));
            }
        }

        // ===== Click handler para selección =====
        private class RowClickHandler : MonoBehaviour, IPointerClickHandler
        {
            private UnitRuntime _unit;
            private BattleInput _input;
            private Image _highlight;

            public void Init(UnitRuntime unit, BattleInput input, Image highlight)
            {
                _unit = unit;
                _input = input;
                _highlight = highlight;

                if (_input != null)
                    _input.OnTargetChanged += HandleChanged;

                HandleChanged(_input != null ? _input.SelectedTarget : null);
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (_input == null || _unit == null) return;
                if (_unit.Team == Team.Enemy && !_unit.IsDead)
                    _input.SetTarget(_unit);
            }

            private void HandleChanged(UnitRuntime selected)
            {
                if (_highlight == null) return;
                _highlight.enabled = (selected == _unit);
            }

            private void OnDestroy()
            {
                if (_input != null) _input.OnTargetChanged -= HandleChanged;
            }
        }
    }
}
