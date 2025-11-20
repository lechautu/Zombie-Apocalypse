namespace GameFx.Features.SaveSystem
{
    public interface ISaveStateProvider
    {
        object PopulateState();
        void ApplyState(object data);
    }
}