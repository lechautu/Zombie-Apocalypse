using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace ARPG.UI
{
    public class DamageNumberHUD : MonoBehaviour
    {
        [Header("Refs")]
        public Camera mainCamera;
        public RectTransform container;        // usually this object
        public TextMeshProUGUI prefab;         // pooled item

        [Header("Motion")]
        public Vector2 startJitter = new Vector2(8f, 4f);
        public Vector2 floatVelocity = new Vector2(0f, 48f); // px/sec
        public float life = 0.8f;
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);

        class Item
        {
            public TextMeshProUGUI tmp;
            public Vector2 pos;
            public Vector2 vel;
            public float t;
            public Transform follow;   // optional: follow a transform while alive
            public Vector3 worldOffset;
        }

        readonly Queue<TextMeshProUGUI> _pool = new();
        readonly List<Item> _live = new();

        public static DamageNumberHUD Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            if (!mainCamera) mainCamera = Camera.main;
            if (!container) container = (RectTransform)transform;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var it = _live[i];

                // Optional follow (for multi-tick dots)
                if (it.follow)
                {
                    var sp = mainCamera.WorldToScreenPoint(it.follow.position + it.worldOffset);
                    Vector2 anchored;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(container, sp, null, out anchored);
                    it.pos = anchored;
                }

                it.t += dt;
                float n = Mathf.Clamp01(it.t / life);

                it.pos += it.vel * dt;
                it.tmp.rectTransform.anchoredPosition = it.pos;

                float a = alphaCurve.Evaluate(n);
                var c = it.tmp.color; c.a = a; it.tmp.color = c;
                it.tmp.rectTransform.localScale = Vector3.one * scaleCurve.Evaluate(n);

                if (it.t >= life)
                {
                    Recycle(it.tmp);
                    _live.RemoveAt(i);
                }
            }
        }

        TextMeshProUGUI Get()
        {
            if (_pool.Count > 0) { var t = _pool.Dequeue(); t.gameObject.SetActive(true); return t; }
            return Instantiate(prefab, container);
        }

        void Recycle(TextMeshProUGUI t)
        {
            t.gameObject.SetActive(false);
            _pool.Enqueue(t);
        }

        // === Public API ===
        public void SpawnWorld(Vector3 worldPos, float amount, bool crit = false, Color? color = null)
        {
            var sp = mainCamera.WorldToScreenPoint(worldPos);
            Vector2 anchored;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, sp, null, out anchored);

            var tmp = Get();
            tmp.text = crit ? Mathf.RoundToInt(amount).ToString() + "!" : Mathf.RoundToInt(amount).ToString();
            tmp.color = color ?? (crit ? new Color(1f, 0.85f, 0.2f, 1f) : Color.white);

            var it = new Item
            {
                tmp = tmp,
                pos = anchored + new Vector2(Random.Range(-startJitter.x, startJitter.x), Random.Range(-startJitter.y, startJitter.y)),
                vel = floatVelocity + new Vector2(Random.Range(-8f, 8f), Random.Range(0f, 8f)),
                t = 0f
            };
            tmp.rectTransform.anchoredPosition = it.pos;
            _live.Add(it);
        }

        public void SpawnFollow(Transform follow, Vector3 worldOffset, float amount, bool crit = false, Color? color = null)
        {
            var tmp = Get();
            tmp.text = crit ? Mathf.RoundToInt(amount).ToString() + "!" : Mathf.RoundToInt(amount).ToString();
            tmp.color = color ?? (crit ? new Color(1f, 0.85f, 0.2f, 1f) : Color.white);

            // Initialize at follow position
            var sp = mainCamera.WorldToScreenPoint(follow.position + worldOffset);
            Vector2 anchored;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, sp, null, out anchored);

            var it = new Item
            {
                tmp = tmp,
                pos = anchored,
                vel = floatVelocity,
                t = 0f,
                follow = follow,
                worldOffset = worldOffset
            };
            tmp.rectTransform.anchoredPosition = it.pos;
            _live.Add(it);
        }
    }
}