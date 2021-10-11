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

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = false;

            // Have to load this first to check if we're allowed multiple instances
            Settings = new SettingsManager();

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
