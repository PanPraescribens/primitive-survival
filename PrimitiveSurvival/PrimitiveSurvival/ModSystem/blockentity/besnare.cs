namespace PrimitiveSurvival.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using System;
    using System.Linq;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.GameContent;
    using Vintagestory.API.Config;
    using Vintagestory.API.Common.Entities;
    using System.Diagnostics;

    public class BESnare : BlockEntityDisplay, IAnimalFoodSource
    {

        private readonly string[] baitTypes = { "fruit", "grain", "legume", "meat", "vegetable", "jerky", "mushroom", "bread", "poultry", "pickledvegetable", "redmeat", "bushmeat", "cheese", "fishfillet", "fisheggs", "fisheggscooked" };
        protected static readonly Random Rnd = new Random();
        private readonly int maxSlots = 1;

        public ItemSlot BaitSlot => this.inventory[0];

        public ItemStack BaitStack
        {
            get => this.inventory[0].Itemstack;
            set => this.inventory[0].Itemstack = value;
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (this.inventory != null)
            {
                if (!this.BaitSlot.Empty)
                {
                    if (this.BaitStack.Item != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Item.FirstCodePart()) >= 0)
                        {
                            if (this.Api.Side == EnumAppSide.Server)
                            { this.Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                        }
                    }
                    else if (this.BaitStack.Block != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Block.FirstCodePart()) >= 0)
                        {
                            if (this.Api.Side == EnumAppSide.Server)
                            { this.Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                        }
                    }
                }
            }
        }


        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (this.Api.Side == EnumAppSide.Server)
            { this.Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
        }


        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            if (this.Api.Side == EnumAppSide.Server)
            { this.Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
        }


        #region IAnimalFoodSource impl
        public bool IsSuitableFor(Entity entity) => true;

        public float ConsumeOnePortion()
        {
            this.TryClearContents();
            return 1f;
        }

        public string Type => "food";

        public Vec3d Position => Pos.ToVec3d().Add(0.5, 0.5, 0.5);
        #endregion


        public override string InventoryClassName => "snare";
        protected InventoryGeneric inventory;

        public override InventoryBase Inventory => this.inventory;


        public BESnare()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            this.meshes = new MeshData[this.maxSlots];
        }


        public bool TryClearContents()
        {
            if (!this.BaitSlot.Empty)
            {
                this.BaitSlot.TakeOutWhole();
                return true;
            }
            return false;
        }


        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot.Empty)
            {
                if (this.TryTake(byPlayer, blockSel))
                {
                    if (this.Api.Side == EnumAppSide.Server)
                    { this.Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                    return true;
                }
                return false;
            }
            else
            {
                var colObj = playerSlot.Itemstack.Collectible;
                if (colObj.Attributes != null)
                {
                    if (this.TryPut(playerSlot))
                    {
                        if (this.Api.Side == EnumAppSide.Server)
                        { this.Api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this); }
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }


        private bool TryPut(ItemSlot playerSlot)
        {
            var index = -1;
            var playerStack = playerSlot.Itemstack;
            if (this.inventory != null)
            {
                var stacks = this.inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                index = stacks.Count();
                if (index >= this.maxSlots)
                { return false; }
            }

            if (playerStack.Item != null)
            {
                if (Array.IndexOf(this.baitTypes, playerStack.Item.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                { index = 0; }
                else
                { return false; }
            }
            else if (playerStack.Block != null)
            {
                if (Array.IndexOf(this.baitTypes, playerStack.Block.FirstCodePart()) >= 0 && this.BaitSlot.Empty)
                { index = 0; }
                else
                { return false; }
            }

            if (index != -1)
            {
                var moved = playerSlot.TryPutInto(this.Api.World, this.inventory[index]);
                if (moved > 0)
                {
                    this.updateMesh(index);
                    this.MarkDirty(true);
                    return moved > 0;
                }
                else
                { return false; }
            }
            return false;
        }


        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!this.BaitSlot.Empty)
            {
                var stack = this.BaitSlot.TakeOut(1);
                if (stack.StackSize > 0)
                { this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5)); }
                this.updateMesh(0);
                this.MarkDirty(true);
                return true;
            }
            return false;
        }


        public void StealBait(BlockPos pos)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(pos) as BlockSnare;
            var stack = this.BaitSlot.TakeOut(1);
            {
                if (this.Api.Side == EnumAppSide.Server)
                {
                    this.Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
                    Debug.WriteLine("REMOVE POI");
                }
            }
            var blockPath = block.Code.Path;
            block = this.Api.World.BlockAccessor.GetBlock(block.CodeWithPath(blockPath)) as BlockSnare;
            this.Api.World.BlockAccessor.SetBlock(block.BlockId, pos);
            this.MarkDirty(true);
            this.updateMesh(0);
        }


        public void TripTrap(BlockPos pos)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(pos) as BlockSnare;
            var stack = this.BaitSlot.TakeOut(1);
            if (stack != null)
            {
                this.Api.World.SpawnItemEntity(stack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                if (this.Api.Side == EnumAppSide.Server)
                { this.Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this); }
            }
            var blockPath = block.Code.Path;
            blockPath = blockPath.Replace("set", "tripped");
            block = this.Api.World.BlockAccessor.GetBlock(block.CodeWithPath(blockPath)) as BlockSnare;
            this.Api.World.BlockAccessor.SetBlock(block.BlockId, pos);
            this.MarkDirty(true);
            this.updateMesh(0);
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos);
            if (block.Code.Path.Contains("tripped"))
            { sb.Append(Lang.Get("It's tripped. Sneak click to set it back up.")); }
            else
            {
                if (!this.BaitSlot.Empty)
                {
                    if (this.BaitStack.Item != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Item.FirstCodePart()) < 0)
                        { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                        else
                        { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                    }
                    else if (this.BaitStack.Block != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Block.FirstCodePart()) < 0)
                        { sb.Append(Lang.Get("Your bait has gone rotten. Replace it with fresh bait.")); }
                        else
                        { sb.Append(Lang.Get("It's baited so your odds of catching something are pretty good.")); }
                    }
                }
                else if (this.BaitSlot.Empty)
                { sb.Append(Lang.Get("Bait it with some food to increase your odds of catching something.")); }
            }
            sb.AppendLine().AppendLine();
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            var shapeBase = "primitivesurvival:shapes/";
            string shapePath;
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos) as BlockSnare;
            var texture = tesselator.GetTexSource(block);
            var tmpTextureSource = texture;

            if (block.FirstCodePart(1) == "set")
            { shapePath = "block/snare/snare-set"; }
            else
            { shapePath = "block/snare/snare-tripped"; }
            mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, -1, false, tesselator);
            mesher.AddMeshData(mesh);

            if (this.inventory != null)
            {
                if (!this.BaitSlot.Empty) //bait or rot
                {
                    var tripped = true;
                    if (block.FirstCodePart(1) == "set")
                    { tripped = false; }
                    if (this.BaitStack.Item != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Item.FirstCodePart()) < 0)
                        {
                            var tempblock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                        { tmpTextureSource = texture; }
                    }
                    else if (this.BaitStack.Block != null)
                    {
                        if (Array.IndexOf(this.baitTypes, this.BaitStack.Block.FirstCodePart()) < 0)
                        {
                            var tempblock = this.Api.World.GetBlock(block.CodeWithPath("texturerot"));
                            tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tempblock);
                        }
                        else
                        { tmpTextureSource = texture; }
                    }
                    shapePath = "block/trapbait"; //baited (for now)
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, tmpTextureSource, 0, tripped, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
            return true;
        }
    }
}
