namespace _001_Scripts.Core.Pipes
{
    /// <summary>
    /// 게임 실행 전체에서 유지되는 전역 파이프의 기본 신호입니다.
    /// </summary>
    public readonly struct GamePipeMessage
    {
    }

    /// <summary>
    /// 인게임 진입부터 종료까지 유지되는 파이프의 기본 신호입니다.
    /// </summary>
    public readonly struct InGamePipeMessage
    {
    }

    /// <summary>
    /// Core 내부 구현에서만 사용하는 기본 신호입니다.
    /// </summary>
    internal readonly struct InternalPipeMessage
    {
    }
}
