using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Textures
{
    class TexturesCommand : Command
    {
        public TexturesCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = "textures";
            Name = "List Object Textures";
            Description = "List the textures of your currect selection.";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args)
        {
            List<uint> selection = Proxy.Agent.Selection.ToList();

            if(selection.Count > 0)
            {
                List<UUID> textures = new List<UUID>();
                foreach (uint i in selection)
                {
                    Primitive prim;
                    if (Proxy.Network.CurrentSim.ObjectsPrimitives.TryGetValue(i, out prim))
                    {
                        if(prim.Textures != null)
                        {
                            var def = prim.Textures.DefaultTexture;
                            if(def != null)
                            {
                                if (!textures.Contains(def.TextureID))
                                    textures.Add(def.TextureID);
                            }

                            foreach (var block in prim.Textures.FaceTextures)
                            {
                                if (block != null && block.TextureID != UUID.Zero)
                                {
                                    if (!textures.Contains(block.TextureID))
                                        textures.Add(block.TextureID);
                                }
                            }
                        }
                    }
                }

                if (textures.Count > 0)
                {
                    Proxy.SayToUser("--- " + textures.Count.ToString() + " textures ---");
                    foreach (UUID id in textures)
                        Proxy.SayToUser(id.ToString());
                    Proxy.SayToUser("------");
                }
                else return "No textures!";

                return string.Empty;
            }

            return "No objects selected!";
        }
    }
}
