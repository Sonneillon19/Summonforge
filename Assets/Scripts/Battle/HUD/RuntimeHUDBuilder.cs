using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro

namespace RPG.Battle
{
    /// HUD runtime sin prefabs:
    ///   - Nombre (TMP) con outline
    ///   - Barras HP/ATB con Image.fillAmount y animación suave
    ///   - Layout paramétrico (sin solapes)
    public class RuntimeHUDBuilder : MonoBehaviour
    {
        public TurnManager turnManager;

        [Header("Posición y ancho")]
        public Vector2 startPos = new Vector2(16, -16); // esquina sup-izq
        public float rowWidth   = 520f;                 // ancho de cada fila
        public float rowSpacing = 10f;                  // separación entre filas

        [Header("Colores")]
        public Color playerColor  = new Color(0.3f, 1f, 0.3f, 1f);
        public Color enemyColor   = new Color(1f, 0.4f, 0.8f, 1f);
        public Color hpFillColor  = new Color(0.25f, 0.8f, 0.25f, 1f);
        public Color atbFillColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color barBgColor   = new Color(1f, 1f, 1f, 0.15f);

        // --- Métricas de layout (ajústalas a tu gusto) ---
        const float NAME_H   = 28f; // alto del texto
        const float PAD_TOP  = 2f;  // margen superior dentro de la fila
        const float GAP      = 6f;  // espacio entre nombre/HP y HP/ATB
        const float HP_H     = 18f;
        const float ATB_H    = 12f;

        private RectTransform _root;

        private void Awake()
        {
            // Canvas (Overlay) + Scaler (1280x720)
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

            // Contenedor raíz del HUD
            var rootGo = new GameObject("HUD_RuntimeRoot", typeof(RectTransform));
            rootGo.transform.SetParent(canvas.transform, false);
            _root = rootGo.GetComponent<RectTransform>();
            _root.anchorMin = new Vector2(0, 1);
            _root.anchorMax = new Vector2(0, 1);
            _root.pivot     = new Vector2(0, 1);
            _root.anchoredPosition = startPos;

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
                .OrderBy(u => u.Team == Team.Enemy) // Player primero
                .ToList();

            float y = 0f;
            foreach (var u in ordered)
            {
                float usedHeight = CreateRow(_root, u, new Vector2(0, -y));
                y += usedHeight + rowSpacing; // separación basada en altura real
            }
        }

        /// Crea una fila y devuelve la altura usada (para espaciar correctamente)
        private float CreateRow(RectTransform parent, UnitRuntime u, Vector2 offset)
        {
            // Altura total calculada
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

            // --- Barras (orden primero para control de dibujo) ---
            float yHP  = -(PAD_TOP + NAME_H + GAP);
            float yATB = -(PAD_TOP + NAME_H + GAP + HP_H + GAP);

            var hpBg   = CreateBar(rt, "HP_BG",  new Vector2(0, yHP),  rowWidth, HP_H,  barBgColor,  false);
            var hpFill = CreateBar(hpBg, "HP_Fill", Vector2.zero,      rowWidth, HP_H,  hpFillColor, true);

            var atbBg   = CreateBar(rt, "ATB_BG", new Vector2(0, yATB), rowWidth, ATB_H, barBgColor,  false);
            var atbFill = CreateBar(atbBg, "ATB_Fill", Vector2.zero,    rowWidth, ATB_H, atbFillColor, true);

            // --- Nombre arriba ---
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
            nameText.textWrappingMode = TextWrappingModes.NoWrap; // evita warning obsoleto
            nameRt.SetAsLastSibling(); // asegura que el nombre quede encima

            // Updater
            var updater = row.AddComponent<RowUpdater>();
            updater.Bind(u, hpFill.GetComponent<Image>(), atbFill.GetComponent<Image>());

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

        // ===== Updater con interpolación suave =====
        private class RowUpdater : MonoBehaviour
        {
            private UnitRuntime _u;
            private Image _hp;
            private Image _atb;
            private int _maxHp;

            // valores mostrados (interpolados)
            private float _hpShown01;
            private float _atbShown01;

            // velocidad de interpolación (mayor = más rápido)
            private const float HP_SPEED  = 8f;
            private const float ATB_SPEED = 14f;

            public void Bind(UnitRuntime u, Image hpFill, Image atbFill)
            {
                _u  = u;
                _hp = hpFill;
                _atb = atbFill;

                _hp.type = Image.Type.Filled;
                _hp.fillMethod = Image.FillMethod.Horizontal;
                _hp.fillOrigin = (int)Image.OriginHorizontal.Left;

                _atb.type = Image.Type.Filled;
                _atb.fillMethod = Image.FillMethod.Horizontal;
                _atb.fillOrigin = (int)Image.OriginHorizontal.Left;

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

            // Lerp “suave” e independiente del framerate
            private float SmoothLerp(float current, float target, float speed)
            {
                return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.deltaTime));
            }
        }
    }
}
