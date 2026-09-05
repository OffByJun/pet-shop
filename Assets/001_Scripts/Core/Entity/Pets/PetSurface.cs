using System.Collections.Generic;
using System.Runtime.InteropServices;
using _001_Scripts.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _001_Scripts.Core.Entity.Pets
{
    /// <summary>
    /// 펫 표면을 텍스처로 시뮬레이션합니다. 한 프레임에 쌓인 접촉을 컴퓨트 셰이더 한 번으로 처리하고,
    /// 구역별 평균도 GPU에서 낸 뒤 작은 버퍼만 CPU로 되읽습니다.
    /// 컴퓨트를 못 쓰는 기기에서는 블릿 방식으로 자동 대체합니다.
    /// </summary>
    public sealed class PetSurface : GameBehaviour
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ToolInteraction
        {
            public Vector2 Uv;
            public float Radius;
            public float Strength;
            public Vector2 Direction;
            public uint Type;
            public uint Pad;
            public const int Stride = 32;
        }

        [Header("Wiring")]
        [Tooltip("표면 상태를 입힐 파츠 이미지입니다. 비우면 자식에서 모두 찾습니다.")]
        [SerializeField] private Image[] parts = new Image[0];
        [SerializeField] private ComputeShader compute;
        [SerializeField] private Shader stampShader;
        [SerializeField] private Shader surfaceShader;

        [Header("Simulation")]
        [SerializeField, Min(64)] private int resolution = 256;
        [Tooltip("컴퓨트를 못 쓸 때 되읽기에 사용하는 축소 해상도입니다.")]
        [SerializeField, Min(4)] private int readbackResolution = 32;
        [Tooltip("몇 초마다 CPU로 요약값을 가져올지입니다.")]
        [SerializeField, Min(.02f)] private float readbackInterval = .05f;
        [SerializeField, Range(0f, 1f)] private float startingDirt = .75f;
        [SerializeField, Range(0f, 1f)] private float startingFurOrder = .25f;

        private RenderTexture surface;
        private RenderTexture blitBack;
        private RenderTexture readbackTarget;
        private Material stampMaterial;
        private Material surfaceMaterial;
        private ComputeBuffer toolBuffer;
        private ComputeBuffer zoneBuffer;
        private ComputeBuffer resultBuffer;
        private int clearKernel = -1;
        private int applyKernel = -1;
        private int reduceKernel = -1;

        private readonly List<Image> resolvedParts = new List<Image>();
        private readonly List<ToolInteraction> pending = new List<ToolInteraction>(64);
        private readonly List<Rect> zones = new List<Rect>();
        private readonly List<Vector4> zonePayload = new List<Vector4>();
        private Vector4[] zoneResults = new Vector4[1];
        private float[] dirtGrid;
        private int dirtGridSize;
        private float nextReadback;
        private bool readbackPending;
        private bool zonesDirty = true;

        private const int MaxTools = 256;
        private const int MaxZones = 32;

        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int ToolsId = Shader.PropertyToID("_Tools");
        private static readonly int ZonesId = Shader.PropertyToID("_Zones");
        private static readonly int ZoneResultsId = Shader.PropertyToID("_ZoneResults");
        private static readonly int ToolCountId = Shader.PropertyToID("_ToolCount");
        private static readonly int ZoneCountId = Shader.PropertyToID("_ZoneCount");
        private static readonly int ResolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int DeltaId = Shader.PropertyToID("_Delta");
        private static readonly int StartingDirtId = Shader.PropertyToID("_StartingDirt");
        private static readonly int StartingFurId = Shader.PropertyToID("_StartingFurOrder");
        private static readonly int SurfaceTexId = Shader.PropertyToID("_SurfaceTex");
        private static readonly int StampUvId = Shader.PropertyToID("_StampUV");
        private static readonly int StampDirId = Shader.PropertyToID("_StampDir");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");

        public PetSurfaceState State { get; private set; } = PetSurfaceState.Fresh;
        public bool IsReady => surface != null;
        /// <summary>컴퓨트 경로로 돌고 있는지입니다. false면 블릿으로 대체된 상태입니다.</summary>
        public bool UsesCompute { get; private set; }

        private void Awake() => Rebuild();

        private void OnDestroy() => Release();

        private void LateUpdate()
        {
            if (!IsReady) return;
            Flush();
            if (Time.unscaledTime < nextReadback || readbackPending) return;
            nextReadback = Time.unscaledTime + readbackInterval;
            RequestReadback();
        }

        public void ResetSurface()
        {
            if (!IsReady) Rebuild();
            if (!IsReady) return;
            pending.Clear();
            if (UsesCompute)
            {
                compute.SetTexture(clearKernel, SurfaceId, surface);
                compute.SetInt(ResolutionId, resolution);
                compute.SetFloat(StartingDirtId, startingDirt);
                compute.SetFloat(StartingFurId, startingFurOrder);
                compute.Dispatch(clearKernel, Groups(resolution), Groups(resolution), 1);
            }
            else
            {
                var previous = RenderTexture.active;
                var seed = new Color(startingDirt, 0f, startingFurOrder, 0f);
                RenderTexture.active = surface;
                GL.Clear(false, true, seed);
                RenderTexture.active = blitBack;
                GL.Clear(false, true, seed);
                RenderTexture.active = previous;
            }
            State = new PetSurfaceState(startingDirt, 0f, startingFurOrder, 0f);
            for (var i = 0; i < zoneResults.Length; i++)
                zoneResults[i] = new Vector4(startingDirt, 0f, startingFurOrder, 0f);
            dirtGrid = null;
            PushToParts();
        }

        /// <summary>접촉을 이번 프레임 목록에 쌓습니다. 실제 계산은 프레임 끝에 한 번에 합니다.</summary>
        public void Apply(ToolStamp stamp)
        {
            if (!IsReady) return;
            if (!UsesCompute) { BlitStamp(stamp); return; }
            if (pending.Count >= MaxTools) return;
            var direction = stamp.Direction.sqrMagnitude < 1e-6f ? Vector2.right : stamp.Direction.normalized;
            pending.Add(new ToolInteraction
            {
                Uv = stamp.Uv,
                Radius = stamp.Radius,
                Strength = stamp.Strength,
                Direction = direction,
                Type = (uint)stamp.Kind
            });
        }

        /// <summary>UV 사각형 안의 평균 오염입니다. 아직 값이 없으면 -1을 돌려줍니다.</summary>
        public float SampleDirt(Rect uvRect)
        {
            if (UsesCompute) return SampleZone(uvRect).x;
            if (dirtGrid == null || dirtGridSize <= 0) return -1f;
            var xMin = Mathf.Clamp(Mathf.FloorToInt(uvRect.xMin * dirtGridSize), 0, dirtGridSize - 1);
            var xMax = Mathf.Clamp(Mathf.CeilToInt(uvRect.xMax * dirtGridSize), xMin + 1, dirtGridSize);
            var yMin = Mathf.Clamp(Mathf.FloorToInt(uvRect.yMin * dirtGridSize), 0, dirtGridSize - 1);
            var yMax = Mathf.Clamp(Mathf.CeilToInt(uvRect.yMax * dirtGridSize), yMin + 1, dirtGridSize);
            var total = 0f;
            var count = 0;
            for (var y = yMin; y < yMax; y++)
            for (var x = xMin; x < xMax; x++)
            {
                total += dirtGrid[y * dirtGridSize + x];
                count++;
            }
            return count == 0 ? -1f : total / count;
        }

        /// <summary>구역을 등록해 두고 GPU가 낸 평균을 그대로 읽습니다.</summary>
        private Vector4 SampleZone(Rect uvRect)
        {
            var slot = FindZone(uvRect);
            if (slot < 0)
            {
                if (zones.Count >= MaxZones - 1) return new Vector4(-1f, 0f, 0f, 0f);
                zones.Add(uvRect);
                zonesDirty = true;
                // 등록 직후에는 아직 잰 값이 없으므로 전체 평균으로 답합니다.
                return zoneResults.Length > 0 ? zoneResults[0] : new Vector4(-1f, 0f, 0f, 0f);
            }
            var index = slot + 1; // 0번은 항상 표면 전체입니다.
            return index < zoneResults.Length ? zoneResults[index] : new Vector4(-1f, 0f, 0f, 0f);
        }

        /// <summary>
        /// 같은 부위를 매 프레임 다시 계산하면 부동소수 오차가 생기므로 근사 일치로 찾습니다.
        /// 정확 일치를 요구하면 호출할 때마다 새 슬롯이 잡혀 전체 평균이 돌아옵니다.
        /// </summary>
        private int FindZone(Rect uvRect)
        {
            const float tolerance = .002f;
            for (var i = 0; i < zones.Count; i++)
            {
                var zone = zones[i];
                if (Mathf.Abs(zone.xMin - uvRect.xMin) > tolerance) continue;
                if (Mathf.Abs(zone.yMin - uvRect.yMin) > tolerance) continue;
                if (Mathf.Abs(zone.width - uvRect.width) > tolerance) continue;
                if (Mathf.Abs(zone.height - uvRect.height) > tolerance) continue;
                return i;
            }
            return -1;
        }

        public bool TryUvFromStage(RectTransform stage, Rect stageZone, out Rect uvRect)
        {
            uvRect = default;
            var rect = transform as RectTransform;
            if (rect == null || stage == null) return false;
            if (!ToUv(stage, rect, new Vector2(stageZone.xMin, stageZone.yMax), out var lower)) return false;
            if (!ToUv(stage, rect, new Vector2(stageZone.xMax, stageZone.yMin), out var upper)) return false;
            uvRect = Rect.MinMaxRect(
                Mathf.Min(lower.x, upper.x), Mathf.Min(lower.y, upper.y),
                Mathf.Max(lower.x, upper.x), Mathf.Max(lower.y, upper.y));
            return true;
        }

        private static bool ToUv(RectTransform stage, RectTransform pet, Vector2 stageNormalized, out Vector2 uv)
        {
            var local = new Vector2(
                stage.rect.xMin + stageNormalized.x * stage.rect.width,
                stage.rect.yMin + (1f - stageNormalized.y) * stage.rect.height);
            var world = stage.TransformPoint(local);
            var petLocal = (Vector2)pet.InverseTransformPoint(world);
            var size = pet.rect.size;
            if (size.x <= 0f || size.y <= 0f) { uv = default; return false; }
            uv = new Vector2((petLocal.x - pet.rect.xMin) / size.x, (petLocal.y - pet.rect.yMin) / size.y);
            return true;
        }

        public bool TryUv(Vector2 screenPosition, Camera camera, out Vector2 uv)
        {
            uv = default;
            var rect = transform as RectTransform;
            if (rect == null) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, screenPosition, camera, out var local)) return false;
            var size = rect.rect.size;
            if (size.x <= 0f || size.y <= 0f) return false;
            uv = new Vector2((local.x - rect.rect.xMin) / size.x, (local.y - rect.rect.yMin) / size.y);
            return uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f;
        }

        // ---------- 내부 ----------

        private static int Groups(int size) => Mathf.Max(1, Mathf.CeilToInt(size / 8f));

        /// <summary>이번 프레임에 쌓인 접촉을 한 번의 디스패치로 처리합니다.</summary>
        private void Flush()
        {
            if (!UsesCompute) return;
            var hasWork = pending.Count > 0;
            if (hasWork) toolBuffer.SetData(pending, 0, 0, pending.Count);
            compute.SetTexture(applyKernel, SurfaceId, surface);
            compute.SetBuffer(applyKernel, ToolsId, toolBuffer);
            compute.SetInt(ToolCountId, pending.Count);
            compute.SetInt(ResolutionId, resolution);
            compute.SetFloat(DeltaId, Time.unscaledDeltaTime);
            compute.Dispatch(applyKernel, Groups(resolution), Groups(resolution), 1);
            pending.Clear();
            if (hasWork) PushToParts();
        }

        private void BlitStamp(ToolStamp stamp)
        {
            if (stampMaterial == null) return;
            stampMaterial.SetVector(StampUvId,
                new Vector4(stamp.Uv.x, stamp.Uv.y, stamp.Radius, stamp.Strength));
            var direction = stamp.Direction.sqrMagnitude < 1e-6f ? Vector2.right : stamp.Direction.normalized;
            stampMaterial.SetVector(StampDirId,
                new Vector4(direction.x, direction.y, (int)stamp.Kind, Time.unscaledDeltaTime));
            stampMaterial.SetFloat(AspectId, 1f);
            Graphics.Blit(surface, blitBack, stampMaterial);
            (surface, blitBack) = (blitBack, surface);
            PushToParts();
        }

        private void RequestReadback()
        {
            readbackPending = true;
            if (!UsesCompute)
            {
                Graphics.Blit(surface, readbackTarget);
                AsyncGPUReadback.Request(readbackTarget, 0, TextureFormat.RGBA32, OnGridReadback);
                return;
            }
            RefreshZoneBuffer();
            compute.SetTexture(reduceKernel, SurfaceId, surface);
            compute.SetBuffer(reduceKernel, ZonesId, zoneBuffer);
            compute.SetBuffer(reduceKernel, ZoneResultsId, resultBuffer);
            compute.SetInt(ZoneCountId, zones.Count + 1);
            compute.SetInt(ResolutionId, resolution);
            compute.Dispatch(reduceKernel, 1, 1, 1);
            AsyncGPUReadback.Request(resultBuffer, OnZoneReadback);
        }

        private void RefreshZoneBuffer()
        {
            if (!zonesDirty) return;
            zonesDirty = false;
            zonePayload.Clear();
            zonePayload.Add(new Vector4(0f, 0f, 1f, 1f)); // 0번 = 표면 전체
            for (var i = 0; i < zones.Count; i++)
                zonePayload.Add(new Vector4(zones[i].xMin, zones[i].yMin, zones[i].width, zones[i].height));
            while (zonePayload.Count < MaxZones) zonePayload.Add(Vector4.zero);
            zoneBuffer.SetData(zonePayload);
        }

        private void OnZoneReadback(AsyncGPUReadbackRequest request)
        {
            readbackPending = false;
            if (request.hasError || !IsReady) return;
            var data = request.GetData<Vector4>();
            if (data.Length == 0) return;
            if (zoneResults.Length < data.Length) zoneResults = new Vector4[data.Length];
            for (var i = 0; i < data.Length; i++) zoneResults[i] = data[i];
            var whole = zoneResults[0];
            State = new PetSurfaceState(whole.x, whole.y, whole.z, whole.w);
        }

        private void OnGridReadback(AsyncGPUReadbackRequest request)
        {
            readbackPending = false;
            if (request.hasError || !IsReady) return;
            var data = request.GetData<Color32>();
            if (data.Length == 0) return;
            float dirt = 0f, wet = 0f, fur = 0f, foam = 0f;
            for (var i = 0; i < data.Length; i++)
            {
                dirt += data[i].r; wet += data[i].g; fur += data[i].b; foam += data[i].a;
            }
            var scale = 1f / (data.Length * 255f);
            State = new PetSurfaceState(dirt * scale, wet * scale, fur * scale, foam * scale);

            if (dirtGrid == null || dirtGrid.Length != data.Length)
            {
                dirtGrid = new float[data.Length];
                dirtGridSize = readbackResolution;
            }
            for (var i = 0; i < data.Length; i++) dirtGrid[i] = data[i].r / 255f;
        }

        private void Rebuild()
        {
            Release();
            if (surfaceShader == null) surfaceShader = Shader.Find("PetShop/PetSurfaceUI");
            if (stampShader == null) stampShader = Shader.Find("PetShop/PetSurfaceStamp");
            if (compute == null) compute = Resources.Load<ComputeShader>("Shaders/PetSurfaceCompute");
            if (surfaceShader == null)
            {
                Debug.LogWarning("PetSurface: the surface shader is missing; the coat stays flat.", this);
                return;
            }
            surfaceMaterial = new Material(surfaceShader) { hideFlags = HideFlags.HideAndDontSave };

            UsesCompute = compute != null && SystemInfo.supportsComputeShaders;
            if (UsesCompute)
            {
                clearKernel = compute.FindKernel("CSClear");
                applyKernel = compute.FindKernel("CSApplyTools");
                reduceKernel = compute.FindKernel("CSReduce");
                toolBuffer = new ComputeBuffer(MaxTools, ToolInteraction.Stride);
                zoneBuffer = new ComputeBuffer(MaxZones, sizeof(float) * 4);
                resultBuffer = new ComputeBuffer(MaxZones, sizeof(float) * 4);
                zoneResults = new Vector4[MaxZones];
            }
            else
            {
                if (stampShader == null)
                {
                    Debug.LogWarning("PetSurface: no compute and no stamp shader; the coat stays flat.", this);
                    return;
                }
                stampMaterial = new Material(stampShader) { hideFlags = HideFlags.HideAndDontSave };
                blitBack = Create(resolution, false);
                readbackTarget = Create(readbackResolution, false);
            }

            surface = Create(resolution, UsesCompute);

            resolvedParts.Clear();
            if (parts != null && parts.Length > 0) resolvedParts.AddRange(parts);
            else GetComponentsInChildren(true, resolvedParts);

            zones.Clear();
            zonesDirty = true;
            ResetSurface();
        }

        private RenderTexture Create(int size, bool randomWrite)
        {
            var texture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "PetSurface " + size,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                enableRandomWrite = randomWrite,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.Create();
            return texture;
        }

        private void PushToParts()
        {
            if (surfaceMaterial == null) return;
            surfaceMaterial.SetTexture(SurfaceTexId, surface);
            for (var i = 0; i < resolvedParts.Count; i++)
            {
                var part = resolvedParts[i];
                if (part == null) continue;
                if (part.material != surfaceMaterial) part.material = surfaceMaterial;
            }
        }

        private void Release()
        {
            if (surface != null) { surface.Release(); DestroyImmediate(surface); surface = null; }
            if (blitBack != null) { blitBack.Release(); DestroyImmediate(blitBack); blitBack = null; }
            if (readbackTarget != null) { readbackTarget.Release(); DestroyImmediate(readbackTarget); readbackTarget = null; }
            if (stampMaterial != null) { DestroyImmediate(stampMaterial); stampMaterial = null; }
            if (surfaceMaterial != null) { DestroyImmediate(surfaceMaterial); surfaceMaterial = null; }
            toolBuffer?.Release(); toolBuffer = null;
            zoneBuffer?.Release(); zoneBuffer = null;
            resultBuffer?.Release(); resultBuffer = null;
        }

        public void Configure(Image[] targets) => parts = targets;
    }
}
