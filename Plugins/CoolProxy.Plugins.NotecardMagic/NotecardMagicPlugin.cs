using OpenMetaverse;
using OpenMetaverse.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace CoolProxy.Plugins.NotecardMagic
{
    public class NotecardMagicPlugin : CoolProxyPlugin, INotecardMagic
    {
        private CoolProxyFrame Proxy;

        public NotecardMagicPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;
            Proxy.RegisterModuleInterface<INotecardMagic>(this);
        }

        public void GetAsset(UUID folder_id, UUID asset_id, AssetType asset_type, InventoryType inventory_type)
        {
            Proxy.Inventory.RequestCreateItem(Proxy.Inventory.InventoryRoot, "New Note", string.Empty, AssetType.Notecard, UUID.Zero, InventoryType.Notecard, PermissionMask.All, (success, new_note_item) =>
            {
                if(!success)
                {
                    Proxy.SayToUser("Failed to create notecard!");
                    return;
                }

                InventoryItem item = QuickItem(asset_id, asset_type, inventory_type);

                AssetNotecard notecard = new AssetNotecard();
                notecard.EmbeddedItems = new List<InventoryItem>();
                notecard.EmbeddedItems.Add(item);
                notecard.Encode();

                Proxy.Inventory.RequestUpdateNotecardTask(notecard.AssetData, new_note_item.UUID, UUID.Zero, (update_success, error_message, item_id, new_asset_id) =>
                {
                    if(update_success)
                    {
                        Proxy.Inventory.RequestCopyItemFromNotecard(UUID.Zero, item_id, folder_id, item.UUID, (the_item) =>
                        {
                            Proxy.Inventory.RemoveItem(item_id);
                        });
                    }
                    else
                    {
                        Proxy.SayToUser(error_message);
                    }
                });
            });
        }

        InventoryItem QuickItem(UUID asset_id, AssetType asset_type, InventoryType inventory_type)
        {
            InventoryItem item = new InventoryItem(UUID.Random());
            item.AssetType = asset_type;
            item.AssetUUID = asset_id;

            item.CreationDate = DateTime.UtcNow;
            item.CreatorData = string.Empty;
            item.CreatorID = UUID.Zero;
            item.Description = string.Empty;
            item.Flags = 0;
            item.GroupID = UUID.Zero;
            item.GroupOwned = false;
            item.InventoryType = inventory_type;
            item.LastOwnerID = UUID.Zero;
            item.Name = asset_id.ToString();
            item.OwnerID = UUID.Random();
            item.ParentUUID = UUID.Random();
            item.Permissions = new Permissions((uint)PermissionMask.All, (uint)PermissionMask.None, (uint)PermissionMask.None, (uint)PermissionMask.All, (uint)PermissionMask.All);
            item.SalePrice = 0;
            item.SaleType = SaleType.Not;
            item.TransactionID = UUID.Zero;

            return item;
        }
    }
}
