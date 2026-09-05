using System.Collections.Generic;
using _001_Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Components
{
    /// <summary>Runtime-built care HUD and pooled uGUI particles, safe for overlay canvases.</summary>
    public sealed class CareFeedbackVfx : MonoBehaviour
    {
        private sealed class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Born;
            public float Life;
            public float Spin;
            public float StartScale;
        }

        private sealed class FloatingLabel
        {
            public RectTransform Rect;
            public Text Text;
            public float Born;
            public float Life;
        }

        [Tooltip("색과 글꼴을 화면 테마에서 가져옵니다. 비우면 기본값을 씁니다.")]
        [SerializeField] private _001_Scripts.UI.Theme.UITheme theme;

        /// <summary>런타임에 붙는 컴포넌트라 소유자가 테마를 넘겨 줍니다.</summary>
        public void SetTheme(_001_Scripts.UI.Theme.UITheme value) => theme = value;

        private readonly List<Particle> particles = new List<Particle>(40);
        private readonly List<FloatingLabel> labels = new List<FloatingLabel>(6);
        private readonly Color[] sparkleColors =
        {
            new Color32(255, 213, 91, 255),
            new Color32(174, 232, 207, 255),
            new Color32(255, 177, 157, 255),
            Color.white
        };

        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform stage;
        private RectTransform layer;
        private RectTransform hud;
        private RectTransform meterFill;
        private RectTransform bondFill;
        private Image hudBackground;
        private Text comboText;
        private Text feverText;
        private Text bondText;
        private Sprite sparkleSprite;
        private float nextBurstTime;
        private float nextFailureTime;
        private float nextInspectionTime;
        private bool initialized;

        public void Initialize(RectTransform stageRoot)
        {
            if (initialized) return;
            initialized = true;
            stage = stageRoot;
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            canvasRect = canvas.transform as RectTransform;
            BuildLayer();
            BuildHud();
            WarmPools();
        }

        public void Render(CareFlowState flow, PetBondState bond = null)
        {
            if (!initialized || hud == null || flow == null) return;
            hud.gameObject.SetActive(true);
            comboText.text = flow.Combo > 0 ? $"FLOW  x{flow.Combo}" : "FLOW  READY";
            feverText.text = flow.IsFever
                ? $"FEVER  {flow.FeverRemaining:0.0}s  ·  x{flow.ProgressMultiplier:0.00}"
                : $"연속 케어로 피버  ·  x{flow.ProgressMultiplier:0.00}";
            meterFill.anchorMax = new Vector2(flow.Meter, 1f);
            if (bond != null)
            {
                bondText.text = $"{bond.TemperamentLabel}  ·  신뢰 {Mathf.RoundToInt(bond.Trust)}  ·  {bond.MoodLabel}";
                bondFill.anchorMax = new Vector2(bond.Trust01, 1f);
                bondFill.GetComponent<Image>().color = bond.Trust >= 72f
                    ? new Color32(255, 177, 157, 255)
                    : bond.Trust >= 50f ? new Color32(174, 232, 207, 255) : new Color32(238, 169, 67, 255);
            }
            hudBackground.color = flow.IsFever
                ? (Color)new Color32(83, 71, 53, 245)
                : theme != null ? new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .94f)
                : (Color)new Color32(30, 48, 61, 235);
            var pulse = flow.IsFever ? 1f + Mathf.Sin(Time.unscaledTime * 8f) * .025f : 1f;
            hud.localScale = Vector3.one * pulse;
        }

        public void Emit(Vector2 screenPosition, CareInteractionStatus status,
            CareFlowFeedback feedback, CareFlowState flow)
        {
            if (!initialized || layer == null || flow == null) return;
            var now = Time.unscaledTime;
            var failed = status == CareInteractionStatus.WrongTool || status == CareInteractionStatus.NeedsWater;
            if (failed)
            {
                if (now < nextFailureTime) return;
                nextFailureTime = now + .28f;
                SpawnLabel(screenPosition, feedback == CareFlowFeedback.Broken ? "FLOW BREAK" : "도구 확인", new Color32(226, 111, 97, 255));
                return;
            }

            if (now < nextBurstTime && feedback == CareFlowFeedback.None) return;
            nextBurstTime = now + .055f;
            var count = feedback switch
            {
                CareFlowFeedback.Fever => 14,
                CareFlowFeedback.Resolved => 10,
                CareFlowFeedback.Perfect => 7,
                CareFlowFeedback.Great => 5,
                _ => status == CareInteractionStatus.StageCompleted ? 9 : 3
            };
            SpawnBurst(screenPosition, count, flow.IsFever ? 1.25f : 1f);

            switch (feedback)
            {
                case CareFlowFeedback.Good:
                    SpawnLabel(screenPosition, "GOOD!", new Color32(107, 178, 151, 255));
                    break;
                case CareFlowFeedback.Great:
                    SpawnLabel(screenPosition, "GREAT!", new Color32(238, 169, 67, 255));
                    break;
                case CareFlowFeedback.Perfect:
                    SpawnLabel(screenPosition, "PERFECT!", new Color32(225, 125, 93, 255));
                    break;
                case CareFlowFeedback.Fever:
                    SpawnLabel(screenPosition, "FEVER!", new Color32(255, 190, 63, 255), 34);
                    SpawnBurst(StageCenterScreenPoint(), 18, 1.5f);
                    break;
                case CareFlowFeedback.Resolved:
                    SpawnLabel(screenPosition, "CLEAN!", new Color32(91, 169, 203, 255), 30);
                    break;
            }
            if (status == CareInteractionStatus.StageCompleted)
                SpawnLabel(screenPosition, "단계 완료!", new Color32(105, 192, 166, 255), 27);
        }

        public void EmitInspection(Vector2 screenPosition, bool discovered)
        {
            if (!initialized || layer == null || Time.unscaledTime < nextInspectionTime) return;
            nextInspectionTime = Time.unscaledTime + .18f;
            if (discovered)
            {
                SpawnBurst(screenPosition, 10, 1.15f);
                SpawnLabel(screenPosition, "증상 발견!", new Color32(131, 117, 181, 255), 30);
            }
            else
            {
                SpawnLabel(screenPosition, "이상 없음", new Color32(112, 124, 128, 220), 22);
            }
        }

        private void Update()
        {
            if (!initialized) return;
            var now = Time.unscaledTime;
            for (var i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];
                if (!particle.Rect.gameObject.activeSelf) continue;
                var t = (now - particle.Born) / particle.Life;
                if (t >= 1f)
                {
                    particle.Rect.gameObject.SetActive(false);
                    continue;
                }
                particle.Rect.anchoredPosition += particle.Velocity * Time.unscaledDeltaTime;
                particle.Velocity += Vector2.down * (120f * Time.unscaledDeltaTime);
                particle.Rect.localRotation *= Quaternion.Euler(0f, 0f, particle.Spin * Time.unscaledDeltaTime);
                var scale = particle.StartScale * Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
                particle.Rect.localScale = Vector3.one * scale;
                var color = particle.Image.color;
                color.a = Mathf.Clamp01((1f - t) * 1.8f);
                particle.Image.color = color;
            }

            for (var i = 0; i < labels.Count; i++)
            {
                var label = labels[i];
                if (!label.Rect.gameObject.activeSelf) continue;
                var t = (now - label.Born) / label.Life;
                if (t >= 1f)
                {
                    label.Rect.gameObject.SetActive(false);
                    continue;
                }
                label.Rect.anchoredPosition += Vector2.up * (42f * Time.unscaledDeltaTime);
                var color = label.Text.color;
                color.a = 1f - Mathf.Clamp01((t - .55f) / .45f);
                label.Text.color = color;
                label.Rect.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, Mathf.Clamp01(t * 5f));
            }
        }

        private void BuildLayer()
        {
            var go = new GameObject("Care Feedback VFX", typeof(RectTransform));
            layer = (RectTransform)go.transform;
            layer.SetParent(canvasRect, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;
            layer.SetAsLastSibling();
        }

        private void BuildHud()
        {
            var root = new GameObject("Flow HUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hud = (RectTransform)root.transform;
            hud.SetParent(layer, false);
            hud.anchorMin = new Vector2(.5f, 1f);
            hud.anchorMax = new Vector2(.5f, 1f);
            hud.pivot = new Vector2(.5f, 1f);
            hud.anchoredPosition = new Vector2(0f, -26f);
            hud.sizeDelta = new Vector2(430f, 92f);
            hudBackground = root.GetComponent<Image>();
            hudBackground.color = theme != null
                ? new Color(theme.Ink.r, theme.Ink.g, theme.Ink.b, .94f)
                : (Color)new Color32(30, 48, 61, 235);
            hudBackground.raycastTarget = false;

            var meterTrack = CreateImage("Meter Track", hud, new Color32(255, 255, 255, 35));
            meterTrack.anchorMin = new Vector2(.04f, .42f);
            meterTrack.anchorMax = new Vector2(.96f, .50f);
            meterTrack.offsetMin = Vector2.zero;
            meterTrack.offsetMax = Vector2.zero;
            meterFill = CreateImage("Meter Fill", meterTrack, theme != null ? theme.Gold : (Color)new Color32(255, 201, 75, 255));
            meterFill.anchorMin = Vector2.zero;
            meterFill.anchorMax = new Vector2(0f, 1f);
            meterFill.offsetMin = Vector2.zero;
            meterFill.offsetMax = Vector2.zero;

            comboText = CreateText("Combo", hud, 22, TextAnchor.MiddleLeft, FontStyle.Bold);
            comboText.rectTransform.anchorMin = new Vector2(.05f, .52f);
            comboText.rectTransform.anchorMax = new Vector2(.48f, .95f);
            comboText.rectTransform.offsetMin = Vector2.zero;
            comboText.rectTransform.offsetMax = Vector2.zero;
            feverText = CreateText("Fever", hud, 13, TextAnchor.MiddleRight, FontStyle.Normal);
            feverText.rectTransform.anchorMin = new Vector2(.38f, .52f);
            feverText.rectTransform.anchorMax = new Vector2(.95f, .95f);
            feverText.rectTransform.offsetMin = Vector2.zero;
            feverText.rectTransform.offsetMax = Vector2.zero;

            bondText = CreateText("Pet Bond", hud, 13, TextAnchor.MiddleCenter, FontStyle.Bold);
            bondText.rectTransform.anchorMin = new Vector2(.04f, .10f);
            bondText.rectTransform.anchorMax = new Vector2(.96f, .36f);
            bondText.rectTransform.offsetMin = Vector2.zero;
            bondText.rectTransform.offsetMax = Vector2.zero;
            var bondTrack = CreateImage("Bond Track", hud, new Color32(255, 255, 255, 35));
            bondTrack.anchorMin = new Vector2(.04f, .035f);
            bondTrack.anchorMax = new Vector2(.96f, .10f);
            bondTrack.offsetMin = Vector2.zero;
            bondTrack.offsetMax = Vector2.zero;
            bondFill = CreateImage("Bond Fill", bondTrack, new Color32(174, 232, 207, 255));
            bondFill.anchorMin = Vector2.zero;
            bondFill.anchorMax = new Vector2(.5f, 1f);
            bondFill.offsetMin = Vector2.zero;
            bondFill.offsetMax = Vector2.zero;
        }

        private void WarmPools()
        {
            var texture = Resources.Load<Texture2D>("UI/VFX/care-paw-sparkle");
            if (texture != null)
                sparkleSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(.5f, .5f), 100f);
            for (var i = 0; i < 40; i++) particles.Add(CreateParticle(i));
            for (var i = 0; i < 6; i++) labels.Add(CreateLabel(i));
        }

        private Particle CreateParticle(int index)
        {
            var go = new GameObject($"Sparkle {index:00}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(layer, false);
            rect.sizeDelta = new Vector2(52f, 52f);
            var image = go.GetComponent<Image>();
            image.sprite = sparkleSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            go.SetActive(false);
            return new Particle { Rect = rect, Image = image };
        }

        private FloatingLabel CreateLabel(int index)
        {
            var text = CreateText($"Feedback {index:00}", layer, 25, TextAnchor.MiddleCenter, FontStyle.Bold);
            text.rectTransform.sizeDelta = new Vector2(240f, 54f);
            text.gameObject.SetActive(false);
            return new FloatingLabel { Rect = text.rectTransform, Text = text };
        }

        private void SpawnBurst(Vector2 screenPosition, int count, float strength)
        {
            var position = ToCanvasPoint(screenPosition);
            for (var i = 0; i < count; i++)
            {
                var particle = FindParticle();
                if (particle == null) break;
                var angle = Random.Range(35f, 145f) * Mathf.Deg2Rad;
                var speed = Random.Range(75f, 180f) * strength;
                particle.Rect.anchoredPosition = position + Random.insideUnitCircle * 14f;
                particle.Rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                particle.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                particle.Born = Time.unscaledTime;
                particle.Life = Random.Range(.45f, .75f);
                particle.Spin = Random.Range(-230f, 230f);
                particle.StartScale = Random.Range(.45f, .85f) * strength;
                particle.Image.color = sparkleColors[Random.Range(0, sparkleColors.Length)];
                particle.Rect.gameObject.SetActive(true);
            }
        }

        private void SpawnLabel(Vector2 screenPosition, string value, Color color, int size = 25)
        {
            var label = FindLabel();
            if (label == null) return;
            label.Rect.anchoredPosition = ToCanvasPoint(screenPosition) + Vector2.up * 34f;
            label.Rect.localScale = Vector3.one * 1.25f;
            label.Text.text = value;
            label.Text.fontSize = size;
            label.Text.color = color;
            label.Born = Time.unscaledTime;
            label.Life = .8f;
            label.Rect.gameObject.SetActive(true);
            label.Rect.SetAsLastSibling();
        }

        private Particle FindParticle()
        {
            for (var i = 0; i < particles.Count; i++)
                if (!particles[i].Rect.gameObject.activeSelf) return particles[i];
            return null;
        }

        private FloatingLabel FindLabel()
        {
            for (var i = 0; i < labels.Count; i++)
                if (!labels[i].Rect.gameObject.activeSelf) return labels[i];
            return null;
        }

        private Vector2 ToCanvasPoint(Vector2 screenPosition)
        {
            if (screenPosition == Vector2.zero) screenPosition = StageCenterScreenPoint();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out var localPoint);
            return localPoint;
        }

        private Vector2 StageCenterScreenPoint()
        {
            if (stage == null) return new Vector2(Screen.width * .5f, Screen.height * .5f);
            return RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, stage.position);
        }

        private static RectTransform CreateImage(string objectName, Transform parent, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private Text CreateText(string objectName, Transform parent, int size, TextAnchor anchor, FontStyle style)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var text = go.GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = theme != null ? theme.DisplayFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = new Color32(247, 244, 235, 255);
            text.raycastTarget = false;
            return text;
        }

        private void OnDestroy()
        {
            if (sparkleSprite != null) Destroy(sparkleSprite);
        }
    }
}
