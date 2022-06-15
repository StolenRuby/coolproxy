using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMetaverse
{
    public class TextureEntry
    {
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }
        public BakeType BakeType { get; private set; }
        public string DefaultImage { get; private set; }
        public WearableType WearableType { get; private set; }

        public TextureEntry(string name, bool is_local, BakeType bake_type = BakeType.NumberOfEntries, string default_image = "", WearableType wearable = WearableType.Invalid)
        {
            Name = name;
            IsLocal = is_local;
            BakeType = bake_type;
            DefaultImage = default_image;
            WearableType = wearable;
        }
    }

    public class AppearanceDictionary
    {
        static readonly Dictionary<AvatarTextureIndex, TextureEntry> TextureMap = new Dictionary<AvatarTextureIndex, TextureEntry>()
        {
            {AvatarTextureIndex.HeadBodypaint, new TextureEntry("head_bodypaint", true, BakeType.NumberOfEntries, "", WearableType.Skin) },
            {AvatarTextureIndex.UpperShirt, new TextureEntry("upper_shirt", true, BakeType.NumberOfEntries, "", WearableType.Shirt) },
            {AvatarTextureIndex.LowerPants, new TextureEntry("lower_pants", true, BakeType.NumberOfEntries, "", WearableType.Pants) },
            {AvatarTextureIndex.EyesIris, new TextureEntry("eyes_iris", true, BakeType.NumberOfEntries, "", WearableType.Eyes) },
            {AvatarTextureIndex.Hair, new TextureEntry("hair_grain", true, BakeType.NumberOfEntries, "", WearableType.Hair) },
            {AvatarTextureIndex.UpperBodypaint, new TextureEntry("upper_bodypaint", true, BakeType.NumberOfEntries, "", WearableType.Skin) },
            {AvatarTextureIndex.LowerBodypaint, new TextureEntry("lower_bodypaint", true, BakeType.NumberOfEntries, "", WearableType.Skin) },
            {AvatarTextureIndex.LowerShoes, new TextureEntry("lower_shoes", true, BakeType.NumberOfEntries, "", WearableType.Shoes) },
            {AvatarTextureIndex.LowerSocks, new TextureEntry("lower_socks", true, BakeType.NumberOfEntries, "", WearableType.Socks) },
            {AvatarTextureIndex.UpperJacket, new TextureEntry("upper_jacket", true, BakeType.NumberOfEntries, "", WearableType.Jacket) },
            {AvatarTextureIndex.LowerJacket, new TextureEntry("lower_jacket", true, BakeType.NumberOfEntries, "", WearableType.Jacket) },
            {AvatarTextureIndex.UpperGloves, new TextureEntry("upper_gloves", true, BakeType.NumberOfEntries, "", WearableType.Gloves) },
            {AvatarTextureIndex.UpperUndershirt, new TextureEntry("upper_undershirt", true, BakeType.NumberOfEntries, "", WearableType.Undershirt) },
            {AvatarTextureIndex.LowerUnderpants, new TextureEntry("lower_underpants", true, BakeType.NumberOfEntries, "", WearableType.Underpants) },
            {AvatarTextureIndex.Skirt, new TextureEntry("skirt", true, BakeType.NumberOfEntries, "", WearableType.Skirt) },

            {AvatarTextureIndex.LowerAlpha, new TextureEntry("lower_alpha", true, BakeType.NumberOfEntries, "", WearableType.Alpha) },
            {AvatarTextureIndex.UpperAlpha, new TextureEntry("upper_alpha", true, BakeType.NumberOfEntries, "", WearableType.Alpha) },
            {AvatarTextureIndex.HeadAlpha, new TextureEntry("head_alpha", true, BakeType.NumberOfEntries, "", WearableType.Alpha) },
            {AvatarTextureIndex.EyesAlpha, new TextureEntry("eyes_alpha", true, BakeType.NumberOfEntries, "", WearableType.Alpha) },
            {AvatarTextureIndex.HairAlpha, new TextureEntry("hair_alpha", true, BakeType.NumberOfEntries, "", WearableType.Alpha) },

            {AvatarTextureIndex.HeadTattoo, new TextureEntry("head_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Tattoo) },
            {AvatarTextureIndex.UpperTattoo, new TextureEntry("upper_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Tattoo) },
            {AvatarTextureIndex.LowerTattoo, new TextureEntry("lower_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Tattoo) },

            {AvatarTextureIndex.HeadUnivTattoo, new TextureEntry("head_universal_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.UpperUnivTattoo, new TextureEntry("upper_universal_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.LowerUnivTattoo, new TextureEntry("lower_universal_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.SkirtTattoo, new TextureEntry("skirt_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.HairTattoo, new TextureEntry("hair_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.EyesTattoo, new TextureEntry("eyes_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.LeftArmTattoo, new TextureEntry("leftarm_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.LeftLegTattoo, new TextureEntry("leftleg_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.Aux1Tattoo, new TextureEntry("aux1_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.Aux2Tattoo, new TextureEntry("aux2_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },
            {AvatarTextureIndex.Aux3Tattoo, new TextureEntry("aux3_tattoo", true, BakeType.NumberOfEntries, "", WearableType.Universal) },

            {AvatarTextureIndex.HeadBaked, new TextureEntry("head-baked", false, BakeType.Head, "head") },
            {AvatarTextureIndex.UpperBaked, new TextureEntry("upper-baked", false, BakeType.Head, "upper") },
            {AvatarTextureIndex.LowerBaked, new TextureEntry("lower-baked", false, BakeType.Head, "lower") },
            {AvatarTextureIndex.EyesBaked, new TextureEntry("eyes-baked", false, BakeType.Head, "eyes") },
            {AvatarTextureIndex.HairBaked, new TextureEntry("hair-baked", false, BakeType.Head, "hair") },
            {AvatarTextureIndex.SkirtBaked, new TextureEntry("skirt-baked", false, BakeType.Head, "skirt") },
            {AvatarTextureIndex.LeftArmBaked, new TextureEntry("leftarm-baked", false, BakeType.Head, "leftarm") },
            {AvatarTextureIndex.LeftLegBaked, new TextureEntry("leftleg-baked", false, BakeType.Head, "leftleg") },
            {AvatarTextureIndex.Aux1Baked, new TextureEntry("aux1-baked", false, BakeType.Head, "aux1") },
            {AvatarTextureIndex.Aux2Baked, new TextureEntry("aux2-baked", false, BakeType.Head, "aux2") },
            {AvatarTextureIndex.Aux3Baked, new TextureEntry("aux3-baked", false, BakeType.Head, "aux3") },
        };

        public static TextureEntry getTexture(AvatarTextureIndex index)
        {
            return TextureMap[index];
        }

        public static WearableType getWearbleType(AvatarTextureIndex index)
        {
            return getTexture(index).WearableType;
        }
    }
}
