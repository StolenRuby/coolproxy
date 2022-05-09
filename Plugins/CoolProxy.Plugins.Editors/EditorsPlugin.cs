using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Editors
{
    public class EditorsPlugin : CoolProxyPlugin
    {
        public EditorsPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            gui.AddInventoryItemOption("Hex Edit", x => new HexEditor(x, frame).Show(), x => x.AssetUUID != UUID.Zero);
            gui.AddInventoryItemOption("Text Edit", x => new TextEditor(x, frame).Show(), x => x.AssetUUID != UUID.Zero);
        }
    }
}
