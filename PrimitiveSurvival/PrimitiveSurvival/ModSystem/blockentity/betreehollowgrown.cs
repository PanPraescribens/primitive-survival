namespace PrimitiveSurvival.ModSystem
{
    using System.Text;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Config;
    using Vintagestory.API.Client;
    using Vintagestory.API.Server;
    using Vintagestory.GameContent;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.Util;
    using PrimitiveSurvival.ModConfig;
    //using System.Diagnostics;


    public class BETreeHollowGrown : BlockEntityDisplayCase, ITexPositionSource
    {
        private readonly int maxSlots = 8;
        public override string InventoryClassName => "treehollowgrown";

        protected InventoryGeneric inventory;


        private readonly double updateMinutes = ModConfig.Loaded.TreeHollowsUpdateMinutes;
        private long updateTick;

        //private const int MinItems = 1;
        //private const int MaxItems = 8;
        public override InventoryBase Inventory => this.inventory;

        public BETreeHollowGrown()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            this.meshes = new MeshData[this.maxSlots];
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            {
                var updateTick = this.RegisterGameTickListener(this.TreeHollowUpdate, (int)(this.updateMinutes * 60000));
            }
        }

        public void TreeHollowUpdate(float par)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos, BlockLayersAccess.Default) as BlockTreeHollowGrown;
            if (this.inventory[0].Empty)
            {
                var uf = new TreeHollows();
                uf.AddItemStacks(this, uf.MakeItemStacks(block.FirstCodePart(1), this.Api as ICoreServerAPI));
                this.MarkDirty();
            }
        }


        internal bool OnInteract(IPlayer byPlayer) //, BlockSelection blockSel)
        {
            if (this.Api.Side.IsServer()) //reset the listener on interact
            {
                this.UnregisterGameTickListener(this.updateTick);
                this.updateTick = this.RegisterGameTickListener(this.TreeHollowUpdate, (int)(this.updateMinutes * 60000));
            }

            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot.Empty)
            {
                if (this.TryTake(byPlayer)) //, blockSel))
                { return true; }
                return false;
            }
            // Comment out the put completely
            /*
            else
            {
                var colObj = playerSlot.Itemstack.Collectible;
                if (colObj.Attributes != null)
                {
                    {
                        if (this.TryPut(playerSlot, blockSel))
                        {
                            var sound = this.Block?.Sounds?.Place;
                            this.Api.World.PlaySoundAt(sound ?? new AssetLocation("game:sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                            return true;
                        }
                    }
                    return false;
                }
            }
            */
            return false;
        }

        internal void OnBreak() // IPlayer byPlayer, BlockPos pos)
        {
            for (var index = this.maxSlots - 1; index >= 0; index--)
            {
                if (!this.inventory[index].Empty)
                {
                    var stack = this.inventory[index].TakeOut(1);
                    if (stack.StackSize > 0)
                    { this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5)); }
                    this.MarkDirty(true);
                }
            }
        }

        // Returns the first available empty inventory slot
        // returns -1 if all slots are full
        /*
        private int FirstFreeSlot()
        {
            var slot = 0;
            var found = false;
            do
            {
                if (this.inventory[slot].Empty)
                { found = true; }
                else
                { slot++; }
            }
            while (slot < this.maxSlots && found == false);
            if (!found)
            { slot = -1; }
            //Debug.WriteLine("Free Slot:" + slot);
            return slot;
        }
        */

        // Returns the last filled inventory slot
        // returns -1 if all slots are empty
        private int LastFilledSlot()
        {
            var slot = this.maxSlots - 1;
            var found = false;
            do
            {
                if (!this.inventory[slot].Empty)
                { found = true; }
                else
                { slot--; }
            }
            while (slot > -1 && found == false);
            //Debug.WriteLine("Last Slot:" + slot);
            return slot;
        }

        /*
        //private bool TryPut(ItemSlot slot, BlockSelection blockSel)
        private bool TryPut()
        {
            return false; //always return false;
            var index = this.FirstFreeSlot();
            if (index == -1)
            { return false; } //inventory full

            var moved = slot.TryPutInto(this.Api.World, this.inventory[index]);
            if (moved > 0)
            {
                //this.updateMesh(index);
                this.MarkDirty(true);
            }
            return moved > 0;
        }
        */

        private bool TryTake(IPlayer byPlayer) //, BlockSelection blockSel)
        {
            var index = this.LastFilledSlot();
            if (index == -1)
            { return false; } //inventory empty

            var stack = this.inventory[index].TakeOut(1);
            if (byPlayer.InventoryManager.TryGiveItemstack(stack))
            {
                var sound = stack.Block?.Sounds?.Place;
                this.Api.World.PlaySoundAt(sound ?? new AssetLocation("game:sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
            }

            if (stack.StackSize > 0)
            {
                this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            //this.updateMesh(index);
            this.MarkDirty(true);
            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
            if (this.Api != null)
            {
                if (this.Api.Side == EnumAppSide.Client)
                { this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos); }
            }
        }

        public override void updateMeshes()
        {
            var index = this.LastFilledSlot() + 1;
            if (index == 0)
            { return; } //inventory empty
            for (var slot = 0; slot < index; slot++)
            {
                if (!this.inventory[slot].Empty)
                {
                    var stack = this.inventory[slot].Itemstack;
                    if (stack?.Item?.Shape != null)
                    {
                        if ((stack.Collectible as ItemWearable) == null)
                        {
                            this.updateMesh(slot);
                        }
                    }
                }
            }
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            if (!(this.Api.World.BlockAccessor.GetBlock(this.Pos, BlockLayersAccess.Default) is BlockTreeHollowGrown block))
            { return base.OnTesselation(mesher, tesselator); }
            mesh = this.capi.TesselatorManager.GetDefaultBlockMesh(block); //add tree hollow
            mesher.AddMeshData(mesh);

            //only render what is in the highest inventory slot
            var index = this.LastFilledSlot();
            if (index > -1)
            {
                var stack = this.inventory[index].Itemstack;
                if (stack != null)
                {
                    if (stack.Class == EnumItemClass.Block)
                    {
                        mesh = this.capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
                    }
                    else
                    {
                        this.nowTesselatingObj = stack.Item;
                        //var itemPath = stack.Item.Code.Path;
                        //var itemPathPart = stack.Item.FirstCodePart(1);
                        if (stack?.Item?.Shape != null)
                        {
                            if ((stack.Collectible as ItemWearable) == null)
                            {
                                this.capi.Tesselator.TesselateItem(stack.Item, out mesh, this);
                                if (mesh != null)
                                {
                                    mesh.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);
                                }
                            }
                        }
                        else
                        {
                            // seeds prolly
                            var shapeBase = "primitivesurvival:shapes/";
                            var shapePath = "block/trapbait"; //baited (for now)
                            var texture = tesselator.GetTexSource(block);
                            mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, tesselator);
                            if (mesh != null)
                            {
                                mesh.Translate(new Vec3f(0f, 0.03f, 0f));
                            }
                        }
                    }
                    if (mesh != null)
                    {
                        if (block.FirstCodePart(1).Contains("base"))
                        { mesh.Translate(new Vec3f(0f, 0.03f, 0f)); }
                        else //up has thicker bottom
                        { mesh.Translate(new Vec3f(0f, 0.23f, 0f)); }
                        if (block.LastCodePart() == "north" || block.LastCodePart() == "south")
                        {
                            mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, 90, 0);
                        }
                        mesher.AddMeshData(mesh);
                    }
                }
            }
            return true;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            //base.GetBlockInfo(forPlayer, sb);
            var index = this.LastFilledSlot();
            if (index == -1)
            { sb.AppendLine(Lang.Get("Empty")); }
            else
            {
                var desc = this.inventory[index].Itemstack.GetName();
                sb.AppendLine(Lang.Get(desc));
            }
            sb.AppendLine();
            if (forPlayer?.CurrentBlockSelection == null)
            { return; }
        }
    }
}
