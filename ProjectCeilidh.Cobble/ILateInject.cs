using System;
namespace ProjectCeilidh.Cobble
{
    public interface ILateInject<T>
    {
        void UnitLoaded(T unit);
    }
}
