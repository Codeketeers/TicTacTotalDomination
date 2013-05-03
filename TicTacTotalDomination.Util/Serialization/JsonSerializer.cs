using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TicTacTotalDomination.Util.Serialization
{
    public class JsonSerializer
    {
        public static string SerializeToJSON<T>(T data)
            where T : class
        {
            var dataStream = new MemoryStream();
            var dataSerializer = new DataContractJsonSerializer(typeof(T));
            dataSerializer.WriteObject(dataStream, data);
            byte[] dataBytes = dataStream.ToArray();
            dataStream.Close();
            string result = Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);

            return result;
        }

        public static T DeseriaizeFromJSON<T>(string json)
            where T : class
        {
            var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var dataSerializer = new DataContractJsonSerializer(typeof(T));
            dataStream.Position = 0;
            T result = dataSerializer.ReadObject(dataStream) as T;

            return result;
        }
    }
}
