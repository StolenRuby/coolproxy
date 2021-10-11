using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy
{
    public static class Util
    {
        public static Encoding UTF8 = Encoding.UTF8;
        public static Encoding UTF8NoBomEncoding = new UTF8Encoding(false);



        // Regions are identified with a 'handle' made up of its world coordinates packed into a ulong.
        // Region handles are based on the coordinate of the region corner with lower X and Y
        // var regions need more work than this to get that right corner from a generic world position
        // this corner must be on a grid point
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong RegionWorldLocToHandle(uint X, uint Y)
        {
            ulong handle = X & 0xffffff00; // make sure it matchs grid coord points.
            handle <<= 32; // to higher half
            handle |= (Y & 0xffffff00);
            return handle;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong RegionGridLocToHandle(uint X, uint Y)
        {
            ulong handle = X;
            handle <<= 40; // shift to higher half and mult by 256)
            handle |= (Y << 8);  // mult by 256)
            return handle;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void RegionHandleToWorldLoc(ulong handle, out uint X, out uint Y)
        {
            X = (uint)(handle >> 32);
            Y = (uint)(handle & 0xfffffffful);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void RegionHandleToRegionLoc(ulong handle, out uint X, out uint Y)
        {
            X = (uint)(handle >> 40) & 0x00ffffffu; //  bring from higher half, divide by 256 and clean
            Y = (uint)(handle >> 8) & 0x00ffffffu; // divide by 256 and clean
            // if you trust the uint cast then the clean can be removed.
        }




        //public static string CalculateMD5Hash(string input)
        //{
        //    // step 1, calculate MD5 hash from input
        //    MD5 md5 = System.Security.Cryptography.MD5.Create();

        //    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

        //    byte[] hash = md5.ComputeHash(inputBytes);

        //    // step 2, convert byte array to hex string
        //    StringBuilder sb = new StringBuilder();

        //    for (int i = 0; i < hash.Length; i++)
        //    {
        //        sb.Append(hash[i].ToString("x2"));

        //    }
        //    return sb.ToString();
        //}


        private static readonly Dictionary<AssetType, string> assetToFileFilter = new Dictionary<AssetType, string>()
        {
            {AssetType.Texture, "Texture|*.tga" },
            {AssetType.Sound, "Sound|*.ogg" },
            {AssetType.CallingCard, "Calling Card|*.callcard" },
            {AssetType.Landmark, "Landmark|*.landmark" },
            {AssetType.LSLText, "Script|*.lsl" },
            {AssetType.Clothing, "Clothing|*.shirt;*.pants;*.shoes;*.socks;*.jacket;*.gloves;*.undershirt;*.underpants;*.skirt;*.alpha;*.tattoo;*.physics" },
            {AssetType.Bodypart, "Body Part|*.hair;*.skin;*.shape;*.eyes" },
            {AssetType.Object, "Object|*.object" },
            {AssetType.Notecard, "Notecard|*.notecard" },
            //{AssetType.LSLText, "lsltext_id" },
            {AssetType.LSLBytecode, "Script Bytecode|*.lslbyte" },
            {AssetType.TextureTGA, "Texture|*.tga" },
            //{AssetType.Bodypart, "Bodypart|*.bodypart" },
            {AssetType.SoundWAV, "Sound|*.wav" },
            {AssetType.ImageTGA, "Texture|*.tga" },
            {AssetType.ImageJPEG, "Texture|*.jpeg" },
            {AssetType.Animation, "Animation|*.anim" },
            {AssetType.Gesture, "Gesture|*.gesture" },
            {AssetType.Mesh, "Mesh|*.mesh" },
            {AssetType.Settings, "Settings|*.sky;*.water;*.daycycle" },
        };

        private static string combinedFilter;

        public static string GetCombinedFilter()
        {
            if(combinedFilter == null)
            {
                combinedFilter = "All Types|*.*|" + string.Join("|", assetToFileFilter.Values);
            }
            return combinedFilter;
        }

        public static string GetFileFilterFromAssetType(AssetType type)
        {
            string filter;
            if (assetToFileFilter.TryGetValue(type, out filter))
            {
                return filter;
            }
            return string.Empty;
        }

        public static string GetExtensionForInventoryType(InventoryType type, uint flags)
        {
            if (type == InventoryType.Settings)
            {
                SettingType settingType = (SettingType)flags;
                return settingType.ToString().ToLower();
            }
            else if (type == InventoryType.Wearable)
            {
                WearableType wearableType = (WearableType)flags;
                return wearableType.ToString().ToLower();
            }
            else
            {
                return type.ToString().ToLower();
            }
        }
    }

    public static class UriExtensions
    {
        public static Uri SetPort(this Uri uri, int newPort)
        {
            var builder = new UriBuilder(uri);
            builder.Port = newPort;
            return builder.Uri;
        }
    }
}
