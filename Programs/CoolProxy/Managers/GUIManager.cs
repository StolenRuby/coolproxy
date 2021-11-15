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
    public delegate bool TrayIconEnable();

    public class GUIManager
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

        public void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType)
        {
            coolForm.AddInventoryItemOption(label, handle, assetType);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType)
        {
            coolForm.AddInventoryItemOption(label, handle, invType);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle)
        {
            coolForm.AddInventoryItemOption(label, handle);
        }

        public void AddInventoryFolderOption(string label, HandleInventoryFolder handle)
        {
            coolForm.AddInventoryFolderOption(label, handle);
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

        public void AddTrayOption(string label, EventHandler option, TrayIconEnable opening = null, object tag = null)
        {
            coolForm.AddTrayOption(label, option, opening, tag);
        }

        public void AddTrayCheck(string label, EventHandler option, TrayIconEnable check, object tag = null)
        {
            coolForm.AddTrayCheck(label, option, check, tag);
        }
    }
}
