namespace GameFx.Features.RemoteConfig
{
    public sealed class LocalRemoteConfig : IRemoteConfig
    {
        readonly RemoteConfigBundle _bundle;
        
        string GetFilePath() => System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "remote_config");

        public LocalRemoteConfig(RemoteConfigBundle bundled)
        {
            _bundle = bundled ?? new RemoteConfigBundle();
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            object value = defaultValue;
            if (typeof(T) == typeof(string) && _bundle.Strings.TryGetValue(key, out var s)) value = s;
            else if (typeof(T) == typeof(double) && _bundle.Numbers.TryGetValue(key, out var n)) value = n;
            else if (typeof(T) == typeof(bool) && _bundle.Bools.TryGetValue(key, out var b)) value = b;
            else if (_bundle.Objects.TryGetValue(key, out var o)) value = o;
            return (T)value;
        }

        public void FetchConfigs()
        {
            
        }
    }
}