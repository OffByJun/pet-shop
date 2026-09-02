namespace _001_Scripts.Data.Pets
{
    public interface IPetCareService
    {
        bool TryCare(PetInstance pet, PetCareAction action, out PetCareResult result);
    }
}
