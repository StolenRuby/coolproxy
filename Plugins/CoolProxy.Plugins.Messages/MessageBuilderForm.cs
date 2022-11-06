using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Messages
{
    public partial class MessageBuilderForm : Form
    {
        private CoolProxyFrame Proxy;

        private static MessageBuilderForm Instance;


        private Assembly openmvAssembly;

        public MessageBuilderForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            Instance = this;
            InitializeComponent();


            openmvAssembly = Assembly.Load("OpenMetaverse");
            if (openmvAssembly == null) throw new Exception("Assembly load exception");

            foreach(PacketType pt in Enum.GetValues(typeof(PacketType)))
            {
                var packet = Packet.BuildPacket(pt);
                if (packet == null) continue;
                if(packet.Trusted)
                {
                    comboBox2.Items.Add(pt.ToString());
                }
                else
                {
                    comboBox1.Items.Add(pt.ToString());
                }    
            }

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            regionsDGV.Rows.Clear();

            foreach(var region in Proxy.Network.Regions.ToArray())
            {
                regionsDGV.Rows.Add(region.Name + (region == Proxy.Network.CurrentSim ? " (main)" : "") , "Alive");
            }
        }

        internal static void Open(Packet packet, Direction dir)
        {
            Instance.Show();

            Instance.textBox1.Text = (dir == Direction.Incoming ? "in " : "out ") + packet.Type.ToString() + Environment.NewLine + PlainPacketDecoder.PacketToString(packet);
            Instance.textBox1.SelectionStart = Instance.textBox1.Text.Length;
            Instance.textBox1.SelectionLength = 0;

            if (Instance.WindowState == FormWindowState.Minimized)
                Instance.WindowState = FormWindowState.Normal;

            Instance.Focus();
        }


        internal void InjectPacket(string packetData, bool toSimulator)
        {
            Direction direction = Direction.Incoming;
            string name = null;
            string block = null;
            object blockObj = null;
            Type packetClass = null;
            Packet packet = null;

            try
            {
                foreach (string line in packetData.Split(new[] { '\n' }))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    Match match;

                    if (name == null)
                    {
                        match = (new Regex(@"^\s*(in|out)\s+(\w+)\s*$")).Match(line);
                        if (!match.Success)
                        {
                            OpenMetaverse.Logger.Log("expecting direction and packet name, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                            return;
                        }

                        string lineDir = match.Groups[1].Captures[0].ToString();
                        string lineName = match.Groups[2].Captures[0].ToString();

                        if (lineDir == "in")
                            direction = Direction.Incoming;
                        else if (lineDir == "out")
                            direction = Direction.Outgoing;
                        else
                        {
                            OpenMetaverse.Logger.Log("expecting 'in' or 'out', got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                            return;
                        }

                        name = lineName;
                        packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + name + "Packet");
                        if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");
                        ConstructorInfo ctr = packetClass.GetConstructor(new Type[] { });
                        if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + name + "Packet");
                        packet = (Packet)ctr.Invoke(new object[] { });
                    }
                    else
                    {
                        match = (new Regex(@"^\s*\[(\w+)\]\s*$")).Match(line);
                        if (match.Success)
                        {
                            block = match.Groups[1].Captures[0].ToString();
                            FieldInfo blockField = packetClass.GetField(block);
                            if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
                            Type blockClass = blockField.FieldType;
                            if (blockClass.IsArray)
                            {
                                blockClass = blockClass.GetElementType();
                                ConstructorInfo ctr = blockClass.GetConstructor(new Type[] { });
                                if (ctr == null) throw new Exception("Couldn't get suitable constructor for " + blockClass.Name);
                                blockObj = ctr.Invoke(new object[] { });
                                object[] arr = (object[])blockField.GetValue(packet);
                                if (arr == null) arr = new object[0];
                                object[] narr = (object[])Array.CreateInstance(blockClass, arr.Length + 1);
                                Array.Copy(arr, narr, arr.Length);
                                narr[arr.Length] = blockObj;
                                blockField.SetValue(packet, narr);
                                //Console.WriteLine("Added block "+block);
                            }
                            else
                            {
                                blockObj = blockField.GetValue(packet);
                            }
                            if (blockObj == null) throw new Exception("Got " + name + "Packet." + block + " == null");
                            //Console.WriteLine("Got block " + name + "Packet." + block);

                            continue;
                        }

                        if (block == null)
                        {
                            OpenMetaverse.Logger.Log("expecting block name, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                            return;
                        }

                        match = (new Regex(@"^\s*(\w+)\s*=\s*(.*)$")).Match(line);
                        if (match.Success)
                        {
                            string lineField = match.Groups[1].Captures[0].ToString();
                            string lineValue = match.Groups[2].Captures[0].ToString();
                            object fval;

                            //FIXME: use of MagicCast inefficient
                            //if (lineValue == "$Value")
                            //    fval = MagicCast(name, block, lineField, value);
                            if (lineValue == "$UUID")
                                fval = UUID.Random();
                            else if (lineValue == "$AgentID")
                                fval = Proxy.Agent.AgentID;
                            else if (lineValue == "$SessionID")
                                fval = Proxy.Agent.SessionID;
                            else
                                fval = MagicCast(name, block, lineField, lineValue);

                            MagicSetField(blockObj, lineField, fval);
                            continue;
                        }
                        OpenMetaverse.Logger.Log("expecting block name or field, got: " + line, OpenMetaverse.Helpers.LogLevel.Error);
                        return;
                    }
                }

                if (name == null)
                {

                    OpenMetaverse.Logger.Log("expecting direction and packet name, got EOF", OpenMetaverse.Helpers.LogLevel.Error);
                    return;
                }

                packet.Header.Reliable = true;

                Proxy.Network.InjectPacket(packet, direction);

                OpenMetaverse.Logger.Log("Injected " + name, OpenMetaverse.Helpers.LogLevel.Info);
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("failed to injected " + name, OpenMetaverse.Helpers.LogLevel.Error, e);
            }
        }

        private static void MagicSetField(object obj, string field, object val)
        {
            Type cls = obj.GetType();

            FieldInfo fieldInf = cls.GetField(field);
            if (fieldInf == null)
            {
                PropertyInfo prop = cls.GetProperty(field);
                if (prop == null) throw new Exception("Couldn't find field " + cls.Name + "." + field);
                prop.SetValue(obj, val, null);
                //throw new Exception("FIXME: can't set properties");
            }
            else
            {
                fieldInf.SetValue(obj, val);
            }
        }

        // MagicCast: given a packet/block/field name and a string, convert the string to a value of the appropriate type
        private object MagicCast(string name, string block, string field, string value)
        {
            Type packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + name + "Packet");
            if (packetClass == null) throw new Exception("Couldn't get class " + name + "Packet");

            FieldInfo blockField = packetClass.GetField(block);
            if (blockField == null) throw new Exception("Couldn't get " + name + "Packet." + block);
            Type blockClass = blockField.FieldType;
            if (blockClass.IsArray) blockClass = blockClass.GetElementType();
            // Console.WriteLine("DEBUG: " + blockClass.Name);

            FieldInfo fieldField = blockClass.GetField(field); PropertyInfo fieldProp = null;
            Type fieldClass = null;
            if (fieldField == null)
            {
                fieldProp = blockClass.GetProperty(field);
                if (fieldProp == null) throw new Exception("Couldn't get " + name + "Packet." + block + "." + field);
                fieldClass = fieldProp.PropertyType;
            }
            else
            {
                fieldClass = fieldField.FieldType;
            }

            try
            {
                if (fieldClass == typeof(byte))
                {
                    return Convert.ToByte(value);
                }
                else if (fieldClass == typeof(ushort))
                {
                    return Convert.ToUInt16(value);
                }
                else if (fieldClass == typeof(uint))
                {
                    return Convert.ToUInt32(value);
                }
                else if (fieldClass == typeof(ulong))
                {
                    return Convert.ToUInt64(value);
                }
                else if (fieldClass == typeof(sbyte))
                {
                    return Convert.ToSByte(value);
                }
                else if (fieldClass == typeof(short))
                {
                    return Convert.ToInt16(value);
                }
                else if (fieldClass == typeof(int))
                {
                    return Convert.ToInt32(value);
                }
                else if (fieldClass == typeof(long))
                {
                    return Convert.ToInt64(value);
                }
                else if (fieldClass == typeof(float))
                {
                    return Convert.ToSingle(value);
                }
                else if (fieldClass == typeof(double))
                {
                    return Convert.ToDouble(value);
                }
                else if (fieldClass == typeof(UUID))
                {
                    return new UUID(value);
                }
                else if (fieldClass == typeof(bool))
                {
                    if (value.ToLower() == "true")
                        return true;
                    else if (value.ToLower() == "false")
                        return false;
                    else
                        throw new Exception();
                }
                else if (fieldClass == typeof(byte[]))
                {
                    if(value.StartsWith("|"))
                    {
                        return Utils.HexStringToBytes(value.Substring(1).Replace(" ", ""), true);
                    }
                    else return Utils.StringToBytes(value);
                }
                else if (fieldClass == typeof(Vector3))
                {
                    Vector3 result;
                    if (Vector3.TryParse(value, out result))
                        return result;
                    else
                        throw new Exception();
                }
                else if (fieldClass == typeof(Vector3d))
                {
                    Vector3d result;
                    if (Vector3d.TryParse(value, out result))
                        return result;
                    else
                        throw new Exception();
                }
                else if (fieldClass == typeof(Vector4))
                {
                    Vector4 result;
                    if (Vector4.TryParse(value, out result))
                        return result;
                    else
                        throw new Exception();
                }
                else if (fieldClass == typeof(Quaternion))
                {
                    Quaternion result;
                    if (Quaternion.TryParse(value, out result))
                        return result;
                    else
                        throw new Exception();
                }
                else
                {
                    throw new Exception("unsupported field type " + fieldClass);
                }
            }
            catch
            {
                throw new Exception("unable to interpret " + value + " as " + fieldClass);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string data = textBox1.Text;
            data = data.Replace(Environment.NewLine, "\n");
            InjectPacket(data, true);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            string val = comboBox.SelectedItem as string;

            if(Enum.TryParse<PacketType>(val, out PacketType pt))
            {
                Packet packet = Packet.BuildPacket(pt);


                Type packetClass = openmvAssembly.GetType("OpenMetaverse.Packets." + pt.ToString() + "Packet");

                var mems = packetClass.GetFields();
                foreach(var m in mems)
                {
                    if(m.FieldType.IsSubclassOf(typeof(PacketBlock)))
                    {
                        var block = Activator.CreateInstance(m.FieldType);
                        m.SetValue(packet, block);
                    }
                    else if(m.FieldType.IsArray)
                    {
                        Type t = m.FieldType.GetElementType();
                        Array a = Array.CreateInstance(t, 1);
                        PacketBlock block = (PacketBlock)Activator.CreateInstance(t);
                        a.SetValue(block, 0);

                        m.SetValue(packet, a);
                    }
                }

                textBox1.Text = (packet.Trusted ? "in " : "out ") + packet.Type.ToString() + Environment.NewLine + PlainPacketDecoder.PacketToString(packet, true);
            }
        }
    }
}
