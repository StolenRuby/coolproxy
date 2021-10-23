using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.MagicRez
{
    public interface IMagicRez
    {
        void Rez(UUID asset_id, Vector3 position, UUID owner_id, UUID creator_id, string description = "", bool grant_perms = false);
    }
}
