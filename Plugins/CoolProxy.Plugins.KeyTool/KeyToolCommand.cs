using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.KeyTool
{
    class KeyToolCommand : Command
    {
        public KeyToolCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".keytool";
            Name = "KeyTool a UUID";
            Description = "Run a keytool on a give UUID.\n\nUsage: .keytool <uuid>";
            Category = CommandCategory.CoolProxy;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
                return "Usage: .keytool <uuid>";

            UUID key = UUID.Zero;

            if (UUID.TryParse(args[1], out key))
            {
                Proxy.SayToUser("KeyTool", "Running KeyTool on " + key.ToString());

                Thread keytoolGUIThread = null;
                keytoolGUIThread = new Thread(new ThreadStart(() =>
                {
                    var keytoolGUI = new KeyToolForm(Proxy, key);
                    //keytoolGUI.FormClosed += (x, y) => { keytoolGUIThread = null; keytoolGUI = null; };
                    //Application.Run(keytoolGUI);
                    keytoolGUI.ShowDialog();
                    keytoolGUIThread = null;
                    keytoolGUI = null;
                }));
                keytoolGUIThread.SetApartmentState(ApartmentState.STA);
                keytoolGUIThread.Start();

                return string.Empty;
            }
            else return "Invalid UUID";
        }
    }
}
