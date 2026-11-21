using RYU.Script.Charcter;

namespace RYU.Script.Service.SpawnService
{
    public interface ISpawnService
    {
        bool TrySpawn(CharacterDataSO data);
    }
}