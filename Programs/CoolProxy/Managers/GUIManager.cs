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
    public delegate bool MenuOptionDel(object user_data);

    public delegate void MenuOptionClick(object user_data);

    public class GUIManager : IGUI
    {
        private CoolProxyForm coolForm;

        public GUIManager(CoolProxyForm form)
        {
            coolForm = form;
        }

        public void AddToolButton(string category, string label, EventHandler eventHandler)
        {
            coolForm.AddToolButton(category, label, eventHandler);
        }

        public void AddToolCheckbox(string category, string label, EventHandler handler, bool button_style = false)
        {
            coolForm.AddToolCheckbox(category, label, handler, button_style);
        }

        public void AddToolCheckbox(string category, string label, string setting)
        {
            coolForm.AddToolCheckbox(category, label, setting);
        }

        public void AddToolLabel(string category, string text)
        {
            coolForm.AddToolLabel(category, text);
        }

        public void AddToolComboBox(string category, EventHandler handler, object[] values, object default_item = null)
        {
            coolForm.AddToolComboBox(category, handler, values, default_item);
        }

        /////////////////////////////////////

        public void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null)
        {
            coolForm.AddInventoryItemOption(label, handle, assetType, enable);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null)
        {
            coolForm.AddInventoryItemOption(label, handle, invType, enable);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null)
        {
            coolForm.AddInventoryItemOption(label, handle, enable);
        }

        public void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null)
        {
            coolForm.AddInventoryFolderOption(label, handle, enable);
        }


        ////////////////////
        ///


        public void AddSingleMenuItem(string label, HandleAvatarPicker handle)
        {
            coolForm.AddSingleMenuItem(label, handle);
        }


        public void AddMultipleMenuItem(string label, HandleAvatarPickerList handle)
        {
            coolForm.AddMultipleMenuItem(label, handle);
        }


        public void AddToggleFormQuick(string cat, string name, Form form)
        {
            coolForm.AddToggleFormQuick(cat, name, form);
        }

        ////////////////////
        ///

        public void AddMainMenuOption(MenuOption option)
        {
            coolForm.AddMainMenuOption(option);
        }


        ////////////////////
        ///

        public void AddSettingsTab(string label, Panel panel)
        {
            coolForm.AddSettingsTab(label, panel);
        }
    }
}
