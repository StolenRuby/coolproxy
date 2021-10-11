using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy
{
    public class LLSDAssetResource
    {
        public OSDArray instance_list = new OSDArray();
        public OSDArray texture_list = new OSDArray();
        public OSDArray mesh_list = new OSDArray();
        public string metric = String.Empty;
    }

    public class LLSDAssetUploadRequest
    {
        public string asset_type = String.Empty;
        public string description = String.Empty;
        public UUID folder_id = UUID.Zero;
        public UUID texture_folder_id = UUID.Zero;
        public int next_owner_mask = 0;
        public int group_mask = 0;
        public int everyone_mask = 0;
        public string inventory_type = String.Empty;
        public string name = String.Empty;
        public LLSDAssetResource asset_resources = new LLSDAssetResource();
        public LLSDAssetUploadRequest()
        {
        }
    }
}
