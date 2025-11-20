using System;
using System.Collections.Generic;
using System.IO;
using GameFx.Core.Serializer;
using GameFx.Core.Crypto;
using UnityEngine;
using GameFx.Core;
using Logger = GameFx.Core.Log.Logger;
using GameFx.Core.Log;

namespace GameFx.Features.SaveSystem
{
    public sealed class SaveLoader : ISaveLoader
    {
        public string GetSaveFilePath() => Path.Combine(Application.persistentDataPath, "profile.sav");

        readonly ISerializer _serializer;
        readonly ICrypto _crypto;

        readonly List<ISaveStateProvider> _providers = new();

        public SaveLoader(ISerializer serializer, ICrypto crypto)
        {
            _serializer = serializer;
            _crypto = crypto;
        }

        public void RegisterStateProvider(ISaveStateProvider provider)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public Result<bool> LoadProfile()
        {
            if (!File.Exists(GetSaveFilePath()))
            {
                Logger.Log("No save file found to load.", LogLevel.Warning);
                return Result<bool>.Failure("Save file does not exist.");
            }

            var encryptedEntries = File.ReadAllText(GetSaveFilePath());
            var serializedEntries = _crypto.Decrypt(encryptedEntries);
            var entries = _serializer.Deserialize<SerializableEntry[]>(serializedEntries);
            foreach (var entry in entries)
            {
                var type = Type.GetType(entry.TypeName);
                if (type == null)
                {
                    Logger.Log($"Could not find type {entry.TypeName} during load.", LogLevel.Error);
                    continue;
                }

                var state = _serializer.Deserialize(entry.Value, type);

                foreach (var provider in _providers)
                {
                    if (provider.GetType().GetMethod("ApplyState").GetParameters()[0].ParameterType == type)
                    {
                        provider.ApplyState(state);
                        break;
                    }
                }
            }

            return Result<bool>.Success(true);
        }

        public Result<bool> SaveProfile()
        {
            SerializableEntry[] entries = new SerializableEntry[_providers.Count];
            for (int i = 0; i < _providers.Count; i++)
            {
                var provider = _providers[i];
                var state = provider.PopulateState();
                var serializedState = _serializer.Serialize(state);

                entries[i] = new()
                {
                    Value = serializedState,
                    TypeName = state.GetType().AssemblyQualifiedName
                };
            }
            var serializedEntries = _serializer.Serialize(entries);
            var encryptedEntries = _crypto.Encrypt(serializedEntries);
            File.WriteAllText(GetSaveFilePath(), encryptedEntries);
            return Result<bool>.Success(true);
        }
    }
}