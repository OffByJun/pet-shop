namespace _001_Scripts.Data.Items
{
    /// <summary>동물 케어/퀘스트/보상 시스템이 의존할 최소 계약입니다.</summary>
    public interface IItemAcquisitionService { bool TryGrant(ItemBase item, int amount); }
}
