namespace PrimitiveSurvival.ModSystem
{
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    //using System.Diagnostics;

    public class BEIrrigationVessel : BlockEntityLiquidContainer
    {
        public override string InventoryClassName => "irrigationvessel";
        private MeshData currentMesh;
        private BlockIrrigationVessel ownBlock;
        public float MeshAngle;
        public bool Buried;

        private readonly double updateMinutes = 1.0; //update neighbors once per minute

        public BEIrrigationVessel()
        {
            this.inventory = new InventoryGeneric(1, null, null);
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.ownBlock = this.Block as BlockIrrigationVessel;
            if (api.Side == EnumAppSide.Client)
            {
                this.currentMesh = this.GenMesh();
                this.MarkDirty(true);
            }
            else //server
            {
                var updateTick = this.RegisterGameTickListener(this.IrrigationVesselUpdate, (int)(this.updateMinutes * 60000));
            }
        }


        public void IrrigationVesselUpdate(float par)
        {
            if (this.inventory[0].Empty || !this.Buried)
            { return; } //empty or not buried
            if (!this.inventory[0].Itemstack.Collectible.Code.Path.Contains("water"))
            { return; } //not water

            var neibPos = new BlockPos[] {
                this.Pos.EastCopy(),
                this.Pos.EastCopy().NorthCopy(),
                this.Pos.NorthCopy(),
                this.Pos.NorthCopy().WestCopy(),
                this.Pos.SouthCopy(),
                this.Pos.SouthCopy().EastCopy(),
                this.Pos.WestCopy(),
                this.Pos.WestCopy().SouthCopy()
            }; //immediate neighbors

            double waterUsed = 0;
            // Examine sides of immediate neighbors
            foreach (var neib in neibPos)
            {
                if (this.Api.World.BlockAccessor.GetBlockEntity(neib) is BlockEntityFarmland be)
                {
                    //Debug.WriteLine(be.MoistureLevel);
                    //var testBlock = this.Api.World.BlockAccessor.GetBlock(neib);
                    if (be.MoistureLevel < 0.9)
                    {
                        // add 0.1 to it
                        be.WaterFarmland(0.2f, false); //no neighbors until nearby watered
                        waterUsed += 2;
                    }
                    else
                    {
                        //top it up to 100%
                        var topUp = 1.0f - be.MoistureLevel + 0.1f;
                        be.WaterFarmland(topUp, true); //neighbors too now
                        waterUsed += topUp * 5; //use less water in this scenario
                    }
                    var tree = new TreeAttribute();
                    be.ToTreeAttributes(tree);
                }
            }
            //Debug.WriteLine("topped up: " + waterUsed + " ml");
            this.inventory[0].TakeOut((int)waterUsed);
            this.MarkDirty(true);
        }

        public override void OnBlockBroken(IPlayer forPlayer)
        {
            // Don't drop inventory contents
        }


        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            this.Buried = false;
            base.OnBlockPlaced(byItemStack);
            if (this.Api.Side == EnumAppSide.Client)
            {
                this.currentMesh = this.GenMesh();
                this.MarkDirty(true);
            }
        }


        internal MeshData GenMesh()
        {
            if (this.ownBlock == null)
            { return null; }
            var mesh = this.ownBlock.GenMesh(this.Api as ICoreClientAPI, this.GetContent(), this.Pos);
            if (mesh.CustomInts != null)
            {
                for (var i = 0; i < mesh.CustomInts.Count; i++)
                {
                    mesh.CustomInts.Values[i] |= 1 << 27; // Disable water wavy
                    mesh.CustomInts.Values[i] |= 1 << 26; // Enabled weak foam
                }
            }
            return mesh;
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            ITexPositionSource tmpTextureSource;
            MeshData mesh;
            var shapePath = "primitivesurvival:shapes/block/clay/irrigationvessel/irrigationvessel";
            if (this.Api.World.BlockAccessor.GetBlock(this.Pos, BlockLayersAccess.Default) is BlockIrrigationVessel block)
            {
                tmpTextureSource = tesselator.GetTexSource(block);
                mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource);
                mesher.AddMeshData(mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, this.MeshAngle, 0));
                //add dirt
                if (this.Buried)
                {
                    shapePath = "primitivesurvival:shapes/block/clay/irrigationvessel/soildry";
                    if (!this.inventory.Empty)
                    {
                        if (this.inventory[0].Itemstack.Collectible.Code.Path.Contains("water"))
                        {
                            shapePath = "primitivesurvival:shapes/block/clay/irrigationvessel/soilwet";
                        }
                    }
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource);
                    mesher.AddMeshData(mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, this.MeshAngle, 0));
                }
            }
            // fruit press or something else?
            else
            {
                tmpTextureSource = tesselator.GetTexSource(this.ownBlock);
                mesh = this.ownBlock.GenMesh(this.Api as ICoreClientAPI, shapePath, tmpTextureSource); //, tesselator);
                mesher.AddMeshData(mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, this.MeshAngle, 0));
            }

            if (this.GetContent() != null)
            {
                shapePath = "primitivesurvival:shapes/block/clay/contents";
                if (this.GetContent().Block != null)
                {
                    tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(this.GetContent().Block);
                    //fuck this outside of main thread shit again
                    try
                    {
                        mesh = this.ownBlock.GenMesh(this.Api as ICoreClientAPI, this.GetContent(), this.Pos);
                        mesher.AddMeshData(mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, this.MeshAngle, 0));
                    }
                    catch
                    { }
                }
                else if (this.GetContent().Item != null)
                {
                    tmpTextureSource = ((ICoreClientAPI)this.Api).Tesselator.GetTextureSource(this.GetContent().Item);
                    //fuck this outside of main thread shit again
                    try
                    {
                        mesh = this.ownBlock.GenMesh(this.Api as ICoreClientAPI, this.GetContent(), this.Pos);
                        mesher.AddMeshData(mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, this.MeshAngle, 0));
                    }
                    catch { }
                }
            }
            return true;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            this.MeshAngle = tree.GetFloat("meshAngle", this.MeshAngle);
            this.Buried = tree.GetBool("buried", this.Buried);
            if (this.Api != null)
            {
                if (this.Api.Side == EnumAppSide.Client)
                {
                    this.currentMesh = this.GenMesh();
                    this.MarkDirty(true);
                }
            }
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("meshAngle", this.MeshAngle);
            tree.SetBool("buried", this.Buried);
        }


        //Pretty sure this aint doing shit
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            var slot = this.inventory[0];
            if (slot.Empty)
            { sb.AppendLine(Lang.Get("Empty")); }
            else
            { sb.AppendLine(Lang.Get("Contents: {0}x{1}", slot.Itemstack.StackSize, slot.Itemstack.GetName())); }
        }
    }
}
