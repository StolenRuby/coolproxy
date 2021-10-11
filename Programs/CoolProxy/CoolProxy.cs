using GridProxy;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    static class CoolProxy
    {
        //private static Mutex appMutex = null;

        static public SettingsManager Settings = null;

        static public CoolProxyFrame Frame = null;

        public static bool IsDebugMode
        {
            get
            {
                #if DEBUG
                return true;
                #else
                return false;
                #endif
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        static StreamWriter _stdOutWriter;
        public static void WriteLine(string line)
        {
            _stdOutWriter.WriteLine(line);
            Console.WriteLine(line);
        }

        private static CoolProxyForm coolProxyForm;

        internal static GUIManager GUI { get { return coolProxyForm.GUI; } }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //if(CoolProxy.IsDebugMode)
            //{
            //    MessageBox.Show("We're in debug mode!");
            //}

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (Type.GetType("Mono.Runtime") == null)
            {
                var stdout = Console.OpenStandardOutput();
                _stdOutWriter = new StreamWriter(stdout);
                _stdOutWriter.AutoFlush = true;

                AttachConsole(ATTACH_PARENT_PROCESS);
            }


            // This needs to be called before the MessageBox

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = false;

            // Have to load this first to check if we're allowed multiple instances
            Settings = new SettingsManager();



            //OSD osd = Settings.getOSD("TestOSD");
            //OSDMap map = (OSDMap)osd;

            //foreach(string key in map.Keys)
            //{
            //    MessageBox.Show(key);
            //}

            //map["Test"] = "Magic";
            //map["Test2"] = "Magic2";
            //map["Test3"] = "Magic3";

            //Settings.setOSD("TestOSD", map);



            //bool createdNew;
            //var appMutex = new Mutex(true, "CoolProxyApp", out createdNew);

            //if (!createdNew && !Settings.getBool("AllowMultipleInstances"))
            //{
            //    MessageBox.Show("Cool Proxy is already running!");
            //    return;
            //}

            // ToDo: Add support for settings
            Frame = new CoolProxyFrame(new string[] { });

            coolProxyForm = new CoolProxyForm();
            Application.Run(coolProxyForm);
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ((AppDomain)sender).GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);
        }
    }

    public abstract class CoolProxyPlugin : MarshalByRefObject
    {
        //public extern CoolProxyPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame);
    }
}
