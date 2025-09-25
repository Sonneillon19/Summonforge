using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Combat;

namespace RPG.HUD
{
    public class StatusHUD : MonoBehaviour
    {
        [Header("Owner")]
        public CombatUnit owner;

        [Header("Layout")]
        public Transform container;           // si está vacío, usa este mismo transform
        public StatusIconView iconPrefab;     // asigna en Inspector o coloca el prefab en Resources/UI/StatusIconView
        public int maxIcons = 12;

        private readonly Dictionary<StatusEffectSO, StatusIconView> _activeViews = new();
        private readonly Queue<StatusIconView> _pool = new();

        private StatusController _status;     // a qué StatusController estamos suscritos
        private bool _bound = false;

        // ---------- Ciclo de vida ----------
        void OnValidate()
        {
            if (!container) container = transform;
        }

        void Awake()
        {
            if (!owner) owner = GetComponentInParent<CombatUnit>();
            if (!container) container = transform;
        }

        void OnEnable()
        {
            TryBind();            // primer intento
        }

        void Start()
        {
            if (!_bound) TryBind(); // segundo intento
        }

        void Update()
        {
            if (!_bound) TryBind(); // reintenta hasta que exista StatusController
        }

        void OnDisable()
        {
            Unbind();
        }

        // ---------- Binding ----------
        private void TryBind()
        {
            if (_bound) return;

            if (!owner) owner = GetComponentInParent<CombatUnit>();
            if (!owner) return;

            // No dependemos de owner.Status; buscamos el componente real
            var sc = owner.GetComponent<StatusController>();
            if (!sc) return;

            Unbind(); // por seguridad

            _status = sc;
            _status.OnEffectAdded += HandleAdded;
            _status.OnEffectUpdated += HandleUpdated;
            _status.OnEffectRemoved += HandleRemoved;

            _bound = true;

            Debug.Log($"[HUD] {name}: bind OK. owner={(owner ? owner.name : "NULL")}, container={(container ? container.name : "NULL")}, prefab={(iconPrefab ? iconPrefab.name : "NULL")}");

            // pinta los que ya existan
            foreach (var e in _status.Effects)
                HandleAdded(e);
        }

        private void Unbind()
        {
            if (_status != null)
            {
                _status.OnEffectAdded -= HandleAdded;
                _status.OnEffectUpdated -= HandleUpdated;
                _status.OnEffectRemoved -= HandleRemoved;
            }
            _status = null;
            _bound = false;
        }

        // ---------- Handlers ----------
        private void HandleAdded(StatusEffectInstance inst)
        {
            Debug.Log($"[HUD] Added {inst.data.displayName} | icon={(inst.data.icon ? inst.data.icon.name : "NULL")}");

            if (_activeViews.ContainsKey(inst.data))
            {
                HandleUpdated(inst);
                return;
            }
            if (_activeViews.Count >= maxIcons) return;

            var view = CreateView();
            if (view == null)
            {
                Debug.LogError($"[HUD] {name}: CreateView() devolvió null.");
                return;
            }

            _activeViews[inst.data] = view;
            view.gameObject.SetActive(true);
            view.Bind(inst.data.icon, inst.stacks, inst.remainingTurns);
            // fuerza que el HorizontalLayoutGroup recalcule posiciones ahora
            var rt = container as RectTransform;
            if (rt != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Debug.Log($"[HUD] Ahora hijos en {container.name}: {container.childCount}");

        }

        private void HandleUpdated(StatusEffectInstance inst)
        {
            if (_activeViews.TryGetValue(inst.data, out var view))
            {
                view.Bind(inst.data.icon, inst.stacks, inst.remainingTurns);
            }
            else
            {
                HandleAdded(inst);
            }
        }

        private void HandleRemoved(StatusEffectInstance inst)
        {
            if (_activeViews.TryGetValue(inst.data, out var view))
            {
                _activeViews.Remove(inst.data);
                ReturnView(view);
            }
        }
        
        // ---------- Vistas ----------
        private StatusIconView CreateView()
        {
            if (!container) container = transform;

            // Resolver prefab si está vacío
            if (iconPrefab == null)
            {
                // 1) Resources (coloca tu prefab en Assets/Resources/UI/StatusIconView.prefab)
                iconPrefab = Resources.Load<StatusIconView>("UI/StatusIconView");

                // 2) Cualquier StatusIconView existente en escena (fallback QA)
                if (iconPrefab == null)
                {
                    var any = FindObjectOfType<StatusIconView>(true);
                    if (any != null) iconPrefab = any;
                }
            }

            if (iconPrefab == null)
            {
                // 3) Último recurso: crear una vista minimal en runtime
                Debug.LogWarning($"[HUD] {name}: iconPrefab NULL → creando vista mínima en runtime.");
                var go = new GameObject("StatusIconViewRuntime", typeof(RectTransform), typeof(Image), typeof(StatusIconView));
                go.transform.SetParent(container, false);

                var rt = go.GetComponent<RectTransform>();
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48f);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48f);

                var le = go.AddComponent<LayoutElement>();
                le.preferredWidth = 48; le.preferredHeight = 48;

                var viewMin = go.GetComponent<StatusIconView>();
                viewMin.icon = go.GetComponent<Image>();
                viewMin.stackText = null;
                viewMin.durationText = null;

                Debug.Log($"[HUD] Created RUNTIME view '{go.name}' parent={(go.transform.parent ? go.transform.parent.name : "NULL")}");
                return viewMin;
            }

            // Camino normal con prefab
            var v = Instantiate(iconPrefab, container, false);

            var vrt = v.GetComponent<RectTransform>();
            if (vrt != null)
            {
                vrt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48f);
                vrt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48f);
            }

            var vle = v.gameObject.GetComponent<LayoutElement>();
            if (!vle) vle = v.gameObject.AddComponent<LayoutElement>();
            vle.preferredWidth = 48;
            vle.preferredHeight = 48;
            vle.minWidth = vle.preferredWidth = 48f;   // o 64
            vle.minHeight = vle.preferredHeight = 48f;
            vle.flexibleWidth = 0f;
            vle.flexibleHeight = 0f;
            vle.ignoreLayout = false;
            v.transform.localScale = Vector3.one;
            Debug.Log($"[HUD] Created view '{v.name}' parent={(v.transform.parent ? v.transform.parent.name : "NULL")} under container={(container ? container.name : "NULL")}");

            return v;
        }

        private void ReturnView(StatusIconView v)
        {
            if (v == null) return;
            v.gameObject.SetActive(false);
            v.transform.SetParent(transform, false);
            _pool.Enqueue(v);
        }
        
    }
}
