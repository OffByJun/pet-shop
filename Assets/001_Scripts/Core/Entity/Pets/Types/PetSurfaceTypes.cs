using UnityEngine;

namespace _001_Scripts.Core.Entity.Pets
{
    /// <summary>도구가 표면에 남기는 한 번의 자국입니다.</summary>
    public enum SurfaceToolKind
    {
        Water = 0,
        Soap = 1,
        Brush = 2,
        Towel = 3,
        Trim = 4
    }

    /// <summary>표면 시뮬레이션에 보내는 한 번의 접촉입니다. UV는 펫 프레임 기준 0~1입니다.</summary>
    public readonly struct ToolStamp
    {
        public Vector2 Uv { get; }
        public float Radius { get; }
        public float Strength { get; }
        /// <summary>브러시가 움직인 방향입니다. 털 정돈 방향을 정합니다.</summary>
        public Vector2 Direction { get; }
        public SurfaceToolKind Kind { get; }

        public ToolStamp(Vector2 uv, float radius, float strength, Vector2 direction, SurfaceToolKind kind)
        {
            Uv = uv;
            Radius = Mathf.Max(.001f, radius);
            Strength = Mathf.Clamp01(strength);
            Direction = direction;
            Kind = kind;
        }
    }

    /// <summary>
    /// GPU에서 되읽은 표면 요약입니다. CPU 로직은 텍스처가 아니라 이 값만 봅니다.
    /// </summary>
    public readonly struct PetSurfaceState
    {
        /// <summary>남은 오염입니다. 0이면 깨끗합니다.</summary>
        public float Dirt { get; }
        /// <summary>젖은 정도입니다.</summary>
        public float Wetness { get; }
        /// <summary>털 정돈도입니다. 1이면 결이 한 방향으로 정리된 상태입니다.</summary>
        public float FurOrder { get; }
        /// <summary>거품입니다.</summary>
        public float Foam { get; }

        public PetSurfaceState(float dirt, float wetness, float furOrder, float foam)
        {
            Dirt = dirt;
            Wetness = wetness;
            FurOrder = furOrder;
            Foam = foam;
        }

        public float Cleanliness => Mathf.Clamp01(1f - Dirt);
        public float Dryness => Mathf.Clamp01(1f - Wetness);

        public static PetSurfaceState Fresh => new PetSurfaceState(0f, 0f, 1f, 0f);
    }
}
