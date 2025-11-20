using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARPG.Core;

namespace ARPG.UI
{

    public class TargetHealthHUD : MonoBehaviour
    {
        public enum ValueMode { Normalized01, AbsoluteHP }

        [Header("Refs")]
        public LockOnTarget lockOn;
        public Slider slider;                 // <-- use a standard UI Slider
        public TextMeshProUGUI nameText;      // optional
        public CanvasGroup group;             // optional; auto-add if missing

        [Header("Behavior")]
        public ValueMode valueMode = ValueMode.Normalized01;
        [Tooltip("How fast the slider interpolates to target value.")]
        public float lerpSpeed = 10f;
        public bool hideWhenNoLock = true;

        Damageable _current;
        float _targetValue;
        float _displayValue;

        void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
            if (!group) group = gameObject.AddComponent<CanvasGroup>();
            if (!slider)
                Debug.LogWarning("TargetHealthHUD: Slider is not assigned.");
            InitSliderRange(null);  // default to 0..1
            SetVisible(false);
            SetInstant(0f);
        }

        void OnEnable() => RefreshTargetRef();
        void OnDisable() => UnbindCurrent();

        void Update()
        {
            Transform t = lockOn ? lockOn.currentTarget : null;

            if (t == null)
            {
                if (_current != null) UnbindCurrent();
                if (hideWhenNoLock) SetVisible(false);
            }
            else
            {
                if (_current == null || !_current.transform.IsChildOf(t) && _current.transform != t)
                    BindTo(t);

                SetVisible(true);
            }

            // Smooth the slider value
            _displayValue = Mathf.MoveTowards(_displayValue, _targetValue, lerpSpeed * Time.deltaTime);
            if (slider) slider.value = _displayValue;
        }

        // ---- Binding ----
        void BindTo(Transform target)
        {
            UnbindCurrent();

            _current = target.GetComponentInParent<Damageable>();
            if (_current == null)
            {
                if (hideWhenNoLock) SetVisible(false);
                return;
            }

            _current.OnHealthChanged += OnHPChanged;
            _current.OnKilled += OnKilled;

            if (nameText) nameText.text = _current.name;

            InitSliderRange(_current);
            float initial = valueMode == ValueMode.Normalized01
                ? SafeNorm(_current.health, _current.maxHealth)
                : _current.health;

            _targetValue = initial;
            SetInstant(initial);
        }

        void UnbindCurrent()
        {
            if (_current != null)
            {
                _current.OnHealthChanged -= OnHPChanged;
                _current.OnKilled -= OnKilled;
                _current = null;
            }
        }

        void InitSliderRange(Damageable dmg)
        {
            if (!slider) return;

            if (valueMode == ValueMode.Normalized01)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
            else // AbsoluteHP
            {
                float max = (dmg != null && dmg.maxHealth > 0f) ? dmg.maxHealth : 100f;
                slider.minValue = 0f;
                slider.maxValue = max;
            }
        }

        // ---- Events ----
        void OnHPChanged(float newHP)
        {
            if (_current == null || !slider) return;

            if (valueMode == ValueMode.Normalized01)
                _targetValue = SafeNorm(newHP, _current.maxHealth);
            else
            {
                // If maxHealth changed dynamically, keep slider range in sync
                if (!Mathf.Approximately(slider.maxValue, _current.maxHealth))
                    slider.maxValue = Mathf.Max(1f, _current.maxHealth);
                _targetValue = Mathf.Clamp(newHP, slider.minValue, slider.maxValue);
            }
        }

        void OnKilled(HitInfo _)
        {
            if (hideWhenNoLock) SetVisible(false);
        }

        // ---- Visual helpers ----
        void SetVisible(bool v)
        {
            if (!group) return;
            group.alpha = v ? 1f : 0f;
            group.interactable = v;
            group.blocksRaycasts = v;
        }

        void SetInstant(float v)
        {
            _displayValue = _targetValue = v;
            if (slider) slider.value = v;
        }

        float SafeNorm(float hp, float max) => (max > 0f) ? Mathf.Clamp01(hp / max) : 0f;

        // Public utility
        public void RefreshTargetRef()
        {
            var t = lockOn ? lockOn.currentTarget : null;
            if (t) BindTo(t);
            else { UnbindCurrent(); if (hideWhenNoLock) SetVisible(false); }
        }
    }

}
