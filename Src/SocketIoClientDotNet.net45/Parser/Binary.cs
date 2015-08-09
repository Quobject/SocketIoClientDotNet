using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Quobject.SocketIoClientDotNet.Parser
{
    public class Binary
    {
        private static readonly string KEY_PLACEHOLDER = "_placeholder";

        private static readonly string KEY_NUM = "num";

        public static DeconstructedPacket DeconstructPacket(Packet packet)
        {
            var buffers = new List<byte[]>();

            packet.Data = _deconstructPacket(packet.Data, buffers);
            packet.Attachments = buffers.Count;

            var result = new DeconstructedPacket();
            result.Packet = packet;
            result.Buffers = buffers.ToArray();
            return result;
        }

        private static JToken _deconstructPacket(object data, List<byte[]> buffers)
        {
            if (data == null) return null;

            if (data is byte[])
            {
                var byteArray = (byte[]) data;
                return AddPlaceholder(buffers, byteArray);
            }
            if (data is JArray)
            {
                var newData = new JArray();
                var _data = (JArray) data;
                int len = _data.Count;
                for (int i = 0; i < len; i ++)
                {
                    try
                    {
                        newData.Add( _deconstructPacket(_data[i], buffers));                        
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return newData;
            }
            if (!(data is JToken))
            {
                throw new NotImplementedException();
            }
            var jtoken = (JToken) data;
            if (jtoken.Type == JTokenType.String)
            {
                return jtoken.Value<string>();
            }
            else if (jtoken.Type == JTokenType.Bytes)
            {
                var byteArray = jtoken.Value<byte[]>();
                return AddPlaceholder(buffers, byteArray);
            }
            else if (jtoken.Type == JTokenType.Object)
            {

                var newData2 = new JObject();
                var _data2 = (JObject)jtoken;

                foreach (var property in _data2.Properties())
                {
                    try
                    {
                        newData2[property.Name] = _deconstructPacket(property.Value, buffers);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                }
                return newData2;
            }
            throw new NotImplementedException();
        }

        private static JToken AddPlaceholder(List<byte[]> buffers, byte[] byteArray)
        {
            var placeholder = new JObject();
            try
            {
                placeholder.Add(KEY_PLACEHOLDER, true);
                placeholder.Add(KEY_NUM, buffers.Count);
            }
            catch (Exception)
            {
                return null;
            }
            buffers.Add(byteArray);
            return placeholder;
        }

        public static Packet ReconstructPacket(Packet packet, byte[][] buffers)
        {
            packet.Data = _reconstructPacket(packet.Data, buffers);
            packet.Attachments = -1;
            return packet;
        }

        private static object _reconstructPacket(object data, byte[][] buffers)
        {
            //var t = data.GetType();

            if (data is JValue)
            {
                var dataStr = data.ToString();
                if (!dataStr.StartsWith("[") && !dataStr.StartsWith("{"))
                {
                    //
                    return dataStr;
                }
                var jdata = JToken.Parse(data.ToString());
                if (jdata.SelectToken(KEY_PLACEHOLDER) != null)
                {
                    var jpl = jdata[KEY_PLACEHOLDER];
                    var jnum = jdata[KEY_NUM];
                    if (jpl != null && jnum != null)
                    {
                        var placeholder = jpl.ToObject<bool>();
                        if (placeholder)
                        {
                            var num = jnum.ToObject<int>();
                            return buffers[num];
                        }
                    }
                }
                else
                {
                    var recValue = _reconstructPacket(jdata, buffers);
                    return recValue;
                }

             

                //jdata
            }else if (data is JArray)
            {
                var _data = (JArray)data;
                int len = _data.Count;
                var newData = new JArray();
                for (int i = 0; i < len; i++)
                {
                    try
                    {
                        var recValue = _reconstructPacket(_data[i], buffers);
                        if (recValue is string)
                        {
                            //newData[i] = (string) recValue;
                            newData.Add((string)recValue);
                        }

                        if (recValue is byte[])
                        {
                            newData.Add((byte[])recValue);
                        }
                        else if (recValue is JArray)
                        {
                            //newData[i] = (JArray) recValue;
                            newData.Add((JArray)recValue);
                        }
                        else if (recValue is JObject)
                        {
                            //newData[i] = (JObject)recValue;
                            newData.Add((JObject)recValue);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return newData;
            }
            if (!(data is JObject))
            {
                return data;
            }

            var newData1 = new JObject();
            var _data1 = (JObject)data;

            if ((bool) _data1[KEY_PLACEHOLDER])
            {
                var num = (int)_data1[KEY_NUM];
                return num >= 0 && num < buffers.Length ? buffers[num] : null;
            }

            foreach (var property in _data1.Properties())
            {               
                try
                {
                    var recValue = _reconstructPacket(property.Value, buffers);
                    if (recValue is byte[])
                    {
                        newData1[property.Name] = (byte[])recValue;
                    }
                    else if (recValue is JArray)
                    {
                        newData1[property.Name] = (JArray)recValue;
                    }
                    else if (recValue is JObject)
                    {
                        newData1[property.Name] = (JObject)recValue;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return newData1;                
        }


        public class DeconstructedPacket
        {
            public Packet Packet;
            public byte[][] Buffers;
        }
    }
}
