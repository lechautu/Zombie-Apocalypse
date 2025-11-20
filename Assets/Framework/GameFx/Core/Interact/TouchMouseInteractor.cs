using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFx.Core.Interact
{
    public interface ITappable { void OnTap(InteractorHit hit); }
    public interface IPressable { void OnPress(InteractorHit hit); void OnRelease(InteractorHit hit); }
    public interface ILongPressable { void OnLongPress(InteractorHit hit); }
    public interface IDraggable { void OnDrag(InteractorHit hit, Vector3 worldDelta); }


    [DefaultExecutionOrder(-50)]
    public class TouchMouseInteractor : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] Camera cam;
        [SerializeField] LayerMask interactableMask = ~0; // set to your "Interactable" layer(s)
        [SerializeField] float rayDistance = 200f;

        [Header("Tap / Drag / Long Press")]
        [SerializeField] float tapMaxDuration = 0.25f;     // seconds
        [SerializeField] float tapMaxMovePixels = 12f;     // screen px
        [SerializeField] float longPressDuration = 0.6f;   // seconds
        [SerializeField] bool enableDrag = true;

        [Header("Ground Projection (2.5D)")]
        [SerializeField] float groundY = 0f;               // project to y=0 plane for world deltas

        // internal state
        bool isDown;
        int downFingerId = -1; // for touch tracking
        Vector2 downScreenPos;
        Vector3 downWorldPos;
        float downTime;
        bool longPressed;
        bool movedBeyondTap;
        InteractorHit downHit;      // initial hit target (sticky while held)

        void Reset()
        {
            cam = Camera.main;
        }

        void Awake()
        {
            if (!cam) cam = Camera.main;
            if (!cam) Debug.LogWarning("TouchMouseInteractor: assign a Camera.");
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouse();
#else
        HandleTouch();
#endif
        }

        // -------------------- TOUCH --------------------
        void HandleTouch()
        {
            // no touches
            if (Input.touchCount == 0) { ResetIfLost(); return; }

            // choose primary finger (first that began or currently tracked)
            Touch t = default;
            if (!isDown)
            {
                // find a Began touch that is not over UI
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var candidate = Input.touches[i];
                    if (candidate.phase == TouchPhase.Began && !IsPointerOverUI(candidate.fingerId))
                    { t = candidate; Begin(candidate.fingerId, candidate.position); break; }
                }
            }

            // already down: find the current finger
            if (isDown)
            {
                bool found = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].fingerId == downFingerId)
                    { t = Input.touches[i]; found = true; break; }
                }
                if (!found) { Cancel(); return; } // finger vanished

                switch (t.phase)
                {
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        Hold(t.position);
                        break;
                    case TouchPhase.Ended:
                        End(t.position);
                        break;
                    case TouchPhase.Canceled:
                        Cancel();
                        break;
                }
            }
        }

        // -------------------- MOUSE (Editor convenience) --------------------
        void HandleMouse()
        {
            // begin
            if (!isDown && Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                Begin(-1, Input.mousePosition);
            }
            // hold
            if (isDown && Input.GetMouseButton(0))
            {
                Hold((Vector2)Input.mousePosition);
            }
            // end
            if (isDown && Input.GetMouseButtonUp(0))
            {
                End((Vector2)Input.mousePosition);
            }
        }

        // -------------------- CORE STATE MACHINE --------------------
        void Begin(int fingerId, Vector2 screenPos)
        {
            isDown = true;
            downFingerId = fingerId;
            downScreenPos = screenPos;
            downTime = UnityEngine.Time.unscaledTime;
            longPressed = false;
            movedBeyondTap = false;

            // initial raycast
            if (Raycast(screenPos, out downHit))
            {
                downWorldPos = downHit.worldPoint;

                // press callback
                var press = downHit.Get<IPressable>();
                press?.OnPress(downHit);
            }
            else
            {
                // if nothing hit, still remember the world point under finger for drag delta projection
                downWorldPos = ScreenToWorldOnGround(screenPos);
                downHit = default;
            }
        }

        void Hold(Vector2 screenPos)
        {
            if (!isDown) return;

            // track distance for tap decision
            float movedPixels = (screenPos - downScreenPos).magnitude;
            if (movedPixels > tapMaxMovePixels) movedBeyondTap = true;

            // long press
            if (!longPressed && !movedBeyondTap && (UnityEngine.Time.unscaledTime - downTime) >= longPressDuration)
            {
                if (downHit.gameObject)
                {
                    var lp = downHit.Get<ILongPressable>();
                    lp?.OnLongPress(downHit);
                    longPressed = true;
                }
            }

            // drag
            if (enableDrag && downHit.gameObject)
            {
                Vector3 currWorld = ScreenToWorldOnGround(screenPos);
                Vector3 worldDelta = currWorld - downWorldPos;

                if (worldDelta.sqrMagnitude > 0f)
                {
                    var drag = downHit.Get<IDraggable>();
                    drag?.OnDrag(downHit, worldDelta);
                    downWorldPos = currWorld;
                }
            }
        }

        void End(Vector2 screenPos)
        {
            if (!isDown) return;

            // release callback
            if (downHit.gameObject)
            {
                var press = downHit.Get<IPressable>();
                press?.OnRelease(downHit);
            }

            // tap?
            float duration = UnityEngine.Time.unscaledTime - downTime;
            if (!movedBeyondTap && duration <= tapMaxDuration && !IsPointerOverUICurrent(screenPos))
            {
                // re-validate target on release (optional)
                InteractorHit upHit;
                if (Raycast(screenPos, out upHit) && upHit.gameObject == downHit.gameObject)
                {
                    var tap = upHit.Get<ITappable>();
                    tap?.OnTap(upHit);
                }
                else if (downHit.gameObject) // fallback to down target
                {
                    var tap = downHit.Get<ITappable>();
                    tap?.OnTap(downHit);
                }
            }

            ResetState();
        }

        void Cancel()
        {
            if (!isDown) return;
            // inform release on cancel
            if (downHit.gameObject)
            {
                var press = downHit.Get<IPressable>();
                press?.OnRelease(downHit);
            }
            ResetState();
        }

        void ResetIfLost()
        {
            if (isDown) Cancel();
        }

        void ResetState()
        {
            isDown = false;
            downFingerId = -1;
            downHit = default;
            longPressed = false;
            movedBeyondTap = false;
        }

        // -------------------- HELPERS --------------------
        bool Raycast(Vector2 screenPos, out InteractorHit ih)
        {
            ih = default;
            if (!cam) return false;

            // Ray ray = cam.ScreenPointToRay(screenPos);
            var origin = cam.ScreenToWorldPoint(screenPos);
            var hit2D = Physics2D.Raycast(origin, Vector3.forward, rayDistance, interactableMask);
            if (hit2D)
            {
                ih = new InteractorHit
                {
                    gameObject = hit2D.collider.gameObject,
                    transform = hit2D.collider.transform,
                    worldPoint = hit2D.point,
                    worldNormal = hit2D.normal,
                    // rawHit = hit2D // RaycastHit2D is a different type; omitted
                };
                return true;
            }
            else if (Physics.Raycast(cam.ScreenPointToRay(screenPos), out RaycastHit hit, rayDistance, interactableMask))
            {
                ih = new InteractorHit
                {
                    gameObject = hit.collider.gameObject,
                    transform = hit.collider.transform,
                    worldPoint = hit.point,
                    worldNormal = hit.normal,
                    rawHit = hit
                };
                return true;
            }
            return false;
        }

        Vector3 ScreenToWorldOnGround(Vector2 screenPos)
        {
            if (!cam) return Vector3.zero;
            Plane plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray ray = cam.ScreenPointToRay(screenPos);
            return plane.Raycast(ray, out float t) ? ray.GetPoint(t) : cam.transform.position + cam.transform.forward * 10f;
        }

        bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (EventSystem.current.IsPointerOverGameObject()) return true;
#endif
            for (int i = 0; i < Input.touchCount; i++)
                if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                    return true;

            return false;
        }

        bool IsPointerOverUI(int fingerId) =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);

        bool IsPointerOverUICurrent(Vector2 screenPos)
        {
            // Optional recheck using current pointer position; for mouse IsPointerOverGameObject() already uses current.
#if UNITY_EDITOR || UNITY_STANDALONE
            return IsPointerOverUI();
#else
        // Find touch matching position (approx). If not available, fall back to any.
        return IsPointerOverUI();
#endif
        }
    }
}