using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Verse;

namespace RimElShrine.Data
{
    public static class DataContractSerializeHelper
    {
        public static bool TryWrite(Type classType, object instance, string path, IEnumerable<Type>? knownTypes = null)
        {
            try
            {
                knownTypes ??= [];
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  "
                };
                $"Writing serialization files, path={path}".Msg();
                using var writer = XmlWriter.Create(path, settings);
                var serializer = new DataContractSerializer(classType, knownTypes);
                serializer.WriteObject(writer, instance);
                return true;
            }
            catch (Exception ex)
            {
                $"Failed to serialize {classType.Name} to {path}: {ex}".Error();
                return false;
            }
        }

        public static bool TryRead(Type classType, out object? result, string path, IEnumerable<Type>? knownTypes = null)
        {
            knownTypes ??= [];
            result = null;
            try
            {
                if (!File.Exists(path))
                {
                    Log.Warning($"File not found: {path}");
                    return false;
                }
                using var stream = new FileStream(path, FileMode.Open);
                using var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                var serializer = new DataContractSerializer(classType, knownTypes);
                result = serializer.ReadObject(reader, true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to deserialize {classType.Name} from {path}: {ex}");
                return false;
            }
        }
    }
}
