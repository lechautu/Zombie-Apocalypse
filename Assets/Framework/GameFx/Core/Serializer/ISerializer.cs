namespace GameFx.Core.Serializer
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string data);

        object Deserialize(string data, System.Type type);
    }
}