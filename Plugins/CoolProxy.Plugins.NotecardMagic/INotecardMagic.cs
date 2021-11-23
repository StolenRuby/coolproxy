using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.NotecardMagic
{
    public interface INotecardMagic
    {
        void GetAsset(UUID folder_id, UUID asset_id, AssetType asset_type, InventoryType inventory_type);
    }
}
