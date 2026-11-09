using System;

namespace RYU.Script.Service
{
    public interface IStorageService
    {
        int CurrentStorage { get; }
        int MaxStorage { get; }
        event Action OnChanged;
        bool AddStorage(int kb);
        void MinusStorage(int kb);
    }
}