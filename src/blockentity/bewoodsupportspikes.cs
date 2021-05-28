using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace primitiveSurvival
{
    public class BEWoodSupportSpikes : BlockEntityDisplay, IAnimalFoodSource
    {
        //public string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable" };
        protected static Random rnd = new Random();
        public int maxSlots = 4;
        //ICoreClientAPI capi;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (inventory != null)
            {
                if (!inventory[maxSlots - 1].Empty) //camouflaged means poi
                {
                    if (Api.Side == EnumAppSide.Server)
                    { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                }
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (Api.Side == EnumAppSide.Server)
            { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            if (Api.Side == EnumAppSide.Server)
            { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
        }

        #region IAnimalFoodSource impl
        public bool IsSuitableFor(Entity entity)
        { return true; }

        public float ConsumeOnePortion()
        {
            //TryClearContents();
            return 1f; //Was 0f
        }

        public string Type
        { get { return "food"; } }

        public Vec3d Position
        { get { return base.Pos.ToVec3d().Add(0.5, 0.5, 0.5); } }
        #endregion


        public override string InventoryClassName
        {
            get { return "woodsupportspikes"; }
        }
        protected InventoryGeneric inventory;

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }


        public BEWoodSupportSpikes()
        {
            inventory = new InventoryGeneric(maxSlots, null, null);
            meshes = new MeshData[maxSlots];
        }


        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot.Empty)
            {
                if (TryTake(byPlayer, blockSel))
                {
                    if (inventory[maxSlots - 1].Empty) //camouflaged means poi
                    {
                        if (Api.Side == EnumAppSide.Server)
                        { Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
                    }
                    return true;
                }
                return false;
            }
            else
            {
                if (TryPut(playerSlot, blockSel))
                {
                    if (!inventory[maxSlots - 1].Empty) //camouflaged means poi
                    {
                        if (Api.Side == EnumAppSide.Server)
                        { Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                    }
                    return true;
                }
                return false;

            }
        }


        private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
        {
            ItemStack playerStack = playerSlot.Itemstack;

            //find first available slot
            int availSlot = maxSlots;
            for (int slot = maxSlots - 1; slot >= 0; slot--)
            { if (inventory[slot].Empty) availSlot = slot; }
            if (availSlot == maxSlots) return false;

            bool canPlace = false;
            if (availSlot < maxSlots - 1)
            {
                if (playerStack.Item != null)
                { if (playerStack.Item.Code.Path == "drygrass") canPlace = true; }
                else if (playerStack.Block.BlockMaterial == EnumBlockMaterial.Plant) canPlace = true;
            }
            else
            {
                if (playerStack.Block != null)
                {
                    if (playerStack.Block.Fertility > 0) canPlace = true;
                }
            }
            if (canPlace)
            {
                playerSlot.TryPutInto(Api.World, inventory[availSlot]);
                MarkDirty(true);
                return true;
            }
            return false;
        }


        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int usedSlot = -1;
            for (int slot = 0; slot < maxSlots; slot++)
            { if (!inventory[slot].Empty) usedSlot = slot; }

            if (usedSlot > -1)
            {

                ItemStack stack = inventory[usedSlot].TakeOutWhole();
                if (stack.StackSize > 0)
                { byPlayer.InventoryManager.TryGiveItemstack(stack); }
                MarkDirty(true);
                return true;
            }
            return false;
        }

        public string GetBlockName(IWorldAccessor world, BlockPos pos)
        {
            if (inventory != null)
            {
                if (!inventory[3].Empty)
                {
                    ItemStack stack = inventory[3].Itemstack;
                    if (stack.Block != null)
                    {
                        string msg = Lang.Get(GlobalConstants.DefaultDomain + ":block-" + stack.Block.Code.Path);
                        return msg;
                    }
                }
            }
            return Lang.Get("Conceal Your Pit");
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            if (inventory != null)
            {
                string msg = "";
                if (inventory[0].Empty)
                { msg = Lang.Get("some"); }
                else if (inventory[1].Empty)
                { msg = Lang.Get("more"); }
                else if (inventory[2].Empty)
                { msg = Lang.Get("even more"); }
                if (msg != "")
                {
                    sb.Append(Lang.Get("Add") + " ").Append(msg).Append(" " + Lang.Get("dry grass or some other plant based material to conceal your pit"));
                }
                else if (inventory[3].Empty)
                { sb.AppendLine(Lang.Get("Add dirt or sand to camouflage your pit")); }

                if (inventory[3].Empty)
                { sb.AppendLine().AppendLine(); }
            }
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            string shapeBase = "primitivesurvival:shapes/";
            string shapePath;
            BlockWoodSupportSpikes block = Api.World.BlockAccessor.GetBlock(Pos) as BlockWoodSupportSpikes;
            ITexPositionSource texture = tesselator.GetTexSource(block);
            ITexPositionSource tmpTextureSource;
            shapePath = "block/woodsupportspikes";
            mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, -1, tesselator);
            mesher.AddMeshData(mesh);

            if (inventory != null)
            {
                int usedSlots = 0;
                for (int i = 0; i < maxSlots; i++)
                { if (!inventory[i].Empty) usedSlots++; }

                if (usedSlots > 0 && usedSlots < 4)
                {
                    Block tmpBlock = Api.World.GetBlock(block.CodeWithPath("foilage-" + usedSlots.ToString()));
                    shapePath = "block/foilage";
                    tmpTextureSource = tesselator.GetTexSource(tmpBlock);
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, usedSlots, tesselator);
                    mesher.AddMeshData(mesh);
                }
                else if (usedSlots == 4)
                {
                    shapePath = "block/foilage";
                    tmpTextureSource = tesselator.GetTexSource(inventory[3].Itemstack.Block);
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, usedSlots, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
            return true;
        }
    }
}