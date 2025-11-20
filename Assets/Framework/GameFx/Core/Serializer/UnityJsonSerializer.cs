using UnityEngine;

namespace GameFx.Core.Serializer
{
    public sealed class UnityJsonSerializer : ISerializer
    {
        public T Deserialize<T>(string data)
        {
            return JsonUtility.FromJson<T>(data);
        }

        public string Serialize<T>(T obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public object Deserialize(string data, System.Type type)
        {
            return JsonUtility.FromJson(data, type);
        }
    }
}