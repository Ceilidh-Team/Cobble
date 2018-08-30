namespace ProjectCeilidh.Cobble
{
    public interface ILateInject<in T>
    {
        void UnitLoaded(T unit);
    }
}
