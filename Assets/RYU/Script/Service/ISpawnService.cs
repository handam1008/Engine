using RYU.Script.Charcter;

namespace RYU.Script.Serivce
{
    public interface ISpawnService
    {
        bool TrySpawn(CharacterDataSO data);
    }
}