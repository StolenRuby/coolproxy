/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CoolProxy.Plugins.Messages
{

    public static class PlainPacketDecoder
    {
        /// <summary>
        /// Creates a formatted string containing the values of a Packet
        /// </summary>
        /// <param name="packet">The Packet</param>
        /// <returns>A formatted string of values of the nested items in the Packet object</returns>
        public static string PacketToString(Packet packet, bool creating = false)
        {
            StringBuilder result = new StringBuilder();

            //result.Append(packet.Trusted ? "in " : "out ");
            //result.AppendFormat("{0}" + Environment.NewLine, packet.Type);

            FieldInfo[] fields = packet.GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                // we're not interested in any of these here
                if (fields[i].Name == "Type" || fields[i].Name == "Header" || fields[i].Name == "HasVariableBlocks" || fields[i].Name == "Trusted")
                    continue;

                if (fields[i].FieldType.IsArray)
                {
                    //result.AppendFormat("[" + fields[i].Name + "][]" + Environment.NewLine);
                    RecursePacketArray(fields[i], packet, ref result);
                }
                else
                {
                    result.Append("[" + fields[i].Name + "]" + Environment.NewLine);
                    RecursePacketField(fields[i], packet, ref result, creating);
                }
            }
            return result.ToString();
        }

        private static void RecursePacketArray(FieldInfo fieldInfo, object packet, ref StringBuilder result)
        {

            var packetDataObject = fieldInfo.GetValue(packet);
            int k = -1;
            foreach (object nestedArrayRecord in (Array)packetDataObject ?? new object[0])
            {
                result.AppendFormat("[" + fieldInfo.Name + "]" + Environment.NewLine);

                FieldInfo[] fields = nestedArrayRecord?.GetType().GetFields() ?? new FieldInfo[0];
                ++k;
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].FieldType.IsArray) // default for an array (probably a byte[])
                    {
                        bool use_hex = false;

                        if (fields[i].Name == "Data" || fields[i].Name == "TextureEntry" || fields[i].Name == "ObjectData" || fields[i].Name == "Color" || fields[i].Name == "TypeData")
                        {
                            use_hex = true;
                        }

                        byte[] value = (byte[])fields[i].GetValue(nestedArrayRecord) ?? new byte[0];
                        result.AppendFormat("{0}={1}" + Environment.NewLine,
                            fields[i].Name,
                            use_hex ? "|" + string.Join(" ", (value.Select(x => x.ToString("X2")))) : Utils.BytesToString(value));
                    }
                    else // default for a field
                    {
                        result.AppendFormat("{0}={1}" + Environment.NewLine,
                            fields[i].Name,
                            fields[i].GetValue(nestedArrayRecord));
                    }
                }

                // Handle Properties
                foreach (PropertyInfo propertyInfo in nestedArrayRecord?.GetType().GetProperties() ?? new PropertyInfo[0])
                {
                    if (propertyInfo.Name.Equals("Length"))
                        continue;

                    /* Leave the c for now at the end, it signifies something useful that still needs to be done i.e. a decoder written */
                    result.AppendFormat("{0, 30}: {1,-40} [{2}]c" + Environment.NewLine,
                        propertyInfo.Name,
                        Utils.BytesToString((byte[])propertyInfo.GetValue(nestedArrayRecord, null)),
                        propertyInfo.PropertyType.Name);
                }
                //result.AppendFormat("{0,32}" + Environment.NewLine, "***");
            }
        }

        private static void RecursePacketField(FieldInfo fieldInfo, object packet, ref StringBuilder result, bool creating)
        {
            object packetDataObject = fieldInfo.GetValue(packet);

            // handle Fields
            foreach (FieldInfo packetValueField in fieldInfo.GetValue(packet).GetType().GetFields())
            {
                if (packetValueField.FieldType.IsArray)
                {
                    bool use_hex = false;

                    if(packetValueField.Name == "Data" || packetValueField.Name == "TextureEntry" || packetValueField.Name == "Params")
                    {
                        use_hex = true;
                    }

                    byte[] data = (byte[])packetValueField.GetValue(packetDataObject) ?? new byte[0];
                    result.AppendFormat("{0}={1}" + Environment.NewLine,
                        packetValueField.Name,
                        use_hex ? "|" + string.Join(" ", (data.Select(x => x.ToString("X2")))) : Utils.BytesToString(data));
                }
                else
                {
                    if(creating)
                    {
                        if (packetValueField.Name == "AgentID")
                        {
                            result.AppendLine("AgentID=$AgentID");
                            continue;
                        }
                        else if (packetValueField.Name == "SessionID")
                        {
                            result.AppendLine("SessionID=$SessionID");
                            continue;
                        }
                    }

                    result.AppendFormat("{0}={1}" + Environment.NewLine, packetValueField.Name, packetValueField.GetValue(packetDataObject));

                }
            }

            // Handle Properties
            foreach (PropertyInfo propertyInfo in packetDataObject.GetType().GetProperties())
            {
                if (propertyInfo.Name.Equals("Length"))
                    continue;
                
                if (propertyInfo.GetValue(packetDataObject, null).GetType() == typeof(byte[]))
                {
                    result.AppendFormat("{0}={1}" + Environment.NewLine,
                        propertyInfo.Name,
                        Utils.BytesToString((byte[])propertyInfo.GetValue(packetDataObject, null)));
                        //propertyInfo.PropertyType.Name);
                }
                else
                {

                    result.AppendFormat("{0}={1}" + Environment.NewLine,
                        propertyInfo.Name,
                        propertyInfo.GetValue(packetDataObject, null));
                }
            }
        }
    }
}
