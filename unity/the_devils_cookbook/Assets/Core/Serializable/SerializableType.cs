using System;
using System.IO;
using UnityEngine;

namespace TDC.Serializable
{
    [System.Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        public Type Type;
        public byte[] Data;

        public SerializableType(System.Type aType)
        {
            Type = aType;
        }

        public static System.Type Read(BinaryReader aReader)
        {
            byte paramCount = aReader.ReadByte();
            if (paramCount == 0xFF)
                return null;
            string typeName = aReader.ReadString();
            var type = System.Type.GetType(typeName);
            if (type == null)
                throw new System.Exception("Can't find type; '" + typeName + "'");
            if (!type.IsGenericTypeDefinition || paramCount <= 0) return type;
            var p = new System.Type[paramCount];
            for (var i = 0; i < paramCount; i++)
            {
                p[i] = Read(aReader);
            }
            type = type.MakeGenericType(p);
            return type;
        }

        public static void Write(BinaryWriter aWriter, System.Type aType)
        {
            if (aType == null)
            {
                aWriter.Write((byte)0xFF);
                return;
            }
            if (aType.IsGenericType)
            {
                var t = aType.GetGenericTypeDefinition();
                var p = aType.GetGenericArguments();
                aWriter.Write((byte)p.Length);
                aWriter.Write(t.AssemblyQualifiedName ?? string.Empty);
                foreach (Type type in p)
                {
                    Write(aWriter, type);
                }
                return;
            }
            aWriter.Write((byte)0);
            aWriter.Write(aType.AssemblyQualifiedName ?? string.Empty);
        }

        public void OnBeforeSerialize()
        {
            using var stream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(stream);
            Write(binaryWriter, Type);
            Data = stream.ToArray();
        }

        public void OnAfterDeserialize()
        {
            using var stream = new MemoryStream(Data);
            using var r = new BinaryReader(stream);
            Type = Read(r);
        }
    }
}