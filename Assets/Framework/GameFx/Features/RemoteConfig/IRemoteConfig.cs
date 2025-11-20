namespace GameFx.Features.RemoteConfig
{
    public interface IRemoteConfig
    {
        T GetValue<T>(string key, T defaultValue);
        void FetchConfigs();
    }
}