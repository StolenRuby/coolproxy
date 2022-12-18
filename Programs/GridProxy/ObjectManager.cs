using OpenMetaverse;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace GridProxy
{
    public class ObjectManager
    {
        private ProxyFrame Frame = null;



        //public Dictionary<uint, Avatar> ObjectsAvatars = new Dictionary<uint, Avatar>();

        //public Dictionary<uint, Primitive> ObjectsPrimitives = new Dictionary<uint, Primitive>();


        #region Agent Info
        //internal uint localID;
        //internal Vector3 relativePosition;
        //internal Quaternion relativeRotation = Quaternion.Identity;
        //internal Vector4 collisionPlane;
        //internal Vector3 velocity;
        //internal Vector3 acceleration;
        //internal Vector3 angularVelocity;
        //internal uint sittingOn;
        //internal int lastInterpolation;
        #endregion

        //internal UUID CurrentSelection = UUID.Zero;

        //internal List<uint> SelectedLocalIDs = new List<uint>();

        //public uint[] Selection
        //{
        //    get
        //    {
        //        return SelectedLocalIDs.ToArray();
        //    }
        //}

        public ObjectManager(ProxyFrame frame)
        {
            this.Frame = frame;

            Frame.Network.AddDelegate(PacketType.ObjectUpdate, Direction.Incoming, ObjectUpdateHandler);
            Frame.Network.AddDelegate(PacketType.ImprovedTerseObjectUpdate, Direction.Incoming, ImprovedTerseObjectUpdateHandler);
            Frame.Network.AddDelegate(PacketType.ObjectUpdateCompressed, Direction.Incoming, ObjectUpdateCompressedHandler);
            Frame.Network.AddDelegate(PacketType.ObjectUpdateCached, Direction.Incoming, ObjectUpdateCachedHandler);
            Frame.Network.AddDelegate(PacketType.KillObject, Direction.Incoming, KillObjectHandler);
            Frame.Network.AddDelegate(PacketType.ObjectPropertiesFamily, Direction.Incoming, ObjectPropertiesFamilyHandler);
            Frame.Network.AddDelegate(PacketType.ObjectProperties, Direction.Incoming, ObjectPropertiesHandler);
            Frame.Network.AddDelegate(PacketType.PayPriceReply, Direction.Incoming, PayPriceReplyHandler);
            // Client.Network.RegisterEventCallback("ObjectPhysicsProperties", ObjectPhysicsPropertiesHandler);

            this.KillObject += CopybotPlugin_KillObject;

            //Frame.Regions.AddDelegate(PacketType.ObjectUpdateCached, Direction.Incoming, (packet, sim) => 
            //{
            //    ObjectUpdateCachedPacket ouc = (ObjectUpdateCachedPacket)packet;

            //    foreach(var block in ouc.ObjectData)
            //    {
            //        block.CRC = 69;
            //    }

            //    return ouc;
            //});
        }
        
        private void CopybotPlugin_KillObject(object sender, KillObjectEventArgs e)
        {
            if (Frame.Agent.SelectedLocalIDs.Contains(e.ObjectLocalID))
                Frame.Agent.SelectedLocalIDs.Remove(e.ObjectLocalID);
        }

        #region Public Methods

        /// <summary>
        /// Request information for a single object from a <see cref="RegionProxy"/> 
        /// you are currently connected to
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>
        /// <param name="localID">The Local ID of the object</param>
        public void RequestObject(RegionProxy simulator, uint localID)
        {
            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[1];
            request.ObjectData[0] = new RequestMultipleObjectsPacket.ObjectDataBlock();
            request.ObjectData[0].ID = localID;
            request.ObjectData[0].CacheMissType = 0;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request information for multiple objects contained in
        /// the same simulator
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the objects are located</param>
        /// <param name="localIDs">An array containing the Local IDs of the objects</param>
        public void RequestObjects(RegionProxy simulator, List<uint> localIDs)
        {
            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                request.ObjectData[i] = new RequestMultipleObjectsPacket.ObjectDataBlock();
                request.ObjectData[i].ID = localIDs[i];
                request.ObjectData[i].CacheMissType = 0;
            }

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Attempt to purchase an original object, a copy, or the contents of
        /// an object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>
        /// <param name="saleType">Whether the original, a copy, or the object
        /// contents are on sale. This is used for verification, if the this
        /// sale type is not valid for the object the purchase will fail</param>
        /// <param name="price">Price of the object. This is used for 
        /// verification, if it does not match the actual price the purchase
        /// will fail</param>
        /// <param name="groupID">Group ID that will be associated with the new
        /// purchase</param>
        /// <param name="categoryID">Inventory folder UUID where the object or objects 
        /// purchased should be placed</param>
        /// <example>
        /// <code>
        ///     BuyObject(Client.Network.CurrentSim, 500, SaleType.Copy, 
        ///         100, UUID.Zero, Frame.Agent.InventoryRootFolderUUID);
        /// </code> 
        ///</example>
        public void BuyObject(RegionProxy simulator, uint localID, SaleType saleType, int price, UUID groupID,
            UUID categoryID)
        {
            ObjectBuyPacket buy = new ObjectBuyPacket();

            buy.AgentData.AgentID = Frame.Agent.AgentID;
            buy.AgentData.SessionID = Frame.Agent.SessionID;
            buy.AgentData.GroupID = groupID;
            buy.AgentData.CategoryID = categoryID;

            buy.ObjectData = new ObjectBuyPacket.ObjectDataBlock[1];
            buy.ObjectData[0] = new ObjectBuyPacket.ObjectDataBlock();
            buy.ObjectData[0].ObjectLocalID = localID;
            buy.ObjectData[0].SaleType = (byte)saleType;
            buy.ObjectData[0].SalePrice = price;

            simulator.Inject(buy, Direction.Outgoing);
        }

        /// <summary>
        /// Request prices that should be displayed in pay dialog. This will triggger the simulator
        /// to send us back a PayPriceReply which can be handled by OnPayPriceReply event
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>
        /// <param name="objectID">The ID of the object</param>
        /// <remarks>The result is raised in the <see cref="PayPriceReply"/> event</remarks>
        public void RequestPayPrice(RegionProxy simulator, UUID objectID)
        {
            RequestPayPricePacket payPriceRequest = new RequestPayPricePacket();

            payPriceRequest.ObjectData = new RequestPayPricePacket.ObjectDataBlock();
            payPriceRequest.ObjectData.ObjectID = objectID;

            simulator.Inject(payPriceRequest, Direction.Outgoing);
        }

        /// <summary>
        /// Select a single object. This will cause the <see cref="RegionProxy"/> to send us 
        /// an <see cref="ObjectPropertiesPacket"/> which will raise the <see cref="ObjectProperties"/> event
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>        
        /// <seealso cref="ObjectPropertiesFamilyEventArgs"/>
        public void SelectObject(RegionProxy simulator, uint localID)
        {
            SelectObject(simulator, localID, true);
        }

        /// <summary>
        /// Select a single object. This will cause the <see cref="RegionProxy"/> to send us 
        /// an <see cref="ObjectPropertiesPacket"/> which will raise the <see cref="ObjectProperties"/> event
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>
        /// <param name="localID">The Local ID of the object</param>
        /// <param name="automaticDeselect">if true, a call to <see cref="DeselectObject"/> is
        /// made immediately following the request</param>
        /// <seealso cref="ObjectPropertiesFamilyEventArgs"/>
        public void SelectObject(RegionProxy simulator, uint localID, bool automaticDeselect)
        {
            ObjectSelectPacket select = new ObjectSelectPacket();

            select.AgentData.AgentID = Frame.Agent.AgentID;
            select.AgentData.SessionID = Frame.Agent.SessionID;

            select.ObjectData = new ObjectSelectPacket.ObjectDataBlock[1];
            select.ObjectData[0] = new ObjectSelectPacket.ObjectDataBlock();
            select.ObjectData[0].ObjectLocalID = localID;

            simulator.Inject(select, Direction.Outgoing);

            if (automaticDeselect)
            {
                DeselectObject(simulator, localID);
            }
        }

        /// <summary>
        /// Select multiple objects. This will cause the <see cref="RegionProxy"/> to send us 
        /// an <see cref="ObjectPropertiesPacket"/> which will raise the <see cref="ObjectProperties"/> event
        /// </summary>        
        /// <param name="simulator">The <see cref="RegionProxy"/> the objects are located</param> 
        /// <param name="localIDs">An array containing the Local IDs of the objects</param>
        /// <param name="automaticDeselect">Should objects be deselected immediately after selection</param>
        /// <seealso cref="ObjectPropertiesFamilyEventArgs"/>
        public void SelectObjects(RegionProxy simulator, uint[] localIDs, bool automaticDeselect)
        {
            ObjectSelectPacket select = new ObjectSelectPacket();

            select.AgentData.AgentID = Frame.Agent.AgentID;
            select.AgentData.SessionID = Frame.Agent.SessionID;

            select.ObjectData = new ObjectSelectPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; i++)
            {
                select.ObjectData[i] = new ObjectSelectPacket.ObjectDataBlock();
                select.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            simulator.Inject(select, Direction.Outgoing);

            if (automaticDeselect)
            {
                DeselectObjects(simulator, localIDs);
            }
        }

        /// <summary>
        /// Select multiple objects. This will cause the <see cref="RegionProxy"/> to send us 
        /// an <see cref="ObjectPropertiesPacket"/> which will raise the <see cref="ObjectProperties"/> event
        /// </summary>        
        /// <param name="simulator">The <see cref="RegionProxy"/> the objects are located</param> 
        /// <param name="localIDs">An array containing the Local IDs of the objects</param>
        /// <seealso cref="ObjectPropertiesFamilyEventArgs"/>
        public void SelectObjects(RegionProxy simulator, uint[] localIDs)
        {
            SelectObjects(simulator, localIDs, true);
        }

        /// <summary>
        /// Update the properties of an object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>        
        /// <param name="physical">true to turn the objects physical property on</param>
        /// <param name="temporary">true to turn the objects temporary property on</param>
        /// <param name="phantom">true to turn the objects phantom property on</param>
        /// <param name="castsShadow">true to turn the objects cast shadows property on</param>
        public void SetFlags(RegionProxy simulator, uint localID, bool physical, bool temporary, bool phantom, bool castsShadow)
        {
            ObjectFlagUpdatePacket flags = new ObjectFlagUpdatePacket();
            flags.AgentData.AgentID = Frame.Agent.AgentID;
            flags.AgentData.SessionID = Frame.Agent.SessionID;
            flags.AgentData.ObjectLocalID = localID;
            flags.AgentData.UsePhysics = physical;
            flags.AgentData.IsTemporary = temporary;
            flags.AgentData.IsPhantom = phantom;
            flags.AgentData.CastsShadows = castsShadow;

            flags.ExtraPhysics = new ObjectFlagUpdatePacket.ExtraPhysicsBlock[0];

            simulator.Inject(flags, Direction.Outgoing);

            // SetFlags(simulator, localID, physical, temporary, phantom, castsShadow, PhysicsShapeType.Prim, 1000f, 0.6f, 0.5f, 1f);
        }

        /// <summary>
        /// Update the properties of an object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>        
        /// <param name="physical">true to turn the objects physical property on</param>
        /// <param name="temporary">true to turn the objects temporary property on</param>
        /// <param name="phantom">true to turn the objects phantom property on</param>
        /// <param name="castsShadow">true to turn the objects cast shadows property on</param>
        /// <param name="physicsType">Type of the represetnation prim will have in the physics engine</param>
        /// <param name="density">Density - normal value 1000</param>
        /// <param name="friction">Friction - normal value 0.6</param>
        /// <param name="restitution">Restitution - standard value 0.5</param>
        /// <param name="gravityMultiplier">Gravity multiplier - standar value 1.0</param>
        public void SetFlags(RegionProxy simulator, uint localID, bool physical, bool temporary, bool phantom, bool castsShadow,
            PhysicsShapeType physicsType, float density, float friction, float restitution, float gravityMultiplier)
        {
            ObjectFlagUpdatePacket flags = new ObjectFlagUpdatePacket();
            flags.AgentData.AgentID = Frame.Agent.AgentID;
            flags.AgentData.SessionID = Frame.Agent.SessionID;
            flags.AgentData.ObjectLocalID = localID;
            flags.AgentData.UsePhysics = physical;
            flags.AgentData.IsTemporary = temporary;
            flags.AgentData.IsPhantom = phantom;
            flags.AgentData.CastsShadows = castsShadow;

            flags.ExtraPhysics = new ObjectFlagUpdatePacket.ExtraPhysicsBlock[1];
            flags.ExtraPhysics[0] = new ObjectFlagUpdatePacket.ExtraPhysicsBlock();
            flags.ExtraPhysics[0].PhysicsShapeType = (byte)physicsType;
            flags.ExtraPhysics[0].Density = density;
            flags.ExtraPhysics[0].Friction = friction;
            flags.ExtraPhysics[0].Restitution = restitution;
            flags.ExtraPhysics[0].GravityMultiplier = gravityMultiplier;

            simulator.Inject(flags, Direction.Outgoing);
        }

        /// <summary>
        /// Sets the sale properties of a single object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>        
        /// <param name="saleType">One of the options from the <see cref="SaleType"/> enum</param>
        /// <param name="price">The price of the object</param>
        public void SetSaleInfo(RegionProxy simulator, uint localID, SaleType saleType, int price)
        {
            ObjectSaleInfoPacket sale = new ObjectSaleInfoPacket();
            sale.AgentData.AgentID = Frame.Agent.AgentID;
            sale.AgentData.SessionID = Frame.Agent.SessionID;
            sale.ObjectData = new ObjectSaleInfoPacket.ObjectDataBlock[1];
            sale.ObjectData[0] = new ObjectSaleInfoPacket.ObjectDataBlock();
            sale.ObjectData[0].LocalID = localID;
            sale.ObjectData[0].SalePrice = price;
            sale.ObjectData[0].SaleType = (byte)saleType;

            simulator.Inject(sale, Direction.Outgoing);
        }

        /// <summary>
        /// Sets the sale properties of multiple objects
        /// </summary>        
        /// <param name="simulator">The <see cref="RegionProxy"/> the objects are located</param> 
        /// <param name="localIDs">An array containing the Local IDs of the objects</param>
        /// <param name="saleType">One of the options from the <see cref="SaleType"/> enum</param>
        /// <param name="price">The price of the object</param>
        public void SetSaleInfo(RegionProxy simulator, List<uint> localIDs, SaleType saleType, int price)
        {
            ObjectSaleInfoPacket sale = new ObjectSaleInfoPacket();
            sale.AgentData.AgentID = Frame.Agent.AgentID;
            sale.AgentData.SessionID = Frame.Agent.SessionID;
            sale.ObjectData = new ObjectSaleInfoPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                sale.ObjectData[i] = new ObjectSaleInfoPacket.ObjectDataBlock();
                sale.ObjectData[i].LocalID = localIDs[i];
                sale.ObjectData[i].SalePrice = price;
                sale.ObjectData[i].SaleType = (byte)saleType;
            }

            simulator.Inject(sale, Direction.Outgoing);
        }

        /// <summary>
        /// Deselect a single object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>
        public void DeselectObject(RegionProxy simulator, uint localID)
        {
            ObjectDeselectPacket deselect = new ObjectDeselectPacket();

            deselect.AgentData.AgentID = Frame.Agent.AgentID;
            deselect.AgentData.SessionID = Frame.Agent.SessionID;

            deselect.ObjectData = new ObjectDeselectPacket.ObjectDataBlock[1];
            deselect.ObjectData[0] = new ObjectDeselectPacket.ObjectDataBlock();
            deselect.ObjectData[0].ObjectLocalID = localID;

            simulator.Inject(deselect, Direction.Outgoing);
        }

        /// <summary>
        /// Deselect multiple objects.
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the objects are located</param> 
        /// <param name="localIDs">An array containing the Local IDs of the objects</param>
        public void DeselectObjects(RegionProxy simulator, uint[] localIDs)
        {
            ObjectDeselectPacket deselect = new ObjectDeselectPacket();

            deselect.AgentData.AgentID = Frame.Agent.AgentID;
            deselect.AgentData.SessionID = Frame.Agent.SessionID;

            deselect.ObjectData = new ObjectDeselectPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; i++)
            {
                deselect.ObjectData[i] = new ObjectDeselectPacket.ObjectDataBlock();
                deselect.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            simulator.Inject(deselect, Direction.Outgoing);
        }

        /// <summary>
        /// Perform a click action on an object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>
        public void ClickObject(RegionProxy simulator, uint localID)
        {
            ClickObject(simulator, localID, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Perform a click action (Grab) on a single object
        /// </summary>
        /// <param name="simulator">The <see cref="RegionProxy"/> the object is located</param>        
        /// <param name="localID">The Local ID of the object</param>
        /// <param name="uvCoord">The texture coordinates to touch</param>
        /// <param name="stCoord">The surface coordinates to touch</param>
        /// <param name="faceIndex">The face of the position to touch</param>
        /// <param name="position">The region coordinates of the position to touch</param>
        /// <param name="normal">The surface normal of the position to touch (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to touch (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void ClickObject(RegionProxy simulator, uint localID, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Frame.Agent.AgentID;
            grab.AgentData.SessionID = Frame.Agent.SessionID;
            grab.ObjectData.GrabOffset = Vector3.Zero;
            grab.ObjectData.LocalID = localID;
            grab.SurfaceInfo = new ObjectGrabPacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabPacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            simulator.Inject(grab, Direction.Outgoing);

            // TODO: If these hit the server out of order the click will fail 
            // and we'll be grabbing the object
            Thread.Sleep(50);

            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Frame.Agent.AgentID;
            degrab.AgentData.SessionID = Frame.Agent.SessionID;
            degrab.ObjectData.LocalID = localID;
            degrab.SurfaceInfo = new ObjectDeGrabPacket.SurfaceInfoBlock[1];
            degrab.SurfaceInfo[0] = new ObjectDeGrabPacket.SurfaceInfoBlock();
            degrab.SurfaceInfo[0].UVCoord = uvCoord;
            degrab.SurfaceInfo[0].STCoord = stCoord;
            degrab.SurfaceInfo[0].FaceIndex = faceIndex;
            degrab.SurfaceInfo[0].Position = position;
            degrab.SurfaceInfo[0].Normal = normal;
            degrab.SurfaceInfo[0].Binormal = binormal;

            simulator.Inject(degrab, Direction.Outgoing);
        }

        /// <summary>
        /// Create (rez) a new prim object in a simulator
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object to place the object in</param>
        /// <param name="prim">Data describing the prim object to rez</param>
        /// <param name="groupID">Group ID that this prim will be set to, or UUID.Zero if you
        /// do not want the object to be associated with a specific group</param>
        /// <param name="position">An approximation of the position at which to rez the prim</param>
        /// <param name="scale">Scale vector to size this prim</param>
        /// <param name="rotation">Rotation quaternion to rotate this prim</param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created. This
        /// function will not set textures, light and flexible data, or other 
        /// extended primitive properties</remarks>
        public void AddPrim(RegionProxy simulator, Primitive.ConstructionData prim, UUID groupID, Vector3 position,
            Vector3 scale, Quaternion rotation)
        {
            AddPrim(simulator, prim, groupID, position, scale, rotation, PrimFlags.CreateSelected);
        }

        /// <summary>
        /// Create (rez) a new prim object in a simulator
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="RegionProxy"/> object to place the object in</param>
        /// <param name="prim">Data describing the prim object to rez</param>
        /// <param name="groupID">Group ID that this prim will be set to, or UUID.Zero if you
        /// do not want the object to be associated with a specific group</param>
        /// <param name="position">An approximation of the position at which to rez the prim</param>
        /// <param name="scale">Scale vector to size this prim</param>
        /// <param name="rotation">Rotation quaternion to rotate this prim</param>
        /// <param name="createFlags">Specify the <seealso cref="PrimFlags"/></param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created. This
        /// function will not set textures, light and flexible data, or other 
        /// extended primitive properties</remarks>
        public void AddPrim(RegionProxy simulator, Primitive.ConstructionData prim, UUID groupID, Vector3 position,
            Vector3 scale, Quaternion rotation, PrimFlags createFlags)
        {
            ObjectAddPacket packet = new ObjectAddPacket();
            packet.Header.Reliable = true;

            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;
            packet.AgentData.GroupID = groupID;

            packet.ObjectData.State = prim.State;
            packet.ObjectData.AddFlags = (uint)createFlags;
            packet.ObjectData.PCode = (byte)PCode.Prim;

            packet.ObjectData.Material = (byte)prim.Material;
            packet.ObjectData.Scale = scale;
            packet.ObjectData.Rotation = rotation;

            packet.ObjectData.PathCurve = (byte)prim.PathCurve;
            packet.ObjectData.PathBegin = Primitive.PackBeginCut(prim.PathBegin);
            packet.ObjectData.PathEnd = Primitive.PackEndCut(prim.PathEnd);
            packet.ObjectData.PathRadiusOffset = Primitive.PackPathTwist(prim.PathRadiusOffset);
            packet.ObjectData.PathRevolutions = Primitive.PackPathRevolutions(prim.PathRevolutions);
            packet.ObjectData.PathScaleX = Primitive.PackPathScale(prim.PathScaleX);
            packet.ObjectData.PathScaleY = Primitive.PackPathScale(prim.PathScaleY);
            packet.ObjectData.PathShearX = (byte)Primitive.PackPathShear(prim.PathShearX);
            packet.ObjectData.PathShearY = (byte)Primitive.PackPathShear(prim.PathShearY);
            packet.ObjectData.PathSkew = Primitive.PackPathTwist(prim.PathSkew);
            packet.ObjectData.PathTaperX = Primitive.PackPathTaper(prim.PathTaperX);
            packet.ObjectData.PathTaperY = Primitive.PackPathTaper(prim.PathTaperY);
            packet.ObjectData.PathTwist = Primitive.PackPathTwist(prim.PathTwist);
            packet.ObjectData.PathTwistBegin = Primitive.PackPathTwist(prim.PathTwistBegin);

            packet.ObjectData.ProfileCurve = prim.profileCurve;
            packet.ObjectData.ProfileBegin = Primitive.PackBeginCut(prim.ProfileBegin);
            packet.ObjectData.ProfileEnd = Primitive.PackEndCut(prim.ProfileEnd);
            packet.ObjectData.ProfileHollow = Primitive.PackProfileHollow(prim.ProfileHollow);

            packet.ObjectData.RayStart = position;
            packet.ObjectData.RayEnd = position;
            packet.ObjectData.RayEndIsIntersection = 0;
            packet.ObjectData.RayTargetID = UUID.Zero;
            packet.ObjectData.BypassRaycast = 1;

            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Rez a Linden tree
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="scale">The size of the tree</param>
        /// <param name="rotation">The rotation of the tree</param>
        /// <param name="position">The position of the tree</param>
        /// <param name="treeType">The Type of tree</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to set the tree to, 
        /// or UUID.Zero if no group is to be set</param>
        /// <param name="newTree">true to use the "new" Linden trees, false to use the old</param>
        public void AddTree(RegionProxy simulator, Vector3 scale, Quaternion rotation, Vector3 position,
            Tree treeType, UUID groupOwner, bool newTree)
        {
            ObjectAddPacket add = new ObjectAddPacket();

            add.AgentData.AgentID = Frame.Agent.AgentID;
            add.AgentData.SessionID = Frame.Agent.SessionID;
            add.AgentData.GroupID = groupOwner;
            add.ObjectData.BypassRaycast = 1;
            add.ObjectData.Material = 3;
            add.ObjectData.PathCurve = 16;
            add.ObjectData.PCode = newTree ? (byte)PCode.NewTree : (byte)PCode.Tree;
            add.ObjectData.RayEnd = position;
            add.ObjectData.RayStart = position;
            add.ObjectData.RayTargetID = UUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)treeType;

            simulator.Inject(add, Direction.Outgoing);
        }

        /// <summary>
        /// Rez grass and ground cover
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="scale">The size of the grass</param>
        /// <param name="rotation">The rotation of the grass</param>
        /// <param name="position">The position of the grass</param>
        /// <param name="grassType">The type of grass from the <seealso cref="Grass"/> enum</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to set the tree to, 
        /// or UUID.Zero if no group is to be set</param>
        public void AddGrass(RegionProxy simulator, Vector3 scale, Quaternion rotation, Vector3 position,
            Grass grassType, UUID groupOwner)
        {
            ObjectAddPacket add = new ObjectAddPacket();

            add.AgentData.AgentID = Frame.Agent.AgentID;
            add.AgentData.SessionID = Frame.Agent.SessionID;
            add.AgentData.GroupID = groupOwner;
            add.ObjectData.BypassRaycast = 1;
            add.ObjectData.Material = 3;
            add.ObjectData.PathCurve = 16;
            add.ObjectData.PCode = (byte)PCode.Grass;
            add.ObjectData.RayEnd = position;
            add.ObjectData.RayStart = position;
            add.ObjectData.RayTargetID = UUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)grassType;

            simulator.Inject(add, Direction.Outgoing);
        }

        public void DuplicateObject(RegionProxy simulator, uint local_id, Vector3 offset, PrimFlags flags)
        {
            ObjectDuplicatePacket dup = new ObjectDuplicatePacket();
            dup.Header.Reliable = true;
            dup.AgentData.AgentID = Frame.Agent.AgentID;
            dup.AgentData.SessionID = Frame.Agent.SessionID;
            dup.SharedData.Offset = offset;
            dup.SharedData.DuplicateFlags = (uint)flags;
            dup.ObjectData = new ObjectDuplicatePacket.ObjectDataBlock[1];
            dup.ObjectData[0] = new ObjectDuplicatePacket.ObjectDataBlock();
            dup.ObjectData[0].ObjectLocalID = local_id;

            simulator.Inject(dup, Direction.Outgoing);
        }

        public void RezInventoryItem(RegionProxy simulator, InventoryItem item, Vector3 pos)
        {
            RezObjectPacket rez = new RezObjectPacket();
            rez.AgentData.AgentID = Frame.Agent.AgentID;
            rez.AgentData.SessionID = Frame.Agent.SessionID;

            rez.RezData.BypassRaycast = 1;
            rez.RezData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            rez.RezData.GroupMask = (uint)item.Permissions.GroupMask;
            rez.RezData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            rez.RezData.FromTaskID = UUID.Zero;
            rez.RezData.ItemFlags = item.Flags;
            rez.RezData.RayEnd = pos;
            rez.RezData.RayEndIsIntersection = false;
            rez.RezData.RayStart = pos;
            rez.RezData.RayTargetID = UUID.Zero;
            rez.RezData.RemoveItem = false;
            rez.RezData.RezSelected = true;

            rez.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            rez.InventoryData.CreatorID = item.CreatorID;
            rez.InventoryData.Flags = item.Flags;
            rez.InventoryData.ItemID = item.UUID;
            rez.InventoryData.FolderID = item.ParentUUID;
            rez.InventoryData.GroupID = item.GroupID;
            rez.InventoryData.GroupOwned = item.GroupOwned;
            rez.InventoryData.InvType = (sbyte)item.InventoryType;
            rez.InventoryData.Name = Utils.StringToBytes(item.Name);
            rez.InventoryData.Description = Utils.StringToBytes(item.Description);
            rez.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            rez.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            rez.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            rez.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            rez.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            rez.InventoryData.OwnerID = item.OwnerID;
            rez.InventoryData.SalePrice = item.SalePrice;
            rez.InventoryData.SaleType = (byte)item.SaleType;
            rez.InventoryData.TransactionID = UUID.Zero;
            rez.InventoryData.Type = 0;
            rez.InventoryData.CRC = InventoryManager.ItemCRC(item);

            simulator.Inject(rez, Direction.Outgoing);
        }

        /// <summary>
        /// Set the textures to apply to the faces of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="textures">The texture data to apply</param>
        public void SetTextures(RegionProxy simulator, uint localID, Primitive.TextureEntry textures)
        {
            SetTextures(simulator, localID, textures, String.Empty);
        }

        /// <summary>
        /// Set the textures to apply to the faces of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="textures">The texture data to apply</param>
        /// <param name="mediaUrl">A media URL (not used)</param>
        public void SetTextures(RegionProxy simulator, uint localID, Primitive.TextureEntry textures, string mediaUrl)
        {
            ObjectImagePacket image = new ObjectImagePacket();
            image.Header.Reliable = true;

            image.AgentData.AgentID = Frame.Agent.AgentID;
            image.AgentData.SessionID = Frame.Agent.SessionID;
            image.ObjectData = new ObjectImagePacket.ObjectDataBlock[1];
            image.ObjectData[0] = new ObjectImagePacket.ObjectDataBlock();
            image.ObjectData[0].ObjectLocalID = localID;
            image.ObjectData[0].TextureEntry = textures.GetBytes();
            image.ObjectData[0].MediaURL = Utils.StringToBytes(mediaUrl);

            simulator.Inject(image, Direction.Outgoing);
        }

        public void SetShape(RegionProxy region, uint local, Primitive.ConstructionData data)
        {
            ObjectShapePacket packet = new ObjectShapePacket();
            packet.Header.Reliable = true;
            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            packet.ObjectData = new ObjectShapePacket.ObjectDataBlock[1];
            packet.ObjectData[0] = new ObjectShapePacket.ObjectDataBlock();
            packet.ObjectData[0].ObjectLocalID = local;
            packet.ObjectData[0].PathCurve = (byte)data.PathCurve;
            packet.ObjectData[0].PathBegin = Primitive.PackBeginCut(data.PathBegin);
            packet.ObjectData[0].PathEnd = Primitive.PackEndCut(data.PathEnd);
            packet.ObjectData[0].PathRadiusOffset = Primitive.PackPathTwist(data.PathRadiusOffset);
            packet.ObjectData[0].PathRevolutions = Primitive.PackPathRevolutions(data.PathRevolutions);
            packet.ObjectData[0].PathScaleX = Primitive.PackPathScale(data.PathScaleX);
            packet.ObjectData[0].PathScaleY = Primitive.PackPathScale(data.PathScaleY);
            packet.ObjectData[0].PathShearX = (byte)Primitive.PackPathShear(data.PathShearX);
            packet.ObjectData[0].PathShearY = (byte)Primitive.PackPathShear(data.PathShearY);
            packet.ObjectData[0].PathSkew = Primitive.PackPathTwist(data.PathSkew);
            packet.ObjectData[0].PathTaperX = Primitive.PackPathTaper(data.PathTaperX);
            packet.ObjectData[0].PathTaperY = Primitive.PackPathTaper(data.PathTaperY);
            packet.ObjectData[0].PathTwist = Primitive.PackPathTwist(data.PathTwist);
            packet.ObjectData[0].PathTwistBegin = Primitive.PackPathTwist(data.PathTwistBegin);
            packet.ObjectData[0].ProfileCurve = data.profileCurve;
            packet.ObjectData[0].ProfileBegin = Primitive.PackBeginCut(data.ProfileBegin);
            packet.ObjectData[0].ProfileEnd = Primitive.PackEndCut(data.ProfileEnd);
            packet.ObjectData[0].ProfileHollow = Primitive.PackProfileHollow(data.ProfileHollow);

            region.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Set the Light data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="light">A <seealso cref="Primitive.LightData"/> object containing the data to set</param>
        public void SetLight(RegionProxy simulator, uint localID, Primitive.LightData light)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();
            extra.Header.Reliable = true;

            extra.AgentData.AgentID = Frame.Agent.AgentID;
            extra.AgentData.SessionID = Frame.Agent.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Light;
            if (light.Intensity == 0.0f)
            {
                // Disables the light if intensity is 0
                extra.ObjectData[0].ParamInUse = false;
            }
            else
            {
                extra.ObjectData[0].ParamInUse = true;
            }
            extra.ObjectData[0].ParamData = light.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            simulator.Inject(extra, Direction.Outgoing);
        }

        /// <summary>
        /// Set the flexible data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="flexible">A <seealso cref="Primitive.FlexibleData"/> object containing the data to set</param>
        public void SetFlexible(RegionProxy simulator, uint localID, Primitive.FlexibleData flexible)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();
            extra.Header.Reliable = true;

            extra.AgentData.AgentID = Frame.Agent.AgentID;
            extra.AgentData.SessionID = Frame.Agent.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Flexible;
            extra.ObjectData[0].ParamInUse = true;
            extra.ObjectData[0].ParamData = flexible.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            simulator.Inject(extra, Direction.Outgoing);
        }

        /// <summary>
        /// Set the sculptie texture and data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="sculpt">A <seealso cref="Primitive.SculptData"/> object containing the data to set</param>
        public void SetSculpt(RegionProxy simulator, uint localID, Primitive.SculptData sculpt)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();
            extra.Header.Reliable = true;

            extra.AgentData.AgentID = Frame.Agent.AgentID;
            extra.AgentData.SessionID = Frame.Agent.SessionID;

            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Sculpt;
            extra.ObjectData[0].ParamInUse = true;
            extra.ObjectData[0].ParamData = sculpt.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            simulator.Inject(extra, Direction.Outgoing);

            if (sculpt.Type == SculptType.Mesh) return;

            // Not sure why, but if you don't send this the sculpted prim disappears
            ObjectShapePacket shape = new ObjectShapePacket();
            shape.Header.Reliable = true;

            shape.AgentData.AgentID = Frame.Agent.AgentID;
            shape.AgentData.SessionID = Frame.Agent.SessionID;

            shape.ObjectData = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock[1];
            shape.ObjectData[0] = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock();
            shape.ObjectData[0].ObjectLocalID = localID;
            shape.ObjectData[0].PathScaleX = 100;
            shape.ObjectData[0].PathScaleY = 150;
            shape.ObjectData[0].PathCurve = 32;

            simulator.Inject(shape, Direction.Outgoing);
        }

        /// <summary>
        /// Unset additional primitive parameters on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="type">The extra parameters to set</param>
        public void SetExtraParamOff(RegionProxy simulator, uint localID, ExtraParamType type)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Frame.Agent.AgentID;
            extra.AgentData.SessionID = Frame.Agent.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)type;
            extra.ObjectData[0].ParamInUse = false;
            extra.ObjectData[0].ParamData = Utils.EmptyBytes;
            extra.ObjectData[0].ParamSize = 0;

            simulator.Inject(extra, Direction.Outgoing);
        }

        public void SetExtraParams(RegionProxy simulator, uint localID, Primitive.SculptData sculpt, Primitive.FlexibleData flex, Primitive.LightData light)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();
            extra.Header.Reliable = true;

            extra.AgentData.AgentID = Frame.Agent.AgentID;
            extra.AgentData.SessionID = Frame.Agent.SessionID;

            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[3];

            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Sculpt;
            extra.ObjectData[0].ParamInUse = sculpt != null;
            extra.ObjectData[0].ParamData = sculpt == null ? Utils.EmptyBytes : sculpt.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            extra.ObjectData[1] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[1].ObjectLocalID = localID;
            extra.ObjectData[1].ParamType = (byte)ExtraParamType.Light;
            extra.ObjectData[1].ParamInUse = light != null;
            extra.ObjectData[1].ParamData = light == null ? Utils.EmptyBytes : light.GetBytes();
            extra.ObjectData[1].ParamSize = (uint)extra.ObjectData[1].ParamData.Length;

            extra.ObjectData[2] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[2].ObjectLocalID = localID;
            extra.ObjectData[2].ParamType = (byte)ExtraParamType.Flexible;
            extra.ObjectData[2].ParamInUse = flex != null;
            extra.ObjectData[2].ParamData = flex == null ? Utils.EmptyBytes : flex.GetBytes();
            extra.ObjectData[2].ParamSize = (uint)extra.ObjectData[2].ParamData.Length;

            simulator.Inject(extra, Direction.Outgoing);
        }

        /// <summary>
        /// Link multiple prims into a linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to link</param>
        /// <remarks>The last object in the array will be the root object of the linkset TODO: Is this true?</remarks>
        public void LinkPrims(RegionProxy simulator, List<uint> localIDs)
        {
            ObjectLinkPacket packet = new ObjectLinkPacket();
            packet.Header.Reliable = true;

            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            packet.ObjectData = new ObjectLinkPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectLinkPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Delink/Unlink multiple prims from a linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to delink</param>
        public void DelinkPrims(RegionProxy simulator, List<uint> localIDs)
        {
            ObjectDelinkPacket packet = new ObjectDelinkPacket();

            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            packet.ObjectData = new ObjectDelinkPacket.ObjectDataBlock[localIDs.Count];

            int i = 0;
            foreach (uint localID in localIDs)
            {
                packet.ObjectData[i] = new ObjectDelinkPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localID;

                i++;
            }

            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Change the rotation of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="rotation">The new rotation of the object</param>
        public void SetRotation(RegionProxy simulator, uint localID, Quaternion rotation)
        {
            ObjectRotationPacket objRotPacket = new ObjectRotationPacket();
            objRotPacket.Header.Reliable = true;
            objRotPacket.AgentData.AgentID = Frame.Agent.AgentID;
            objRotPacket.AgentData.SessionID = Frame.Agent.SessionID;

            objRotPacket.ObjectData = new ObjectRotationPacket.ObjectDataBlock[1];

            objRotPacket.ObjectData[0] = new ObjectRotationPacket.ObjectDataBlock();
            objRotPacket.ObjectData[0].ObjectLocalID = localID;
            objRotPacket.ObjectData[0].Rotation = rotation;
            simulator.Inject(objRotPacket, Direction.Outgoing);
        }

        /// <summary>
        /// Set the name of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="name">A string containing the new name of the object</param>
        public void SetName(RegionProxy simulator, uint localID, string name)
        {
            SetNames(simulator, new uint[] { localID }, new string[] { name });
        }

        /// <summary>
        /// Set the name of multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to change the name of</param>
        /// <param name="names">An array which contains the new names of the objects</param>
        public void SetNames(RegionProxy simulator, uint[] localIDs, string[] names)
        {
            ObjectNamePacket namePacket = new ObjectNamePacket();
            namePacket.Header.Reliable = true;
            namePacket.AgentData.AgentID = Frame.Agent.AgentID;
            namePacket.AgentData.SessionID = Frame.Agent.SessionID;

            namePacket.ObjectData = new ObjectNamePacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; ++i)
            {
                namePacket.ObjectData[i] = new ObjectNamePacket.ObjectDataBlock();
                namePacket.ObjectData[i].LocalID = localIDs[i];
                namePacket.ObjectData[i].Name = Utils.StringToBytes(names[i]);
            }

            simulator.Inject(namePacket, Direction.Outgoing);
        }

        /// <summary>
        /// Set the description of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="description">A string containing the new description of the object</param>
        public void SetDescription(RegionProxy simulator, uint localID, string description)
        {
            SetDescriptions(simulator, new uint[] { localID }, new string[] { description });
        }

        /// <summary>
        /// Set the descriptions of multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to change the description of</param>
        /// <param name="descriptions">An array which contains the new descriptions of the objects</param>
        public void SetDescriptions(RegionProxy simulator, uint[] localIDs, string[] descriptions)
        {
            ObjectDescriptionPacket descPacket = new ObjectDescriptionPacket();
            descPacket.Header.Reliable = true;
            descPacket.AgentData.AgentID = Frame.Agent.AgentID;
            descPacket.AgentData.SessionID = Frame.Agent.SessionID;

            descPacket.ObjectData = new ObjectDescriptionPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; ++i)
            {
                descPacket.ObjectData[i] = new ObjectDescriptionPacket.ObjectDataBlock();
                descPacket.ObjectData[i].LocalID = localIDs[i];
                descPacket.ObjectData[i].Description = Utils.StringToBytes(descriptions[i]);
            }

            simulator.Inject(descPacket, Direction.Outgoing);
        }

        /// <summary>
        /// Attach an object to this avatar
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="attachPoint">The point on the avatar the object will be attached</param>
        /// <param name="rotation">The rotation of the attached object</param>
        public void AttachObject(RegionProxy simulator, uint localID, AttachmentPoint attachPoint, Quaternion rotation, bool add = false)
        {
            ObjectAttachPacket attach = new ObjectAttachPacket();
            attach.AgentData.AgentID = Frame.Agent.AgentID;
            attach.AgentData.SessionID = Frame.Agent.SessionID;
            attach.AgentData.AttachmentPoint = (byte)((byte)attachPoint | (add ? 0x80 : 0));

            attach.ObjectData = new ObjectAttachPacket.ObjectDataBlock[1];
            attach.ObjectData[0] = new ObjectAttachPacket.ObjectDataBlock();
            attach.ObjectData[0].ObjectLocalID = localID;
            attach.ObjectData[0].Rotation = rotation;

            simulator.Inject(attach, Direction.Outgoing);
        }

        /// <summary>
        /// Drop an attached object from this avatar
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/>
        /// object where the objects reside. This will always be the simulator the avatar is currently in
        /// </param>
        /// <param name="localID">The object's ID which is local to the simulator the object is in</param>
        public void DropObject(RegionProxy simulator, uint localID)
        {
            ObjectDropPacket dropit = new ObjectDropPacket();
            dropit.AgentData.AgentID = Frame.Agent.AgentID;
            dropit.AgentData.SessionID = Frame.Agent.SessionID;
            dropit.ObjectData = new ObjectDropPacket.ObjectDataBlock[1];
            dropit.ObjectData[0] = new ObjectDropPacket.ObjectDataBlock();
            dropit.ObjectData[0].ObjectLocalID = localID;

            simulator.Inject(dropit, Direction.Outgoing);
        }

        /// <summary>
        /// Detach an object from yourself
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> 
        /// object where the objects reside
        /// 
        /// This will always be the simulator the avatar is currently in
        /// </param>
        /// <param name="localIDs">An array which contains the IDs of the objects to detach</param>
        public void DetachObjects(RegionProxy simulator, List<uint> localIDs)
        {
            ObjectDetachPacket detach = new ObjectDetachPacket();
            detach.AgentData.AgentID = Frame.Agent.AgentID;
            detach.AgentData.SessionID = Frame.Agent.SessionID;
            detach.ObjectData = new ObjectDetachPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                detach.ObjectData[i] = new ObjectDetachPacket.ObjectDataBlock();
                detach.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            simulator.Inject(detach, Direction.Outgoing);
        }

        /// <summary>
        /// Change the position of an object, Will change position of entire linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="position">The new position of the object</param>
        public void SetPosition(RegionProxy simulator, uint localID, Vector3 position)
        {
            UpdateObject(simulator, localID, position, UpdateType.Position | UpdateType.Linked);
        }

        /// <summary>
        /// Change the position of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="position">The new position of the object</param>
        /// <param name="childOnly">if true, will change position of (this) child prim only, not entire linkset</param>
        public void SetPosition(RegionProxy simulator, uint localID, Vector3 position, bool childOnly)
        {
            UpdateType type = UpdateType.Position;

            if (!childOnly)
                type |= UpdateType.Linked;

            UpdateObject(simulator, localID, position, type);
        }

        /// <summary>
        /// Change the Scale (size) of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="scale">The new scale of the object</param>
        /// <param name="childOnly">If true, will change scale of this prim only, not entire linkset</param>
        /// <param name="uniform">True to resize prims uniformly</param>
        public void SetScale(RegionProxy simulator, uint localID, Vector3 scale, bool childOnly, bool uniform)
        {
            UpdateType type = UpdateType.Scale;

            if (!childOnly)
                type |= UpdateType.Linked;

            if (uniform)
                type |= UpdateType.Uniform;

            UpdateObject(simulator, localID, scale, type);
        }

        /// <summary>
        /// Change the Rotation of an object that is either a child or a whole linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="quat">The new scale of the object</param>
        /// <param name="childOnly">If true, will change rotation of this prim only, not entire linkset</param>
        public void SetRotation(RegionProxy simulator, uint localID, Quaternion quat, bool childOnly)
        {
            UpdateType type = UpdateType.Rotation;

            if (!childOnly)
                type |= UpdateType.Linked;

            MultipleObjectUpdatePacket multiObjectUpdate = new MultipleObjectUpdatePacket();
            multiObjectUpdate.AgentData.AgentID = Frame.Agent.AgentID;
            multiObjectUpdate.AgentData.SessionID = Frame.Agent.SessionID;

            multiObjectUpdate.ObjectData = new MultipleObjectUpdatePacket.ObjectDataBlock[1];

            multiObjectUpdate.ObjectData[0] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[0].Type = (byte)type;
            multiObjectUpdate.ObjectData[0].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[0].Data = quat.GetBytes();

            simulator.Inject(multiObjectUpdate, Direction.Outgoing);
        }

        /// <summary>
        /// Send a Multiple Object Update packet to change the size, scale or rotation of a primitive
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="data">The new rotation, size, or position of the target object</param>
        /// <param name="type">The flags from the <seealso cref="UpdateType"/> Enum</param>
        public void UpdateObject(RegionProxy simulator, uint localID, Vector3 data, UpdateType type)
        {
            MultipleObjectUpdatePacket multiObjectUpdate = new MultipleObjectUpdatePacket();
            multiObjectUpdate.Header.Reliable = true;
            multiObjectUpdate.AgentData.AgentID = Frame.Agent.AgentID;
            multiObjectUpdate.AgentData.SessionID = Frame.Agent.SessionID;

            multiObjectUpdate.ObjectData = new MultipleObjectUpdatePacket.ObjectDataBlock[1];

            multiObjectUpdate.ObjectData[0] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[0].Type = (byte)type;
            multiObjectUpdate.ObjectData[0].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[0].Data = data.GetBytes();

            simulator.Inject(multiObjectUpdate, Direction.Outgoing);
        }

        public void UpdateObject(RegionProxy simulator, uint localID, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            MultipleObjectUpdatePacket multiObjectUpdate = new MultipleObjectUpdatePacket();
            multiObjectUpdate.Header.Reliable = true;
            multiObjectUpdate.AgentData.AgentID = Frame.Agent.AgentID;
            multiObjectUpdate.AgentData.SessionID = Frame.Agent.SessionID;

            multiObjectUpdate.ObjectData = new MultipleObjectUpdatePacket.ObjectDataBlock[3];

            multiObjectUpdate.ObjectData[0] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[0].Type = (byte)UpdateType.Position;
            multiObjectUpdate.ObjectData[0].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[0].Data = pos.GetBytes();

            multiObjectUpdate.ObjectData[1] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[1].Type = (byte)UpdateType.Rotation;
            multiObjectUpdate.ObjectData[1].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[1].Data = rot.GetBytes();

            multiObjectUpdate.ObjectData[2] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[2].Type = (byte)UpdateType.Scale;
            multiObjectUpdate.ObjectData[2].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[2].Data = scale.GetBytes();

            simulator.Inject(multiObjectUpdate, Direction.Outgoing);
        }

        /// <summary>
        /// Deed an object (prim) to a group, Object must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to deed the object to</param>
        public void DeedObject(RegionProxy simulator, uint localID, UUID groupOwner)
        {
            ObjectOwnerPacket objDeedPacket = new ObjectOwnerPacket();
            objDeedPacket.AgentData.AgentID = Frame.Agent.AgentID;
            objDeedPacket.AgentData.SessionID = Frame.Agent.SessionID;

            // Can only be use in God mode
            objDeedPacket.HeaderData.Override = false;
            objDeedPacket.HeaderData.OwnerID = UUID.Zero;
            objDeedPacket.HeaderData.GroupID = groupOwner;

            objDeedPacket.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[1];
            objDeedPacket.ObjectData[0] = new ObjectOwnerPacket.ObjectDataBlock();

            objDeedPacket.ObjectData[0].ObjectLocalID = localID;

            simulator.Inject(objDeedPacket, Direction.Outgoing);
        }

        /// <summary>
        /// Deed multiple objects (prims) to a group, Objects must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to deed</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to deed the object to</param>
        public void DeedObjects(RegionProxy simulator, List<uint> localIDs, UUID groupOwner)
        {
            ObjectOwnerPacket packet = new ObjectOwnerPacket();
            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            // Can only be use in God mode
            packet.HeaderData.Override = false;
            packet.HeaderData.OwnerID = UUID.Zero;
            packet.HeaderData.GroupID = groupOwner;

            packet.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectOwnerPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIDs[i];
            }
            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Set the permissions on multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to set the permissions on</param>
        /// <param name="who">The new Who mask to set</param>
        /// <param name="permissions">Which permission to modify</param>
        /// <param name="set">The new state of permission</param>
        public void SetPermissions(RegionProxy simulator, List<uint> localIDs, PermissionWho who,
            PermissionMask permissions, bool set)
        {
            ObjectPermissionsPacket packet = new ObjectPermissionsPacket();
            packet.Header.Reliable = true;

            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            // Override can only be used by gods
            packet.HeaderData.Override = false;

            packet.ObjectData = new ObjectPermissionsPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectPermissionsPacket.ObjectDataBlock();

                packet.ObjectData[i].ObjectLocalID = localIDs[i];
                packet.ObjectData[i].Field = (byte)who;
                packet.ObjectData[i].Mask = (uint)permissions;
                packet.ObjectData[i].Set = Convert.ToByte(set);
            }

            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Request additional properties for an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="objectID"></param>
        public void RequestObjectPropertiesFamily(RegionProxy simulator, UUID objectID)
        {
            RequestObjectPropertiesFamily(simulator, objectID, true);
        }

        /// <summary>
        /// Request additional properties for an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the object resides</param>
        /// <param name="objectID">Absolute UUID of the object</param>
        /// <param name="reliable">Whether to require server acknowledgement of this request</param>
        public void RequestObjectPropertiesFamily(RegionProxy simulator, UUID objectID, bool reliable)
        {
            RequestObjectPropertiesFamilyPacket properties = new RequestObjectPropertiesFamilyPacket();
            properties.AgentData.AgentID = Frame.Agent.AgentID;
            properties.AgentData.SessionID = Frame.Agent.SessionID;
            properties.ObjectData.ObjectID = objectID;
            // TODO: RequestFlags is typically only for bug report submissions, but we might be able to
            // use it to pass an arbitrary uint back to the callback
            properties.ObjectData.RequestFlags = 0;

            properties.Header.Reliable = reliable;

            simulator.Inject(properties, Direction.Outgoing);
        }

        /// <summary>
        /// Set the ownership of a list of objects to the specified group
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.RegionProxy"/> object where the objects reside</param>
        /// <param name="localIds">An array which contains the IDs of the objects to set the group id on</param>
        /// <param name="groupID">The Groups ID</param>
        public void SetObjectsGroup(RegionProxy simulator, List<uint> localIds, UUID groupID)
        {
            ObjectGroupPacket packet = new ObjectGroupPacket();
            packet.AgentData.AgentID = Frame.Agent.AgentID;
            packet.AgentData.GroupID = groupID;
            packet.AgentData.SessionID = Frame.Agent.SessionID;

            packet.ObjectData = new ObjectGroupPacket.ObjectDataBlock[localIds.Count];
            for (int i = 0; i < localIds.Count; i++)
            {
                packet.ObjectData[i] = new ObjectGroupPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIds[i];
            }

            simulator.Inject(packet, Direction.Outgoing);
        }

        /// <summary>
        /// Update current URL of the previously set prim media
        /// </summary>
        /// <param name="primID">UUID of the prim</param>
        /// <param name="newURL">Set current URL to this</param>
        /// <param name="face">Prim face number</param>
        /// <param name="sim">RegionProxy in which prim is located</param>
        //public void NavigateObjectMedia(UUID primID, int face, string newURL, RegionProxy sim)
        //{
        //    Uri url;
        //    if (sim.Caps != null && null != (url = sim.Caps.CapabilityURI("ObjectMediaNavigate")))
        //    {
        //        ObjectMediaNavigateMessage req = new ObjectMediaNavigateMessage();
        //        req.PrimID = primID;
        //        req.URL = newURL;
        //        req.Face = face;

        //        CapsClient request = new CapsClient(url);
        //        request.OnComplete += (CapsClient client, OSD result, Exception error) =>
        //        {
        //            if (error != null)
        //            {
        //                Logger.Log("ObjectMediaNavigate: " + error.Message, Helpers.LogLevel.Error, Client);
        //            }
        //        };

        //        request.BeginGetResponse(req.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
        //    }
        //    else
        //    {
        //        Logger.Log("ObjectMediaNavigate capability not available", Helpers.LogLevel.Error, Client);
        //    }
        //}

        /// <summary>
        /// Set object media
        /// </summary>
        /// <param name="primID">UUID of the prim</param>
        /// <param name="faceMedia">Array the length of prims number of faces. Null on face indexes where there is
        /// no media, <seealso cref="MediaEntry"/> on faces which contain the media</param>
        /// <param name="sim">Simulatior in which prim is located</param>
        //public void UpdateObjectMedia(UUID primID, MediaEntry[] faceMedia, RegionProxy sim)
        //{
        //    Uri url;
        //    if (sim.Caps != null && null != (url = sim.Caps.CapabilityURI("ObjectMedia")))
        //    {
        //        ObjectMediaUpdate req = new ObjectMediaUpdate();
        //        req.PrimID = primID;
        //        req.FaceMedia = faceMedia;
        //        req.Verb = "UPDATE";

        //        CapsClient request = new CapsClient(url);
        //        request.OnComplete += (CapsClient client, OSD result, Exception error) =>
        //        {
        //            if (error != null)
        //            {
        //                Logger.Log("ObjectMediaUpdate: " + error.Message, Helpers.LogLevel.Error, Client);
        //            }
        //        };
        //        request.BeginGetResponse(req.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
        //    }
        //    else
        //    {
        //        Logger.Log("ObjectMedia capability not available", Helpers.LogLevel.Error, Client);
        //    }
        //}

        /// <summary>
        /// Retrieve information about object media
        /// </summary>
        /// <param name="primID">UUID of the primitive</param>
        /// <param name="sim">RegionProxy where prim is located</param>
        /// <param name="callback">Call this callback when done</param>
        //public void RequestObjectMedia(UUID primID, RegionProxy sim, ObjectMediaCallback callback)
        //{
        //    Uri url;
        //    if (sim.Caps != null && null != (url = sim.Caps.CapabilityURI("ObjectMedia")))
        //    {
        //        ObjectMediaRequest req = new ObjectMediaRequest();
        //        req.PrimID = primID;
        //        req.Verb = "GET";

        //        CapsClient request = new CapsClient(url);
        //        request.OnComplete += (CapsClient client, OSD result, Exception error) =>
        //        {
        //            if (result == null)
        //            {
        //                Logger.Log("Failed retrieving ObjectMedia data", Helpers.LogLevel.Error, Client);
        //                try { callback(false, string.Empty, null); }
        //                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }
        //                return;
        //            }

        //            ObjectMediaMessage msg = new ObjectMediaMessage();
        //            msg.Deserialize((OSDMap)result);

        //            if (msg.Request is ObjectMediaResponse)
        //            {
        //                ObjectMediaResponse response = (ObjectMediaResponse)msg.Request;

        //                if (Client.Settings.OBJECT_TRACKING)
        //                {
        //                    Primitive prim = sim.ObjectsPrimitives.Find((Primitive p) => { return p.ID == primID; });
        //                    if (prim != null)
        //                    {
        //                        prim.MediaVersion = response.Version;
        //                        prim.FaceMedia = response.FaceMedia;
        //                    }
        //                }

        //                try { callback(true, response.Version, response.FaceMedia); }
        //                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }
        //            }
        //            else
        //            {
        //                try { callback(false, string.Empty, null); }
        //                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }
        //            }
        //        };

        //        request.BeginGetResponse(req.Serialize(), OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);
        //    }
        //    else
        //    {
        //        Logger.Log("ObjectMedia capability not available", Helpers.LogLevel.Error, Client);
        //        try { callback(false, string.Empty, null); }
        //        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }
        //    }
        //}
        #endregion

        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ObjectUpdateHandler(Packet packet, RegionProxy region)
        {
            ObjectUpdatePacket update = (ObjectUpdatePacket)packet;
            //UpdateDilation(e.RegionProxy, update.RegionData.TimeDilation);

            for (int b = 0; b < update.ObjectData.Length; b++)
            {
                ObjectUpdatePacket.ObjectDataBlock block = update.ObjectData[b];

                ObjectMovementUpdate objectupdate = new ObjectMovementUpdate();
                //Vector4 collisionPlane = Vector4.Zero;
                //Vector3 position;
                //Vector3 velocity;
                //Vector3 acceleration;
                //Quaternion rotation;
                //Vector3 angularVelocity;
                NameValue[] nameValues;
                bool attachment = false;
                PCode pcode = (PCode)block.PCode;

                #region Relevance check

                // Check if we are interested in this object
                //if (!Client.Settings.ALWAYS_DECODE_OBJECTS)
                if(false)
                {
                    switch (pcode)
                    {
                        case PCode.Grass:
                        case PCode.Tree:
                        case PCode.NewTree:
                        case PCode.Prim:
                            if (m_ObjectUpdate == null) continue;
                            break;
                        case PCode.Avatar:
                            // Make an exception for updates about our own agent
                            if (block.FullID != Frame.Agent.AgentID && m_AvatarUpdate == null) continue;
                            break;
                        case PCode.ParticleSystem:
                            continue; // TODO: Do something with these
                    }
                }

                #endregion Relevance check

                #region NameValue parsing

                string nameValue = Utils.BytesToString(block.NameValue);
                if (nameValue.Length > 0)
                {
                    string[] lines = nameValue.Split('\n');
                    nameValues = new NameValue[lines.Length];

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(lines[i]))
                        {
                            NameValue nv = new NameValue(lines[i]);
                            if (nv.Name == "AttachItemID") attachment = true;
                            nameValues[i] = nv;
                        }
                    }
                }
                else
                {
                    nameValues = new NameValue[0];
                }

                #endregion NameValue parsing

                #region Decode Object (primitive) parameters
                Primitive.ConstructionData data = new Primitive.ConstructionData();
                data.State = block.State;
                data.Material = (Material)block.Material;
                data.PathCurve = (PathCurve)block.PathCurve;
                data.profileCurve = block.ProfileCurve;
                data.PathBegin = Primitive.UnpackBeginCut(block.PathBegin);
                data.PathEnd = Primitive.UnpackEndCut(block.PathEnd);
                data.PathScaleX = Primitive.UnpackPathScale(block.PathScaleX);
                data.PathScaleY = Primitive.UnpackPathScale(block.PathScaleY);
                data.PathShearX = Primitive.UnpackPathShear((sbyte)block.PathShearX);
                data.PathShearY = Primitive.UnpackPathShear((sbyte)block.PathShearY);
                data.PathTwist = Primitive.UnpackPathTwist(block.PathTwist);
                data.PathTwistBegin = Primitive.UnpackPathTwist(block.PathTwistBegin);
                data.PathRadiusOffset = Primitive.UnpackPathTwist(block.PathRadiusOffset);
                data.PathTaperX = Primitive.UnpackPathTaper(block.PathTaperX);
                data.PathTaperY = Primitive.UnpackPathTaper(block.PathTaperY);
                data.PathRevolutions = Primitive.UnpackPathRevolutions(block.PathRevolutions);
                data.PathSkew = Primitive.UnpackPathTwist(block.PathSkew);
                data.ProfileBegin = Primitive.UnpackBeginCut(block.ProfileBegin);
                data.ProfileEnd = Primitive.UnpackEndCut(block.ProfileEnd);
                data.ProfileHollow = Primitive.UnpackProfileHollow(block.ProfileHollow);
                data.PCode = pcode;
                #endregion

                #region Decode Additional packed parameters in ObjectData
                int pos = 0;
                switch (block.ObjectData.Length)
                {
                    case 76:
                        // Collision normal for avatar
                        objectupdate.CollisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 60;
                    case 60:
                        // Position
                        objectupdate.Position = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Velocity
                        objectupdate.Velocity = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Acceleration
                        objectupdate.Acceleration = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Rotation (theta)
                        objectupdate.Rotation = new Quaternion(block.ObjectData, pos, true);
                        pos += 12;
                        // Angular velocity (omega)
                        objectupdate.AngularVelocity = new Vector3(block.ObjectData, pos);
                        pos += 12;

                        break;
                    case 48:
                        // Collision normal for avatar
                        objectupdate.CollisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 32;
                    case 32:
                        // The data is an array of unsigned shorts

                        // Position
                        objectupdate.Position = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -0.5f * 256.0f, 1.5f * 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -0.5f * 256.0f, 1.5f * 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 3.0f * 256.0f));
                        pos += 6;
                        // Velocity
                        objectupdate.Velocity = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Acceleration
                        objectupdate.Acceleration = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Rotation (theta)
                        objectupdate.Rotation = new Quaternion(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 6, -1.0f, 1.0f));
                        pos += 8;
                        // Angular velocity (omega)
                        objectupdate.AngularVelocity = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;

                        break;
                    case 16:
                        // The data is an array of single bytes (8-bit numbers)

                        // Position
                        objectupdate.Position = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Velocity
                        objectupdate.Velocity = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Accleration
                        objectupdate.Acceleration = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Rotation
                        objectupdate.Rotation = new Quaternion(
                            Utils.ByteToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 3, -1.0f, 1.0f));
                        pos += 4;
                        // Angular Velocity
                        objectupdate.AngularVelocity = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;

                        break;
                    default:
                        Logger.Log("Got an ObjectUpdate block with ObjectUpdate field length of " +
                            block.ObjectData.Length, Helpers.LogLevel.Warning);

                        continue;
                }
                #endregion

                // Determine the object type and create the appropriate class
                switch (pcode)
                {
                    #region Prim and Foliage
                    case PCode.Grass:
                    case PCode.Tree:
                    case PCode.NewTree:
                    case PCode.Prim:

                        bool isNewObject;
                        lock (region.ObjectsPrimitives)
                            isNewObject = !region.ObjectsPrimitives.ContainsKey(block.ID);

                        Primitive prim = GetPrimitive(region, block.ID, block.FullID);

                        // Textures
                        objectupdate.Textures = new Primitive.TextureEntry(block.TextureEntry, 0,
                            block.TextureEntry.Length);

                        OnObjectDataBlockUpdate(new ObjectDataBlockUpdateEventArgs(null, prim, data, block, objectupdate, nameValues));

                        #region Update Prim Info with decoded data
                        prim.Flags = (PrimFlags)block.UpdateFlags;

                        if ((prim.Flags & PrimFlags.ZlibCompressed) != 0)
                        {
                            Logger.Log("Got a ZlibCompressed ObjectUpdate, implement me!",
                                Helpers.LogLevel.Warning);
                            continue;
                        }

                        // Automatically request ObjectProperties for prim if it was rezzed selected.
                        //if ((prim.Flags & PrimFlags.CreateSelected) != 0)
                        //{
                        //    SelectObject(simulator, prim.LocalID);
                        //}

                        prim.NameValues = nameValues;
                        prim.LocalID = block.ID;
                        prim.ID = block.FullID;
                        prim.ParentID = block.ParentID;
                        prim.RegionHandle = update.RegionData.RegionHandle;
                        prim.Scale = block.Scale;
                        prim.ClickAction = (ClickAction)block.ClickAction;
                        prim.OwnerID = block.OwnerID;
                        prim.MediaURL = Utils.BytesToString(block.MediaURL);
                        prim.Text = Utils.BytesToString(block.Text);
                        prim.TextColor = new Color4(block.TextColor, 0, false, true);
                        prim.IsAttachment = attachment;

                        // Sound information
                        prim.Sound = block.Sound;
                        prim.SoundFlags = (SoundFlags)block.Flags;
                        prim.SoundGain = block.Gain;
                        prim.SoundRadius = block.Radius;

                        // Joint information
                        prim.Joint = (JointType)block.JointType;
                        prim.JointPivot = block.JointPivot;
                        prim.JointAxisOrAnchor = block.JointAxisOrAnchor;

                        // Object parameters
                        prim.PrimData = data;

                        // Textures, texture animations, particle system, and extra params
                        prim.Textures = objectupdate.Textures;

                        prim.TextureAnim = new Primitive.TextureAnimation(block.TextureAnim, 0);
                        prim.ParticleSys = new Primitive.ParticleSystem(block.PSBlock, 0);
                        prim.SetExtraParamsFromBytes(block.ExtraParams, 0);

                        // PCode-specific data
                        switch (pcode)
                        {
                            case PCode.Grass:
                            case PCode.Tree:
                            case PCode.NewTree:
                                if (block.Data.Length == 1)
                                    prim.TreeSpecies = (Tree)block.Data[0];
                                else
                                    Logger.Log("Got a foliage update with an invalid TreeSpecies field", Helpers.LogLevel.Warning);
                                //    prim.ScratchPad = Utils.EmptyBytes;
                                //    break;
                                //default:
                                //    prim.ScratchPad = new byte[block.Data.Length];
                                //    if (block.Data.Length > 0)
                                //        Buffer.BlockCopy(block.Data, 0, prim.ScratchPad, 0, prim.ScratchPad.Length);
                                break;
                        }
                        prim.ScratchPad = Utils.EmptyBytes;

                        // Packed parameters
                        prim.CollisionPlane = objectupdate.CollisionPlane;
                        prim.Position = objectupdate.Position;
                        prim.Velocity = objectupdate.Velocity;
                        prim.Acceleration = objectupdate.Acceleration;
                        prim.Rotation = objectupdate.Rotation;
                        prim.AngularVelocity = objectupdate.AngularVelocity;
                        #endregion

                        EventHandler<PrimEventArgs> handler = m_ObjectUpdate;
                        if (handler != null)
                        {
                            WorkPool.QueueUserWorkItem(delegate (object o)
                            { handler(this, new PrimEventArgs(region, prim, update.RegionData.TimeDilation, isNewObject, attachment)); });
                        }
                        //OnParticleUpdate handler replacing decode particles, PCode.Particle system appears to be deprecated this is a fix
                        if (prim.ParticleSys.PartMaxAge != 0)
                        {
                            OnParticleUpdate(new ParticleUpdateEventArgs(null, prim.ParticleSys, prim));
                        }

                        break;
                    #endregion Prim and Foliage
                    #region Avatar
                    case PCode.Avatar:

                        bool isNewAvatar;
                        lock (region.ObjectsAvatars)
                            isNewAvatar = !region.ObjectsAvatars.ContainsKey(block.ID);

                        // Update some internals if this is our avatar
                        if (block.FullID == Frame.Agent.AgentID && region == Frame.Network.CurrentSim)
                        {
                            #region Update Frame.Agent

                            // We need the local ID to recognize terse updates for our agent
                            Frame.Agent.localID = block.ID;

                            // Packed parameters
                            Frame.Agent.collisionPlane = objectupdate.CollisionPlane;
                            Frame.Agent.relativePosition = objectupdate.Position;
                            Frame.Agent.velocity = objectupdate.Velocity;
                            Frame.Agent.acceleration = objectupdate.Acceleration;
                            Frame.Agent.relativeRotation = objectupdate.Rotation;
                            Frame.Agent.angularVelocity = objectupdate.AngularVelocity;

                            #endregion
                        }

                        #region Create an Avatar from the decoded data

                        Avatar avatar = GetAvatar(region, block.ID, block.FullID);

                        objectupdate.Avatar = true;
                        // Textures
                        objectupdate.Textures = new Primitive.TextureEntry(block.TextureEntry, 0,
                            block.TextureEntry.Length);

                        OnObjectDataBlockUpdate(new ObjectDataBlockUpdateEventArgs(null, avatar, data, block, objectupdate, nameValues));

                        uint oldSeatID = avatar.ParentID;

                        avatar.ID = block.FullID;
                        avatar.LocalID = block.ID;
                        avatar.Scale = block.Scale;
                        avatar.CollisionPlane = objectupdate.CollisionPlane;
                        avatar.Position = objectupdate.Position;
                        avatar.Velocity = objectupdate.Velocity;
                        avatar.Acceleration = objectupdate.Acceleration;
                        avatar.Rotation = objectupdate.Rotation;
                        avatar.AngularVelocity = objectupdate.AngularVelocity;
                        avatar.NameValues = nameValues;
                        avatar.PrimData = data;
                        if (block.Data.Length > 0)
                        {
                            Logger.Log("Unexpected Data field for an avatar update, length " + block.Data.Length, Helpers.LogLevel.Warning);
                        }
                        avatar.ParentID = block.ParentID;
                        avatar.RegionHandle = update.RegionData.RegionHandle;

                        //SetAvatarSittingOn(null, avatar, block.ParentID, oldSeatID);

                        // Textures
                        avatar.Textures = objectupdate.Textures;

                        #endregion Create an Avatar from the decoded data

                        OnAvatarUpdate(new AvatarUpdateEventArgs(null, avatar, update.RegionData.TimeDilation, isNewAvatar));

                        break;
                    #endregion Avatar
                    case PCode.ParticleSystem:
                        DecodeParticleUpdate(block);
                        break;
                    default:
                        Logger.DebugLog("Got an ObjectUpdate block with an unrecognized PCode " + pcode.ToString());
                        break;
                }
            }
            return packet;
        }

        protected void DecodeParticleUpdate(ObjectUpdatePacket.ObjectDataBlock block)
        {
            // TODO: Handle ParticleSystem ObjectUpdate blocks
            // float bounce_b
            // Vector4 scale_range
            // Vector4 alpha_range
            // Vector3 vel_offset
            // float dist_begin_fadeout
            // float dist_end_fadeout
            // UUID image_uuid
            // long flags
            // byte createme
            // Vector3 diff_eq_alpha
            // Vector3 diff_eq_scale
            // byte max_particles
            // byte initial_particles
            // float kill_plane_z
            // Vector3 kill_plane_normal
            // float bounce_plane_z
            // Vector3 bounce_plane_normal
            // float spawn_range
            // float spawn_frequency
            // float spawn_frequency_range
            // Vector3 spawn_direction
            // float spawn_direction_range
            // float spawn_velocity
            // float spawn_velocity_range
            // float speed_limit
            // float wind_weight
            // Vector3 current_gravity
            // float gravity_weight
            // float global_lifetime
            // float individual_lifetime
            // float individual_lifetime_range
            // float alpha_decay
            // float scale_decay
            // float distance_death
            // float damp_motion_factor
            // Vector3 wind_diffusion_factor
        }

        /// <summary>
        /// A terse object update, used when a transformation matrix or
        /// velocity/acceleration for an object changes but nothing else
        /// (scale/position/rotation/acceleration/velocity)
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ImprovedTerseObjectUpdateHandler(Packet packet, RegionProxy region)
        {
            ImprovedTerseObjectUpdatePacket terse = (ImprovedTerseObjectUpdatePacket)packet;
            //UpdateDilation(simulator, terse.RegionData.TimeDilation);

            for (int i = 0; i < terse.ObjectData.Length; i++)
            {
                ImprovedTerseObjectUpdatePacket.ObjectDataBlock block = terse.ObjectData[i];

                try
                {
                    int pos = 4;
                    uint localid = Utils.BytesToUInt(block.Data, 0);

                    // Check if we are interested in this update
                    //if (!Client.Settings.ALWAYS_DECODE_OBJECTS
                    //    && localid != Frame.Agent.localID
                    //    && m_TerseObjectUpdate == null)
                    //{
                    //    continue;
                    //}

                    #region Decode update data

                    ObjectMovementUpdate update = new ObjectMovementUpdate();

                    // LocalID
                    update.LocalID = localid;
                    // State
                    update.State = block.Data[pos++];
                    // Avatar boolean
                    update.Avatar = (block.Data[pos++] != 0);
                    // Collision normal for avatar
                    if (update.Avatar)
                    {
                        update.CollisionPlane = new Vector4(block.Data, pos);
                        pos += 16;
                    }
                    // Position
                    update.Position = new Vector3(block.Data, pos);
                    pos += 12;
                    // Velocity
                    update.Velocity = new Vector3(
                        Utils.UInt16ToFloat(block.Data, pos, -128.0f, 128.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -128.0f, 128.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -128.0f, 128.0f));
                    pos += 6;
                    // Acceleration
                    update.Acceleration = new Vector3(
                        Utils.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;
                    // Rotation (theta)
                    update.Rotation = new Quaternion(
                        Utils.UInt16ToFloat(block.Data, pos, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 6, -1.0f, 1.0f));
                    pos += 8;
                    // Angular velocity (omega)
                    update.AngularVelocity = new Vector3(
                        Utils.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;

                    // Textures
                    // FIXME: Why are we ignoring the first four bytes here?
                    if (block.TextureEntry.Length != 0)
                        update.Textures = new Primitive.TextureEntry(block.TextureEntry, 4, block.TextureEntry.Length - 4);

                    #endregion Decode update data

                    Primitive obj = /*!Client.Settings.OBJECT_TRACKING ? null :*/ (update.Avatar) ?
                        (Primitive)GetAvatar(region, update.LocalID, UUID.Zero) :
                        (Primitive)GetPrimitive(region, update.LocalID, UUID.Zero);

                    // Fire the pre-emptive notice (before we stomp the object)
                    EventHandler<TerseObjectUpdateEventArgs> handler = m_TerseObjectUpdate;
                    if (handler != null)
                    {
                        WorkPool.QueueUserWorkItem(delegate (object o)
                        { handler(this, new TerseObjectUpdateEventArgs(region, obj, update, terse.RegionData.TimeDilation)); });
                    }

                    #region Update Self
                    if (update.LocalID == Frame.Agent.localID)
                    {
                        Frame.Agent.collisionPlane = update.CollisionPlane;
                        Frame.Agent.relativePosition = update.Position;
                        Frame.Agent.velocity = update.Velocity;
                        Frame.Agent.acceleration = update.Acceleration;
                        Frame.Agent.relativeRotation = update.Rotation;
                        Frame.Agent.angularVelocity = update.AngularVelocity;
                    }
                    #endregion Update Self

                    if (/*Client.Settings.OBJECT_TRACKING && */obj != null)
                    {
                        obj.Position = update.Position;
                        obj.Rotation = update.Rotation;
                        obj.Velocity = update.Velocity;
                        obj.CollisionPlane = update.CollisionPlane;
                        obj.Acceleration = update.Acceleration;
                        obj.AngularVelocity = update.AngularVelocity;
                        obj.PrimData.State = update.State;
                        if (update.Textures != null)
                            obj.Textures = update.Textures;
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex);
                }
            }
            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ObjectUpdateCompressedHandler(Packet packet, RegionProxy region)
        {
            ObjectUpdateCompressedPacket update = (ObjectUpdateCompressedPacket)packet;

            for (int b = 0; b < update.ObjectData.Length; b++)
            {
                ObjectUpdateCompressedPacket.ObjectDataBlock block = update.ObjectData[b];
                int i = 0;

                try
                {
                    // UUID
                    UUID FullID = new UUID(block.Data, 0);
                    i += 16;
                    // Local ID
                    uint LocalID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));
                    // PCode
                    PCode pcode = (PCode)block.Data[i++];

                    #region Relevance check

                    //if (!Client.Settings.ALWAYS_DECODE_OBJECTS)
                    if(false)
                    {
                        switch (pcode)
                        {
                            case PCode.Grass:
                            case PCode.Tree:
                            case PCode.NewTree:
                            case PCode.Prim:
                                if (m_ObjectUpdate == null) continue;
                                break;
                        }
                    }

                    #endregion Relevance check

                    bool isNew;
                    lock (region.ObjectsPrimitives)
                        isNew = !region.ObjectsPrimitives.ContainsKey(LocalID);

                    Primitive prim = GetPrimitive(region, LocalID, FullID);

                    prim.LocalID = LocalID;
                    prim.ID = FullID;
                    prim.Flags = (PrimFlags)block.UpdateFlags;
                    prim.PrimData.PCode = pcode;

                    #region Decode block and update Prim

                    // State
                    prim.PrimData.State = block.Data[i++];
                    // CRC
                    i += 4;
                    // Material
                    prim.PrimData.Material = (Material)block.Data[i++];
                    // Click action
                    prim.ClickAction = (ClickAction)block.Data[i++];
                    // Scale
                    prim.Scale = new Vector3(block.Data, i);
                    i += 12;
                    // Position
                    prim.Position = new Vector3(block.Data, i);
                    i += 12;
                    // Rotation
                    prim.Rotation = new Quaternion(block.Data, i, true);
                    i += 12;
                    // Compressed flags
                    CompressedFlags flags = (CompressedFlags)Utils.BytesToUInt(block.Data, i);
                    i += 4;

                    prim.OwnerID = new UUID(block.Data, i);
                    i += 16;

                    // Angular velocity
                    if ((flags & CompressedFlags.HasAngularVelocity) != 0)
                    {
                        prim.AngularVelocity = new Vector3(block.Data, i);
                        i += 12;
                    }

                    // Parent ID
                    if ((flags & CompressedFlags.HasParent) != 0)
                    {
                        prim.ParentID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));
                    }
                    else
                    {
                        prim.ParentID = 0;
                    }

                    // Tree data
                    if ((flags & CompressedFlags.Tree) != 0)
                    {
                        prim.TreeSpecies = (Tree)block.Data[i++];
                        //prim.ScratchPad = Utils.EmptyBytes;
                    }
                    // Scratch pad
                    else if ((flags & CompressedFlags.ScratchPad) != 0)
                    {
                        prim.TreeSpecies = (Tree)0;

                        int size = block.Data[i++];
                        //prim.ScratchPad = new byte[size];
                        //Buffer.BlockCopy(block.Data, i, prim.ScratchPad, 0, size);
                        i += size;
                    }
                    prim.ScratchPad = Utils.EmptyBytes;

                    // Floating text
                    if ((flags & CompressedFlags.HasText) != 0)
                    {
                        int idx = i;
                        while (block.Data[i] != 0)
                        {
                            i++;
                        }

                        // Floating text
                        prim.Text = Utils.BytesToString(block.Data, idx, i - idx);
                        i++;

                        // Text color
                        prim.TextColor = new Color4(block.Data, i, false, true);
                        i += 4;
                    }
                    else
                    {
                        prim.Text = String.Empty;
                    }

                    // Media URL
                    if ((flags & CompressedFlags.MediaURL) != 0)
                    {
                        int idx = i;
                        while (block.Data[i] != 0)
                        {
                            i++;
                        }

                        prim.MediaURL = Utils.BytesToString(block.Data, idx, i - idx);
                        i++;
                    }

                    // Particle system
                    if ((flags & CompressedFlags.HasParticles) != 0)
                    {
                        prim.ParticleSys = new Primitive.ParticleSystem(block.Data, i);
                        i += 86;
                    }

                    // Extra parameters
                    i += prim.SetExtraParamsFromBytes(block.Data, i);

                    //Sound data
                    if ((flags & CompressedFlags.HasSound) != 0)
                    {
                        prim.Sound = new UUID(block.Data, i);
                        i += 16;

                        prim.SoundGain = Utils.BytesToFloat(block.Data, i);
                        i += 4;
                        prim.SoundFlags = (SoundFlags)block.Data[i++];
                        prim.SoundRadius = Utils.BytesToFloat(block.Data, i);
                        i += 4;
                    }

                    // Name values
                    if ((flags & CompressedFlags.HasNameValues) != 0)
                    {
                        string text = String.Empty;
                        while (block.Data[i] != 0)
                        {
                            text += (char)block.Data[i];
                            i++;
                        }
                        i++;

                        // Parse the name values
                        if (text.Length > 0)
                        {
                            string[] lines = text.Split('\n');
                            prim.NameValues = new NameValue[lines.Length];

                            for (int j = 0; j < lines.Length; j++)
                            {
                                if (!String.IsNullOrEmpty(lines[j]))
                                {
                                    NameValue nv = new NameValue(lines[j]);
                                    prim.NameValues[j] = nv;
                                }
                            }
                        }
                    }

                    prim.PrimData.PathCurve = (PathCurve)block.Data[i++];
                    ushort pathBegin = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.PathBegin = Primitive.UnpackBeginCut(pathBegin);
                    ushort pathEnd = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.PathEnd = Primitive.UnpackEndCut(pathEnd);
                    prim.PrimData.PathScaleX = Primitive.UnpackPathScale(block.Data[i++]);
                    prim.PrimData.PathScaleY = Primitive.UnpackPathScale(block.Data[i++]);
                    prim.PrimData.PathShearX = Primitive.UnpackPathShear((sbyte)block.Data[i++]);
                    prim.PrimData.PathShearY = Primitive.UnpackPathShear((sbyte)block.Data[i++]);
                    prim.PrimData.PathTwist = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathTwistBegin = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathRadiusOffset = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathTaperX = Primitive.UnpackPathTaper((sbyte)block.Data[i++]);
                    prim.PrimData.PathTaperY = Primitive.UnpackPathTaper((sbyte)block.Data[i++]);
                    prim.PrimData.PathRevolutions = Primitive.UnpackPathRevolutions(block.Data[i++]);
                    prim.PrimData.PathSkew = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);

                    prim.PrimData.profileCurve = block.Data[i++];
                    ushort profileBegin = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileBegin = Primitive.UnpackBeginCut(profileBegin);
                    ushort profileEnd = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileEnd = Primitive.UnpackEndCut(profileEnd);
                    ushort profileHollow = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileHollow = Primitive.UnpackProfileHollow(profileHollow);

                    // TextureEntry
                    int textureEntryLength = (int)Utils.BytesToUInt(block.Data, i);
                    i += 4;
                    prim.Textures = new Primitive.TextureEntry(block.Data, i, textureEntryLength);
                    i += textureEntryLength;

                    // Texture animation
                    if ((flags & CompressedFlags.TextureAnimation) != 0)
                    {
                        //int textureAnimLength = (int)Utils.BytesToUIntBig(block.Data, i);
                        i += 4;
                        prim.TextureAnim = new Primitive.TextureAnimation(block.Data, i);
                    }

                    #endregion

                    prim.IsAttachment = (flags & CompressedFlags.HasNameValues) != 0 && prim.ParentID != 0;

                    #region Raise Events

                    EventHandler<PrimEventArgs> handler = m_ObjectUpdate;
                    if (handler != null)
                        handler(this, new PrimEventArgs(region, prim, update.RegionData.TimeDilation, isNew, prim.IsAttachment));

                    #endregion
                }
                catch (IndexOutOfRangeException ex)
                {
                    Logger.Log("Error decoding an ObjectUpdateCompressed packet", Helpers.LogLevel.Warning, ex);
                    Logger.Log(block, Helpers.LogLevel.Warning);
                }
            }
            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ObjectUpdateCachedHandler(Packet packet, RegionProxy region)
        {
            //if (Client.Settings.ALWAYS_REQUEST_OBJECTS)
            //{
                //bool cachedPrimitives = Client.Settings.CACHE_PRIMITIVES;
                ObjectUpdateCachedPacket update = (ObjectUpdateCachedPacket)packet;
                List<uint> ids = new List<uint>(update.ObjectData.Length);

                // Object caching is implemented when Client.Settings.PRIMITIVES_FACTORY is True, otherwise request updates for all of these objects
                for (int i = 0; i < update.ObjectData.Length; i++)
                {
                    uint localID = update.ObjectData[i].ID;

                    //if (cachedPrimitives)
                    //{
                    //    if (!simulator.DataPool.NeedsRequest(localID))
                    //    {
                    //        continue;
                    //    }
                    //}
                    ids.Add(localID);
                }
                RequestObjects(region, ids);
            //}

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet KillObjectHandler(Packet packet, RegionProxy region)
        {
            KillObjectPacket kill = (KillObjectPacket)packet;

            // Notify first, so that handler has a chance to get a
            // reference from the ObjectTracker to the object being killed
            uint[] killed = new uint[kill.ObjectData.Length];
            for (int i = 0; i < kill.ObjectData.Length; i++)
            {
                OnKillObject(new KillObjectEventArgs(null, kill.ObjectData[i].ID));
                killed[i] = kill.ObjectData[i].ID;
            }
            OnKillObjects(new KillObjectsEventArgs(null, killed));


            lock (region.ObjectsPrimitives)
            {
                List<uint> removeAvatars = new List<uint>();
                List<uint> removePrims = new List<uint>();

                //if (Client.Settings.OBJECT_TRACKING)
                {
                    uint localID;
                    for (int i = 0; i < kill.ObjectData.Length; i++)
                    {
                        localID = kill.ObjectData[i].ID;

                        if (region.ObjectsPrimitives.ContainsKey(localID))
                            removePrims.Add(localID);

                        region.ObjectsPrimitives.ForEach(prim =>
                        {
                            if (prim.Value.ParentID == localID)
                            {
                                OnKillObject(new KillObjectEventArgs(null, prim.Key));
                                removePrims.Add(prim.Key);
                            }
                        });
                    }
                }

                //if (Client.Settings.AVATAR_TRACKING)
                {
                    lock (region.ObjectsAvatars)
                    {
                        uint localID;
                        for (int i = 0; i < kill.ObjectData.Length; i++)
                        {
                            localID = kill.ObjectData[i].ID;

                            if (region.ObjectsAvatars.ContainsKey(localID))
                                removeAvatars.Add(localID);

                            List<uint> rootPrims = new List<uint>();

                            region.ObjectsPrimitives.ForEach(prim =>
                            {
                                if (prim.Value.ParentID == localID)
                                {
                                    OnKillObject(new KillObjectEventArgs(null, prim.Key));
                                    removePrims.Add(prim.Key);
                                    rootPrims.Add(prim.Key);
                                }
                            });

                            region.ObjectsPrimitives.ForEach(prim =>
                            {
                                if (rootPrims.Contains(prim.Value.ParentID))
                                {
                                    OnKillObject(new KillObjectEventArgs(null, prim.Key));
                                    removePrims.Add(prim.Key);
                                }
                            });
                        }

                        //Do the actual removing outside of the loops but still inside the lock.
                        //This safely prevents the collection from being modified during a loop.
                        foreach (uint removeID in removeAvatars)
                            region.ObjectsAvatars.Remove(removeID);
                    }
                }

                //if (Client.Settings.CACHE_PRIMITIVES)
                //{
                //    simulator.DataPool.ReleasePrims(removePrims);
                //}
                foreach (uint removeID in removePrims)
                    region.ObjectsPrimitives.Remove(removeID);
            }
            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ObjectPropertiesHandler(Packet packet, RegionProxy region)
        {
            ObjectPropertiesPacket op = (ObjectPropertiesPacket)packet;
            ObjectPropertiesPacket.ObjectDataBlock[] datablocks = op.ObjectData;

            for (int i = 0; i < datablocks.Length; ++i)
            {
                ObjectPropertiesPacket.ObjectDataBlock objectData = datablocks[i];
                Primitive.ObjectProperties props = new Primitive.ObjectProperties();

                props.ObjectID = objectData.ObjectID;
                props.AggregatePerms = objectData.AggregatePerms;
                props.AggregatePermTextures = objectData.AggregatePermTextures;
                props.AggregatePermTexturesOwner = objectData.AggregatePermTexturesOwner;
                props.Permissions = new Permissions(objectData.BaseMask, objectData.EveryoneMask, objectData.GroupMask,
                    objectData.NextOwnerMask, objectData.OwnerMask);
                props.Category = (ObjectCategory)objectData.Category;
                props.CreationDate = Utils.UnixTimeToDateTime((uint)objectData.CreationDate);
                props.CreatorID = objectData.CreatorID;
                props.Description = Utils.BytesToString(objectData.Description);
                props.FolderID = objectData.FolderID;
                props.FromTaskID = objectData.FromTaskID;
                props.GroupID = objectData.GroupID;
                props.InventorySerial = objectData.InventorySerial;
                props.ItemID = objectData.ItemID;
                props.LastOwnerID = objectData.LastOwnerID;
                props.Name = Utils.BytesToString(objectData.Name);
                props.OwnerID = objectData.OwnerID;
                props.OwnershipCost = objectData.OwnershipCost;
                props.SalePrice = objectData.SalePrice;
                props.SaleType = (SaleType)objectData.SaleType;
                props.SitName = Utils.BytesToString(objectData.SitName);
                props.TouchName = Utils.BytesToString(objectData.TouchName);

                int numTextures = objectData.TextureID.Length / 16;
                props.TextureIDs = new UUID[numTextures];
                for (int j = 0; j < numTextures; ++j)
                    props.TextureIDs[j] = new UUID(objectData.TextureID, j * 16);

                //if (Client.Settings.OBJECT_TRACKING)
                //{
                    //Primitive findPrim = this.ObjectsPrimitives.Find(
                    //    delegate (Primitive prim) { return prim.ID == props.ObjectID; });
                    Primitive findPrim = region.ObjectsPrimitives.Find(x => { return x.ID == props.ObjectID; });

                    if (findPrim != null)
                    {
                        OnObjectPropertiesUpdated(new ObjectPropertiesUpdatedEventArgs(null, findPrim, props));

                        lock (region.ObjectsPrimitives)
                        {
                            if (region.ObjectsPrimitives.ContainsKey(findPrim.LocalID))
                                region.ObjectsPrimitives[findPrim.LocalID].Properties = props;
                        }
                    }
                //}

                OnObjectProperties(new ObjectPropertiesEventArgs(region, findPrim, props));
            }
            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ObjectPropertiesFamilyHandler(Packet packet, RegionProxy region)
        {
            ObjectPropertiesFamilyPacket op = (ObjectPropertiesFamilyPacket)packet;
            Primitive.ObjectProperties props = new Primitive.ObjectProperties();

            ReportType requestType = (ReportType)op.ObjectData.RequestFlags;

            props.ObjectID = op.ObjectData.ObjectID;
            props.Category = (ObjectCategory)op.ObjectData.Category;
            props.Description = Utils.BytesToString(op.ObjectData.Description);
            props.GroupID = op.ObjectData.GroupID;
            props.LastOwnerID = op.ObjectData.LastOwnerID;
            props.Name = Utils.BytesToString(op.ObjectData.Name);
            props.OwnerID = op.ObjectData.OwnerID;
            props.OwnershipCost = op.ObjectData.OwnershipCost;
            props.SalePrice = op.ObjectData.SalePrice;
            props.SaleType = (SaleType)op.ObjectData.SaleType;
            props.Permissions.BaseMask = (PermissionMask)op.ObjectData.BaseMask;
            props.Permissions.EveryoneMask = (PermissionMask)op.ObjectData.EveryoneMask;
            props.Permissions.GroupMask = (PermissionMask)op.ObjectData.GroupMask;
            props.Permissions.NextOwnerMask = (PermissionMask)op.ObjectData.NextOwnerMask;
            props.Permissions.OwnerMask = (PermissionMask)op.ObjectData.OwnerMask;

            //if (Client.Settings.OBJECT_TRACKING)
            //{
                //Primitive findPrim = this.ObjectsPrimitives.Find(
                //        delegate (Primitive prim) { return prim.ID == op.ObjectData.ObjectID; });
                Primitive findPrim = region.ObjectsPrimitives.Find(x => { return x.ID == op.ObjectData.ObjectID; });

                if (findPrim != null)
                {
                    lock (region.ObjectsPrimitives)
                    {
                        if (region.ObjectsPrimitives.ContainsKey(findPrim.LocalID))
                        {
                            if (region.ObjectsPrimitives[findPrim.LocalID].Properties == null)
                                region.ObjectsPrimitives[findPrim.LocalID].Properties = new Primitive.ObjectProperties();
                            region.ObjectsPrimitives[findPrim.LocalID].Properties.SetFamilyProperties(props);
                        }
                    }
                }
            //}

            OnObjectPropertiesFamily(new ObjectPropertiesFamilyEventArgs(region, findPrim, props, requestType));
            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet PayPriceReplyHandler(Packet packet, RegionProxy region)
        {
            if (m_PayPriceReply != null)
            {
                PayPriceReplyPacket p = (PayPriceReplyPacket)packet;
                UUID objectID = p.ObjectData.ObjectID;
                int defaultPrice = p.ObjectData.DefaultPayPrice;
                int[] buttonPrices = new int[p.ButtonData.Length];

                for (int i = 0; i < p.ButtonData.Length; i++)
                {
                    buttonPrices[i] = p.ButtonData[i].PayButton;
                }

                OnPayPriceReply(new PayPriceReplyEventArgs(null, objectID, defaultPrice, buttonPrices));
            }
            return packet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capsKey"></param>
        /// <param name="message"></param>
        /// <param name="simulator"></param>
        //protected void ObjectPhysicsPropertiesHandler(string capsKey, IMessage message, RegionProxy simulator)
        //{
        //    ObjectPhysicsPropertiesMessage msg = (ObjectPhysicsPropertiesMessage)message;

        //    //if (Client.Settings.OBJECT_TRACKING)
        //    {
        //        for (int i = 0; i < msg.ObjectPhysicsProperties.Length; i++)
        //        {
        //            lock (this.ObjectsPrimitives.Dictionary)
        //            {
        //                if (this.ObjectsPrimitives.Dictionary.ContainsKey(msg.ObjectPhysicsProperties[i].LocalID))
        //                {
        //                    this.ObjectsPrimitives.Dictionary[msg.ObjectPhysicsProperties[i].LocalID].PhysicsProps = msg.ObjectPhysicsProperties[i];
        //                }
        //            }
        //        }
        //    }

        //    if (m_PhysicsProperties != null)
        //    {
        //        for (int i = 0; i < msg.ObjectPhysicsProperties.Length; i++)
        //        {
        //            OnPhysicsProperties(new PhysicsPropertiesEventArgs(simulator, msg.ObjectPhysicsProperties[i]));
        //        }
        //    }
        //}

        #endregion Packet Handlers

        #region Object Tracking Link

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="fullID"></param>
        /// <returns></returns>
        protected Primitive GetPrimitive(RegionProxy sim, uint localID, UUID fullID)
        {
            return GetPrimitive(sim, localID, fullID, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="fullID"></param>
        /// <param name="createIfMissing"></param>
        /// <returns></returns>
        public Primitive GetPrimitive(RegionProxy sim, uint localID, UUID fullID, bool createIfMissing)
        {
            //if (Client.Settings.OBJECT_TRACKING)
            {
                lock (Frame.Network.CurrentSim.ObjectsPrimitives)
                {

                    Primitive prim;

                    if (sim.ObjectsPrimitives.TryGetValue(localID, out prim))
                    {
                        return prim;
                    }
                    else
                    {
                        if (!createIfMissing) return null;
                        //if (Client.Settings.CACHE_PRIMITIVES)
                        if(false)
                        {
                            //prim = simulator.DataPool.MakePrimitive(localID);
                        }
                        else
                        {
                            prim = new Primitive();
                            prim.LocalID = localID;
                            prim.RegionHandle = sim.Handle;
                        }
                        prim.ActiveClients++;
                        prim.ID = fullID;

                        sim.ObjectsPrimitives[localID] = prim;
                        //this.ObjectsPrimitives.Add(localID, prim);

                        return prim;
                    }
                }
            }
            //else
            //{
            //    return new Primitive();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="fullID"></param>
        /// <returns></returns>
        protected Avatar GetAvatar(RegionProxy sim, uint localID, UUID fullID)
        {
            //if (Client.Settings.AVATAR_TRACKING)
            {
                lock (sim.ObjectsAvatars)
                {
                    Avatar avatar;

                    if (sim.ObjectsAvatars.TryGetValue(localID, out avatar))
                    {
                        return avatar;
                    }
                    else
                    {
                        avatar = new Avatar();
                        avatar.LocalID = localID;
                        avatar.ID = fullID;
                        avatar.RegionHandle = sim.Handle;

                        sim.ObjectsAvatars[localID] = avatar;
                        //this.ObjectsAvatars.Add(localID, avatar);

                        return avatar;
                    }
                }
            }
            //else
            //{
            //    return new Avatar();
            //}
        }

        #endregion Object Tracking Link

        #region Delegates

        #region ObjectUpdate event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PrimEventArgs> m_ObjectUpdate;

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// A <see cref="Primitive"/>, Foliage or Attachment</summary>
        /// <seealso cref="RequestObject"/>
        /// <seealso cref="RequestObjects"/>
        public event EventHandler<PrimEventArgs> ObjectUpdate
        {
            add { lock (m_ObjectUpdateLock) { m_ObjectUpdate += value; } }
            remove { lock (m_ObjectUpdateLock) { m_ObjectUpdate -= value; } }
        }
        #endregion ObjectUpdate event

        #region ObjectProperties event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesEventArgs> m_ObjectProperties;

        ///<summary>Raises the ObjectProperties Event</summary>
        /// <param name="e">A ObjectPropertiesEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectProperties(ObjectPropertiesEventArgs e)
        {
            EventHandler<ObjectPropertiesEventArgs> handler = m_ObjectProperties;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// additional <seea cref="Primitive"/> information</summary>
        /// <seealso cref="SelectObject"/>
        /// <seealso cref="SelectObjects"/>
        public event EventHandler<ObjectPropertiesEventArgs> ObjectProperties
        {
            add { lock (m_ObjectPropertiesLock) { m_ObjectProperties += value; } }
            remove { lock (m_ObjectPropertiesLock) { m_ObjectProperties -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesUpdatedEventArgs> m_ObjectPropertiesUpdated;

        ///<summary>Raises the ObjectPropertiesUpdated Event</summary>
        /// <param name="e">A ObjectPropertiesUpdatedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectPropertiesUpdated(ObjectPropertiesUpdatedEventArgs e)
        {
            EventHandler<ObjectPropertiesUpdatedEventArgs> handler = m_ObjectPropertiesUpdated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesUpdatedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// Primitive.ObjectProperties for an object we are currently tracking</summary>
        public event EventHandler<ObjectPropertiesUpdatedEventArgs> ObjectPropertiesUpdated
        {
            add { lock (m_ObjectPropertiesUpdatedLock) { m_ObjectPropertiesUpdated += value; } }
            remove { lock (m_ObjectPropertiesUpdatedLock) { m_ObjectPropertiesUpdated -= value; } }
        }
        #endregion ObjectProperties event

        #region ObjectPropertiesFamily event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesFamilyEventArgs> m_ObjectPropertiesFamily;

        ///<summary>Raises the ObjectPropertiesFamily Event</summary>
        /// <param name="e">A ObjectPropertiesFamilyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectPropertiesFamily(ObjectPropertiesFamilyEventArgs e)
        {
            EventHandler<ObjectPropertiesFamilyEventArgs> handler = m_ObjectPropertiesFamily;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesFamilyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// additional <seea cref="Primitive"/> and <see cref="Avatar"/> details</summary>
        /// <seealso cref="RequestObjectPropertiesFamily"/>
        public event EventHandler<ObjectPropertiesFamilyEventArgs> ObjectPropertiesFamily
        {
            add { lock (m_ObjectPropertiesFamilyLock) { m_ObjectPropertiesFamily += value; } }
            remove { lock (m_ObjectPropertiesFamilyLock) { m_ObjectPropertiesFamily -= value; } }
        }
        #endregion ObjectPropertiesFamily

        #region AvatarUpdate event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarUpdateEventArgs> m_AvatarUpdate;
        private EventHandler<ParticleUpdateEventArgs> m_ParticleUpdate;

        ///<summary>Raises the AvatarUpdate Event</summary>
        /// <param name="e">A AvatarUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarUpdate(AvatarUpdateEventArgs e)
        {
            EventHandler<AvatarUpdateEventArgs> handler = m_AvatarUpdate;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>
        /// Raises the ParticleUpdate Event
        /// </summary>
        /// <param name="e">A ParticleUpdateEventArgs object containing 
        /// the data sent from the simulator</param>
        protected virtual void OnParticleUpdate(ParticleUpdateEventArgs e)
        {
            EventHandler<ParticleUpdateEventArgs> handler = m_ParticleUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarUpdateLock = new object();

        private readonly object m_ParticleUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// updated information for an <see cref="Avatar"/></summary>
        public event EventHandler<AvatarUpdateEventArgs> AvatarUpdate
        {
            add { lock (m_AvatarUpdateLock) { m_AvatarUpdate += value; } }
            remove { lock (m_AvatarUpdateLock) { m_AvatarUpdate -= value; } }
        }
        #endregion AvatarUpdate event

        #region TerseObjectUpdate event
        public event EventHandler<ParticleUpdateEventArgs> ParticleUpdate
        {
            add { lock (m_ParticleUpdateLock) { m_ParticleUpdate += value; } }
            remove { lock (m_ParticleUpdateLock) { m_ParticleUpdate -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<TerseObjectUpdateEventArgs> m_TerseObjectUpdate;

        /// <summary>Thread sync lock object</summary>
        private readonly object m_TerseObjectUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// <see cref="Primitive"/> and <see cref="Avatar"/> movement changes</summary>
        public event EventHandler<TerseObjectUpdateEventArgs> TerseObjectUpdate
        {
            add { lock (m_TerseObjectUpdateLock) { m_TerseObjectUpdate += value; } }
            remove { lock (m_TerseObjectUpdateLock) { m_TerseObjectUpdate -= value; } }
        }
        #endregion TerseObjectUpdate event

        #region ObjectDataBlockUpdate event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectDataBlockUpdateEventArgs> m_ObjectDataBlockUpdate;

        ///<summary>Raises the ObjectDataBlockUpdate Event</summary>
        /// <param name="e">A ObjectDataBlockUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectDataBlockUpdate(ObjectDataBlockUpdateEventArgs e)
        {
            EventHandler<ObjectDataBlockUpdateEventArgs> handler = m_ObjectDataBlockUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectDataBlockUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// updates to an Objects DataBlock</summary>
        public event EventHandler<ObjectDataBlockUpdateEventArgs> ObjectDataBlockUpdate
        {
            add { lock (m_ObjectDataBlockUpdateLock) { m_ObjectDataBlockUpdate += value; } }
            remove { lock (m_ObjectDataBlockUpdateLock) { m_ObjectDataBlockUpdate -= value; } }
        }
        #endregion ObjectDataBlockUpdate event

        #region KillObject event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<KillObjectEventArgs> m_KillObject;

        ///<summary>Raises the KillObject Event</summary>
        /// <param name="e">A KillObjectEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnKillObject(KillObjectEventArgs e)
        {
            EventHandler<KillObjectEventArgs> handler = m_KillObject;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_KillObjectLock = new object();

        /// <summary>Raised when the simulator informs us an <see cref="Primitive"/>
        /// or <see cref="Avatar"/> is no longer within view</summary>
        public event EventHandler<KillObjectEventArgs> KillObject
        {
            add { lock (m_KillObjectLock) { m_KillObject += value; } }
            remove { lock (m_KillObjectLock) { m_KillObject -= value; } }
        }
        #endregion KillObject event

        #region KillObjects event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<KillObjectsEventArgs> m_KillObjects;

        ///<summary>Raises the KillObjects Event</summary>
        /// <param name="e">A KillObjectsEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnKillObjects(KillObjectsEventArgs e)
        {
            EventHandler<KillObjectsEventArgs> handler = m_KillObjects;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_KillObjectsLock = new object();

        /// <summary>Raised when the simulator informs us when a group of <see cref="Primitive"/>
        /// or <see cref="Avatar"/> is no longer within view</summary>
        public event EventHandler<KillObjectsEventArgs> KillObjects
        {
            add { lock (m_KillObjectsLock) { m_KillObjects += value; } }
            remove { lock (m_KillObjectsLock) { m_KillObjects -= value; } }
        }
        #endregion KillObjects event

        #region AvatarSitChanged event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarSitChangedEventArgs> m_AvatarSitChanged;

        ///<summary>Raises the AvatarSitChanged Event</summary>
        /// <param name="e">A AvatarSitChangedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarSitChanged(AvatarSitChangedEventArgs e)
        {
            EventHandler<AvatarSitChangedEventArgs> handler = m_AvatarSitChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarSitChangedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// updated sit information for our <see cref="Avatar"/></summary>
        public event EventHandler<AvatarSitChangedEventArgs> AvatarSitChanged
        {
            add { lock (m_AvatarSitChangedLock) { m_AvatarSitChanged += value; } }
            remove { lock (m_AvatarSitChangedLock) { m_AvatarSitChanged -= value; } }
        }
        #endregion AvatarSitChanged event

        #region PayPriceReply event
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PayPriceReplyEventArgs> m_PayPriceReply;

        ///<summary>Raises the PayPriceReply Event</summary>
        /// <param name="e">A PayPriceReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPayPriceReply(PayPriceReplyEventArgs e)
        {
            EventHandler<PayPriceReplyEventArgs> handler = m_PayPriceReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PayPriceReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// purchase price information for a <see cref="Primitive"/></summary>
        public event EventHandler<PayPriceReplyEventArgs> PayPriceReply
        {
            add { lock (m_PayPriceReplyLock) { m_PayPriceReply += value; } }
            remove { lock (m_PayPriceReplyLock) { m_PayPriceReply -= value; } }
        }
        #endregion PayPriceReply

        #region PhysicsProperties event
        /// <summary>
        /// Callback for getting object media data via CAP
        /// </summary>
        /// <param name="success">Indicates if the operation was succesfull</param>
        /// <param name="version">Object media version string</param>
        /// <param name="faceMedia">Array indexed on prim face of media entry data</param>
        public delegate void ObjectMediaCallback(bool success, string version, MediaEntry[] faceMedia);

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PhysicsPropertiesEventArgs> m_PhysicsProperties;

        ///<summary>Raises the PhysicsProperties Event</summary>
        /// <param name="e">A PhysicsPropertiesEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPhysicsProperties(PhysicsPropertiesEventArgs e)
        {
            EventHandler<PhysicsPropertiesEventArgs> handler = m_PhysicsProperties;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PhysicsPropertiesLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// additional <seea cref="Primitive"/> information</summary>
        /// <seealso cref="SelectObject"/>
        /// <seealso cref="SelectObjects"/>
        public event EventHandler<PhysicsPropertiesEventArgs> PhysicsProperties
        {
            add { lock (m_PhysicsPropertiesLock) { m_PhysicsProperties += value; } }
            remove { lock (m_PhysicsPropertiesLock) { m_PhysicsProperties -= value; } }
        }
        #endregion PhysicsProperties event

        #endregion Delegates
    }

    #region EventArgs
    /// <summary>Provides data for the <see cref="ObjectManager.ObjectUpdate"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectUpdate"/> event occurs when the simulator sends
    /// an <see cref="ObjectUpdatePacket"/> containing a Primitive, Foliage or Attachment data</para>
    /// <para>Note 1: The <see cref="ObjectManager.ObjectUpdate"/> event will not be raised when the object is an Avatar</para>
    /// <para>Note 2: It is possible for the <see cref="ObjectManager.ObjectUpdate"/> to be 
    /// raised twice for the same object if for example the primitive moved to a new simulator, then returned to the current simulator or
    /// if an Avatar crosses the border into a new simulator and returns to the current simulator</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="PrimEventArgs.Prim"/>, <see cref="PrimEventArgs.RegionProxy"/>, and <see cref="PrimEventArgs.IsAttachment"/>
    /// properties to display new Primitives and Attachments on the <see cref="Console"/> window.
    /// <code>
    ///     // Subscribe to the event that gives us prim and foliage information
    ///     Client.Objects.ObjectUpdate += Objects_ObjectUpdate;
    ///     
    ///
    ///     private void Objects_ObjectUpdate(object sender, PrimEventArgs e)
    ///     {
    ///         Console.WriteLine("Primitive {0} {1} in {2} is an attachment {3}", e.Prim.ID, e.Prim.LocalID, e.RegionProxy.Name, e.IsAttachment);
    ///     }
    /// </code>
    /// </example>
    /// <seealso cref="ObjectManager.ObjectUpdate"/>
    /// <seealso cref="ObjectManager.AvatarUpdate"/>
    /// <seealso cref="AvatarUpdateEventArgs"/>
    public class PrimEventArgs : EventArgs
    {
        private readonly RegionProxy m_Region;
        private readonly bool m_IsNew;
        private readonly bool m_IsAttachment;
        private readonly Primitive m_Prim;
        private readonly ushort m_TimeDilation;

        /// <summary>Get the simulator the <see cref="Primitive"/> originated from</summary>
        public RegionProxy Region { get { return m_Region; } }
        /// <summary>Get the <see cref="Primitive"/> details</summary>
        public Primitive Prim { get { return m_Prim; } }
        /// <summary>true if the <see cref="Primitive"/> did not exist in the dictionary before this update (always true if object tracking has been disabled)</summary>
        public bool IsNew { get { return m_IsNew; } }
        /// <summary>true if the <see cref="Primitive"/> is attached to an <see cref="Avatar"/></summary>
        public bool IsAttachment { get { return m_IsAttachment; } }
        /// <summary>Get the simulator Time Dilation</summary>
        public ushort TimeDilation { get { return m_TimeDilation; } }

        /// <summary>
        /// Construct a new instance of the PrimEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the object originated from</param>
        /// <param name="prim">The Primitive</param>
        /// <param name="timeDilation">The simulator time dilation</param>
        /// <param name="isNew">The prim was not in the dictionary before this update</param>
        /// <param name="isAttachment">true if the primitive represents an attachment to an agent</param>
        public PrimEventArgs(RegionProxy region, Primitive prim, ushort timeDilation, bool isNew, bool isAttachment)
        {
            this.m_Region = region;
            this.m_IsNew = isNew;
            this.m_IsAttachment = isAttachment;
            this.m_Prim = prim;
            this.m_TimeDilation = timeDilation;
        }
    }


    /// <summary>Provides primitive data containing updated location, velocity, rotation, textures for the <see cref="ObjectManager.TerseObjectUpdate"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.TerseObjectUpdate"/> event occurs when the simulator sends updated location, velocity, rotation, etc</para>        
    /// </remarks>
    public class TerseObjectUpdateEventArgs : EventArgs
    {
        private readonly RegionProxy m_Region;
        private readonly Primitive m_Prim;
        private readonly ObjectMovementUpdate m_Update;
        private readonly ushort m_TimeDilation;

        /// <summary>Get the simulator the object is located</summary>
        public RegionProxy Region { get { return m_Region; } }
        /// <summary>Get the primitive details</summary>
        public Primitive Prim { get { return m_Prim; } }
        /// <summary></summary>
        public ObjectMovementUpdate Update { get { return m_Update; } }
        /// <summary></summary>
        public ushort TimeDilation { get { return m_TimeDilation; } }

        public TerseObjectUpdateEventArgs(RegionProxy region, Primitive prim, ObjectMovementUpdate update, ushort timeDilation)
        {
            this.m_Region = region;
            this.m_Prim = prim;
            this.m_Update = update;
            this.m_TimeDilation = timeDilation;
        }
    }


    /// <summary>Provides additional primitive data for the <see cref="ObjectManager.ObjectProperties"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectProperties"/> event occurs when the simulator sends
    /// an <see cref="ObjectPropertiesPacket"/> containing additional details for a Primitive, Foliage data or Attachment data</para>
    /// <para>The <see cref="ObjectManager.ObjectProperties"/> event is also raised when a <see cref="ObjectManager.SelectObject"/> request is
    /// made.</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="PrimEventArgs.Prim"/>, <see cref="PrimEventArgs.Simulator"/> and
    /// <see cref="ObjectPropertiesEventArgs.Properties"/>
    /// properties to display new attachments and send a request for additional properties containing the name of the
    /// attachment then display it on the <see cref="Console"/> window.
    /// <code>    
    ///     // Subscribe to the event that provides additional primitive details
    ///     Client.Objects.ObjectProperties += Objects_ObjectProperties;
    ///      
    ///     // handle the properties data that arrives
    ///     private void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)
    ///     {
    ///         Console.WriteLine("Primitive Properties: {0} Name is {1}", e.Properties.ObjectID, e.Properties.Name);
    ///     }   
    /// </code>
    /// </example>
    public class ObjectPropertiesEventArgs : EventArgs
    {
        protected readonly RegionProxy m_Simulator;
        protected readonly Primitive.ObjectProperties m_Properties;

        /// <summary>Get the simulator the object is located</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive properties</summary>
        public Primitive.ObjectProperties Properties { get { return m_Properties; } }

        public Primitive Prim { get; private set; }

        /// <summary>
        /// Construct a new instance of the ObjectPropertiesEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the object is located</param>
        /// <param name="props">The primitive Properties</param>
        public ObjectPropertiesEventArgs(RegionProxy simulator, Primitive prim, Primitive.ObjectProperties props)
        {
            this.m_Simulator = simulator;
            this.Prim = prim;
            this.m_Properties = props;
        }
    }


    /// <summary>Provides additional primitive data, permissions and sale info for the <see cref="ObjectManager.ObjectPropertiesFamily"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectPropertiesFamily"/> event occurs when the simulator sends
    /// an <see cref="ObjectPropertiesPacket"/> containing additional details for a Primitive, Foliage data or Attachment. This includes
    /// Permissions, Sale info, and other basic details on an object</para>
    /// <para>The <see cref="ObjectManager.ObjectProperties"/> event is also raised when a <see cref="ObjectManager.RequestObjectPropertiesFamily"/> request is
    /// made, the viewer equivalent is hovering the mouse cursor over an object</para>
    /// </remarks>    
    public class ObjectPropertiesFamilyEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly Primitive.ObjectProperties m_Properties;
        private readonly ReportType m_Type;

        /// <summary>Get the simulator the object is located</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public Primitive.ObjectProperties Properties { get { return m_Properties; } }
        /// <summary></summary>
        public ReportType Type { get { return m_Type; } }

        public Primitive Prim { get; private set; }

        public ObjectPropertiesFamilyEventArgs(RegionProxy simulator, Primitive prim, Primitive.ObjectProperties props, ReportType type)
        {
            this.m_Simulator = simulator;
            this.Prim = prim;
            this.m_Properties = props;
            this.m_Type = type;
        }
    }
    #endregion
}
