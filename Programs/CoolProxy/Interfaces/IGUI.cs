using CoolGUI.Controls;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public class MenuItem
    {
        public string Name { get; private set; }
        public string Label { get; private set; }

        public Color Color { get; set; } = Color.Black;

        public MenuItem(string name, string label)
        {
            Name = name;
            Label = label;
        }
    }


    public class MenuOption : MenuItem
    {
        public bool Default { get; private set; }
        public string[] DefaultPath { get; private set; }

        public MenuOptionClick Clicked { get; set; } = null;
        public MenuOptionDel Checked { get; set; } = null;
        public MenuOptionDel Enabled { get; set; } = null;
        public object Tag { get; set; } = null;

        public MenuOption(string name, string label, bool on_by_default, string default_folder = null) : base(name, label)
        {
            Default = on_by_default;
            DefaultPath = default_folder != null ? new string[] { default_folder } : null;
        }

        public MenuOption(string name, string label, bool on_by_default, string[] default_path) : base(label, name)
        {
            Default = on_by_default;
            DefaultPath = default_path;
        }
    }

    public class MenuFolder : MenuItem
    {
        public List<MenuItem> SubItems { get; set; } = new List<MenuItem>();

        public MenuFolder(string name, string label) : base(name, label) { }
    }

    public class MenuSeparator : MenuItem
    {
        public MenuSeparator() : base(string.Empty, string.Empty) { }
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
        void AddMainMenuOption(MenuOption option);
    }
}
