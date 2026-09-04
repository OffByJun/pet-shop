namespace _001_Scripts.Core.Composition
{
    /// <summary>호스트에 조합되어 기능을 채우는 조각입니다.</summary>
    public interface IModule { }

    /// <summary>실행 순서가 중요한 조각. 값이 작을수록 먼저입니다.</summary>
    public interface IOrderedModule : IModule { int Order { get; } }
}
