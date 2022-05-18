using CoolGUI.Controls;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public class TrayOption
    {
        public string Label { get; set; }
        public EventHandler Option { get; set; }
        public TrayIconEnable Enable { get; set; }
        public TrayIconEnable Checked { get; set; }
        public object Tag { get; set; }

        public List<TrayOption> SubMenu { get; set; }

        public TrayOption(string label, EventHandler option, TrayIconEnable enabled, TrayIconEnable check, object tag)
        {
            Label = label;
            Option = option;
            Enable = enabled;
            Checked = check;
            Tag = tag;
        }
    }

    public interface IGUI
    {
        ////////// Settings //////////
        void AddSettingsTab(string label, Panel panel);

        ////////// ToolBox //////////
        void AddToolButton(string category, string label, EventHandler eventHandler);
        void AddToolCheckbox(string category, string label, EventHandler handler, bool button_style = false);
        void AddToolCheckbox(string category, string label, string setting);
        void AddToolLabel(string category, string text);
        void AddToolComboBox(string category, EventHandler handler, object[] values, object default_item = null);
        void AddToggleFormQuick(string cat, string name, Form form);

        ////////// Inventory //////////
        void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null);
        void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null);
        void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null);
        void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null);

        ////////// Avatar Tracker //////////
        void AddSingleMenuItem(string label, HandleAvatarPicker handle);
        void AddMultipleMenuItem(string label, HandleAvatarPickerList handle);

        ////////// Tray //////////
        void AddTrayOption(string label, EventHandler option, TrayIconEnable opening = null, object tag = null);
        void AddTrayCheck(string label, EventHandler option, TrayIconEnable check, object tag = null);
        void AddTrayOption(TrayOption option);
    }
}
