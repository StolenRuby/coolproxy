using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ClientAO
{
    class AOCommand : Command
    {
        SettingsManager Settings;

        public AOCommand(SettingsManager settings, CoolProxyFrame frame)
        {
            Settings = settings;
            Proxy = frame;
            CMD = ".ao";
            Name = "Controls for Client AO";
            Description = "Usage: .ao <on/off>\n.ao load <path/to/notecard>";
            Category = CommandCategory.Other;


            frame.Inventory.FolderUpdated += Inventory_FolderUpdated;
        }

        public override string Execute(string[] args)
        {
            if(args.Length < 2)
            {
                return "Not enough args!";
            }

            string cmd = args[1];
            if("load" == cmd)
            {
                //Load notecard from path
                //exemple: /ao Objects/My AOs/wetikon/config.txt
                string[] tmp = new string[args.Length - 2];
                //join the arguments together with spaces, to
                //take care of folder and item names with spaces in them
                for (int i = 2; i < args.Length; i++)
                {
                    tmp[i - 2] = args[i];
                }

                PathNames = string.Join(" ", tmp).Split(new char[] { '/' });
                PathIndex = 0;
                CurrentFolder = Proxy.Inventory.InventoryRoot;
                Proxy.Inventory.RequestFolderContents(CurrentFolder, Proxy.Agent.AgentID, PathNames.Length > 1, PathNames.Length == 1, InventorySortOrder.ByName);
            }
            else if ("on" == cmd)
            {
                Settings.setBool("EnableAO", true);
            }
            else if ("off" == cmd)
            {
                Settings.setBool("EnableAO", false);
            }

            return string.Empty;
        }


        UUID CurrentFolder = UUID.Zero;
        string[] PathNames;
        int PathIndex = 0;

        private void Inventory_FolderUpdated(object sender, GridProxy.FolderUpdatedEventArgs e)
        {
            if (e.FolderID == CurrentFolder)
            {
                string next_name = PathNames[PathIndex].ToLower();

                bool last_entry = PathIndex == PathNames.Length - 1;

                if (last_entry)
                {
                    List<InventoryBase> contents = Proxy.Inventory.Store.GetContents(CurrentFolder);

                    foreach (var item in contents)
                    {
                        if (item is InventoryItem)
                        {
                            if (item.Name.ToLower() == next_name && item is InventoryNotecard)
                            {
                                InventoryNotecard notecard = item as InventoryNotecard;
                                Proxy.SayToUser("Notecard found! Asset ID is " + notecard.AssetUUID.ToString());
                                CurrentFolder = UUID.Zero;
                                ClientAOPlugin.Instance.LoadNotecard(notecard);
                                return;
                            }
                        }
                    }

                    Proxy.SayToUser("Notecard not found!");
                }
                else
                {
                    List<InventoryBase> contents = Proxy.Inventory.Store.GetContents(CurrentFolder);

                    bool found = false;

                    foreach (var item in contents)
                    {
                        if (item is InventoryFolder)
                        {
                            if (item.Name.ToLower() == next_name)
                            {
                                CurrentFolder = item.UUID;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        PathIndex++;
                        Proxy.Inventory.RequestFolderContents(CurrentFolder, Proxy.Agent.AgentID, true, true, InventorySortOrder.ByName);
                    }
                    else
                    {
                        Proxy.SayToUser("Folder `" + next_name + "` was not found!");
                    }
                }
            }
        }
    }
}
