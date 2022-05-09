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
        static public CoolProxyFrame Frame = null;

        private static CoolProxyForm coolProxyForm;

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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = false;

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
