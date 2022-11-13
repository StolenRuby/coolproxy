using CoolProxy.Plugins.OpenSim;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.GetAvatar
{
    public class GetAvatarPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;
        private IROBUST ROBUST;

        public GetAvatarPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            ROBUST = frame.RequestModuleInterface<IROBUST>();
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            gui.AddMainMenuOption(new MenuOption("RETRIEVE_STORED_AVATAR", "Retrieve Stored Avatar...", true, "Hacks")
            {
                Clicked = (x) => retrieveStoredAvatar(null, null)
            });

            gui.AddToolButton("Hacks", "Retrieve Stored Avatar", retrieveStoredAvatar);
        }

        private void retrieveStoredAvatar(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearchForm = new AvatarPickerSearchForm();

            avatarPickerSearchForm.StartPosition = FormStartPosition.CenterScreen;

            if (avatarPickerSearchForm.ShowDialog() == DialogResult.OK)
            {
                Proxy.SayToUser("Retrieving the stored appearance of " + avatarPickerSearchForm.SelectedName);

                string name = avatarPickerSearchForm.SelectedName;
                string[] split = name.Split(' ');

                string grid_uri = Proxy.Network.CurrentSim.GridURI;

                if (split[1].StartsWith("@"))
                {
                    grid_uri = "http://" + split[1].Substring(1);
                }

                GetAvatar(avatarPickerSearchForm.SelectedID, grid_uri, avatarPickerSearchForm.SelectedName);
            }
        }

        public void GetAvatar(UUID avatar, string grid_uri, string folder_name)
        {
            try
            {
                Dictionary<string, object> sendData = new Dictionary<string, object> {
                        { "METHOD", "getavatar"},
                        { "UserID", avatar.ToString() }
                    };

                string request_str = ServerUtils.BuildQueryString(sendData);

                WebClient webClient = new WebClient();
                webClient.UploadStringCompleted += (x, y) =>
                {
                    if (y.Error != null)
                    {
                        Proxy.SayToUser(y.Error.Message);
                        return;
                    }

                    Dictionary<string, object> replyData = ServerUtils.ParseXmlResponse(y.Result);

                    if (replyData.ContainsKey("result"))
                    {
                        var boom = (Dictionary<string, object>)replyData["result"];

                        List<UUID> attachment_ids = new List<UUID>();

                        foreach (var pair in boom)
                        {
                            if (pair.Key.StartsWith("_ap_"))
                            {
                                UUID[] keys = pair.Value.ToString().Split(',').Select(key => new UUID(key)).ToArray();
                                attachment_ids.AddRange(keys);
                            }
                            else if (pair.Key.StartsWith("Wearable "))
                            {
                                string val = pair.Value.ToString().Split(':')[0];
                                attachment_ids.Add(new UUID(val));
                            }
                        }

                        UUID parent_id = Proxy.Inventory.SuitcaseID != UUID.Zero ?
                            Proxy.Inventory.FindSuitcaseFolderForType(FolderType.Clothing) :
                            Proxy.Inventory.FindFolderForType(FolderType.Clothing);

                        UUID folder_id = UUID.Random();

                        ROBUST.Inventory.AddFolder(folder_name, folder_id, parent_id, Proxy.Agent.AgentID, FolderType.None, 0, (folder_added) =>
                        {
                            if (folder_added)
                            {
                                int count = 0;

                                foreach (UUID id in attachment_ids)
                                {
                                    ROBUST.Inventory.GetItem(id, avatar, (item) =>
                                    {
                                        if (item == null)
                                        {
                                            Proxy.SayToUser(id.ToString() + " was not found!");
                                            count++;
                                            return;
                                        }

                                        item.UUID = UUID.Random();
                                        item.CreationDate = DateTime.UtcNow;
                                        item.OwnerID = Proxy.Agent.AgentID;
                                        item.ParentUUID = folder_id;

                                        ROBUST.Inventory.AddItem(item, (success) =>
                                        {
                                            if (++count == attachment_ids.Count)
                                            {
                                                Proxy.SayToUser("All done! Requesting folder contents...");
                                                Proxy.Inventory.RequestFolderContents(parent_id, Proxy.Agent.AgentID, true, true, InventorySortOrder.ByDate, false);
                                            }
                                        });
                                    });
                                }
                            }
                        });
                    }
                };

                string target_uri = grid_uri;
                target_uri += "avatar";

                Uri uri = new Uri(target_uri);

                uri.SetPort(8003); // boom now

                webClient.UploadStringAsync(uri, "POST", request_str);
            }
            catch (Exception ex)
            {
                Proxy.SayToUser(ex.Message);
                return;
            }
        }
    }
}
