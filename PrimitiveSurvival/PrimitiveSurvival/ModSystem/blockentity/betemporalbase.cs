namespace PrimitiveSurvival.ModSystem
{
    using System;
    using System.Linq;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Vintagestory.API.Config;
    using PrimitiveSurvival.ModConfig;

    public class BETemporalBase : BlockEntityDisplay
    {

        private readonly int tickSeconds = 1;
        private readonly int maxSlots = 6;
        private static readonly Random Rnd = new Random();

        private readonly bool dropsGold = ModConfig.Loaded.AltarDropsGold;

        public override string InventoryClassName => "temporalbase";
        protected InventoryGeneric inventory;

        public override InventoryBase Inventory => this.inventory;


        public BETemporalBase()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            this.meshes = new MeshData[this.maxSlots];
        }

        public ItemSlot MiddleSlot => this.inventory[0];

        public ItemSlot TopSlot => this.inventory[1];

        public ItemStack MiddleStack
        {
            get => this.inventory[0].Itemstack;
            set => this.inventory[0].Itemstack = value;
        }

        public ItemStack TopStack
        {
            get => this.inventory[1].Itemstack;
            set => this.inventory[1].Itemstack = value;
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side.IsServer())
            { this.RegisterGameTickListener(this.TemporalUpdate, this.tickSeconds * 1000); }
        }


        private void GenerateTerrorParticles(BlockPos pos, IWorldAccessor world, int color, int gearcount, float gravityEffect)
        {
            if (gearcount > 0)
            {
                float minQuantity = 1;
                float maxQuantity = gearcount * gearcount * 5;
                //color = ColorUtil.ToRgba(255, 205, 10, 10);
                var minPos = new Vec3d();
                var addPos = new Vec3d();
                var minVelocity = new Vec3f(0.1f, 0.0f, 0.1f);
                var maxVelocity = new Vec3f(0.5f, 0.5f, 0.5f);
                float lifeLength = 2 * gearcount;
                //float gravityEffect = -0.01f;
                var minSize = 0.01f;
                var maxSize = 0.5f;

                var terrorparticles = new SimpleParticleProperties(
                    minQuantity, maxQuantity,
                    color,
                    minPos, addPos,
                    minVelocity, maxVelocity,
                    lifeLength,
                    gravityEffect,
                    minSize, maxSize,
                    EnumParticleModel.Cube
                );
                terrorparticles.MinPos.Set(pos.ToVec3d().AddCopy(-(gearcount - 1), 0f, -(gearcount - 1)));
                terrorparticles.AddPos.Set(new Vec3d(gearcount + gearcount - 1, 0.2f, gearcount + gearcount - 1));
                terrorparticles.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 2);
                terrorparticles.OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -255);
                terrorparticles.ShouldDieInAir = false;
                terrorparticles.SelfPropelled = false;
                //terrorparticles.GlowLevel = 128;
                world.SpawnParticles(terrorparticles);
            }
        }


        public int GearCount(string type)
        {
            var count = 0;
            for (var i = 2; i <= 5; i++)
            {
                if (!this.inventory[i].Empty) //a gear
                {
                    if (this.inventory[i].Itemstack.Item.FirstCodePart(1).Contains(type))
                    { count++; }
                }
            }
            return count;
        }


        public bool SurroundingAreaOK(BlockPos pos, string type)
        {
            var areaOK = true;
            Block testBlock;
            var downpos = pos.DownCopy();
            var neibPos = new BlockPos[] { downpos.NorthCopy(), downpos.SouthCopy(), downpos.EastCopy(), downpos.WestCopy(), downpos.NorthCopy().EastCopy(), downpos.SouthCopy().WestCopy(), downpos.SouthCopy().EastCopy(), downpos.NorthCopy().WestCopy() };
            foreach (var neib in neibPos)
            {
                testBlock = this.Api.World.BlockAccessor.GetBlock(neib);
                if (type == "water")
                {
                    if (testBlock.LiquidCode != "water")
                    { areaOK = false; }
                }
                else //fertile ground
                {
                    if (testBlock.Fertility <= 0)
                    { areaOK = false; }
                }
            }
            return areaOK;
        }


        public void TemporalUpdate(float par)
        {
            var toptype = "";
            var gearcount = 0;
            string[] temporalTypes = { "game:vegetable-cabbage", "game:vegetable-carrot", "game:vegetable-onion", "game:vegetable-parsnip", "game:vegetable-turnip", "game:vegetable-pumpkin" };
            string[] astralTypes = { "primitivesurvival:psfish-trout-raw", "primitivesurvival:psfish-perch-raw", "primitivesurvival:psfish-carp-raw", "primitivesurvival:psfish-bass-raw", "primitivesurvival:psfish-pike-raw", "primitivesurvival:psfish-arcticchar-raw", "primitivesurvival:psfish-catfish-raw", "primitivesurvival:psfish-bluegill-raw", "primitivesurvival:psfish-mutant-raw", "primitivesurvival:psfish-mutant-raw" };

            if (this.dropsGold)
            {
                for (var i = 1; i < 4; i++)
                {
                    Array.Resize(ref temporalTypes, temporalTypes.Length + 1);
                    temporalTypes[temporalTypes.Length - 1] = "game:nugget-nativegold";
                    Array.Resize(ref astralTypes, astralTypes.Length + 1);
                    astralTypes[astralTypes.Length - 1] = "game:nugget-nativegold";
                }
            }

            if (!this.inventory[1].Empty)
            {
                toptype = this.TopStack.Block.FirstCodePart(1);
                if (toptype == "statue")
                { toptype = this.TopStack.Block.FirstCodePart(); }
            }

            if (toptype == "dagon" || toptype == "hydra")
            {
                var areaOK = this.SurroundingAreaOK(this.Pos, "water");
                if (areaOK)
                {
                    gearcount = this.GearCount("astral");
                    this.GenerateTerrorParticles(this.Pos, this.Api.World, ColorUtil.ToRgba(80, 30, 30, 30), gearcount, 1.0f);
                    var entity = this.Api.World.GetNearestEntity(this.Pos.ToVec3d(), 1 + gearcount, 1 + gearcount, null);
                    if (entity != null)
                    {
                        var dmg = Rnd.Next(3) + 1;
                        var damaged = entity.ReceiveDamage(new DamageSource()
                        {
                            Source = EnumDamageSource.Void,
                            Type = EnumDamageType.PiercingAttack
                        }, dmg);
                        if (damaged) //drop astralType
                        {
                            var dropType = astralTypes[Rnd.Next(astralTypes.Count())];
                            var dropCount = 1;
                            if (dropType == "game:nugget-nativegold")
                            { dropCount = Rnd.Next(5) + 1; }
                            var newStack = new ItemStack(this.Api.World.GetItem(new AssetLocation(dropType)), dropCount);
                            this.Api.World.SpawnItemEntity(newStack, this.Pos.ToVec3d().Add(0.5, 10, 0.5));
                        }
                    }
                }
            }
            else if (toptype == "cthulu")
            {
                var areaOK = this.SurroundingAreaOK(this.Pos, "ground");
                if (areaOK)
                {
                    gearcount = this.GearCount("temporal");
                    this.GenerateTerrorParticles(this.Pos, this.Api.World, ColorUtil.ToRgba(80, 50, 120, 50), gearcount, 0.5f);
                    var entity = this.Api.World.GetNearestEntity(this.Pos.ToVec3d(), 1 + gearcount, 1 + gearcount, null);
                    if (entity != null)
                    {
                        var dmg = Rnd.Next(3) + 1;
                        var damaged = entity.ReceiveDamage(new DamageSource()
                        {
                            Source = EnumDamageSource.Void,
                            Type = EnumDamageType.PiercingAttack
                        }, dmg);
                        if (damaged) //drop temporalType
                        {
                            var dropType = temporalTypes[Rnd.Next(temporalTypes.Count())];
                            var dropCount = 1;
                            if (dropType == "game:nugget-nativegold")
                            { dropCount = Rnd.Next(5) + 1; }
                            var newStack = new ItemStack(this.Api.World.GetItem(new AssetLocation(dropType)), dropCount);
                            this.Api.World.SpawnItemEntity(newStack, this.Pos.ToVec3d().Add(0.5, 10, 0.5));
                        }
                    }
                }
            }
        }


        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (playerSlot.Empty)
            {
                if (this.TryTake(byPlayer, blockSel))
                { return true; }
                return false;
            }
            else
            {
                if (this.TryPut(byPlayer))
                { return true; }
                return false;
            }
        }


        internal void OnBreak(IPlayer byPlayer, BlockPos pos)
        {
            string tmpPath;
            string lastPart;
            for (var index = this.maxSlots - 1; index >= 0; index--)
            {
                if (!this.inventory[index].Empty)
                {
                    if (index <= 1)
                    {
                        tmpPath = this.inventory[index].Itemstack.Collectible.Code.Path;
                        lastPart = this.inventory[index].Itemstack.Collectible.LastCodePart();
                        tmpPath = tmpPath.Replace(lastPart, "north");
                        var tmpBlock = this.Api.World.GetBlock(this.Block.CodeWithPath(tmpPath));
                        this.inventory[index].Itemstack = new ItemStack(tmpBlock);
                    }
                    var stack = this.inventory[index].TakeOut(1);
                    this.Api.World.SpawnItemEntity(stack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }
                this.MarkDirty(true);
            }
        }


        private bool TryPut(IPlayer byPlayer)
        {
            var index = -1;
            var bookorient = "north";
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            var playerStack = playerSlot.Itemstack;
            if (this.inventory != null)
            {
                var stacks = this.inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
                if (stacks.Count() >= this.maxSlots)
                { return false; }
            }

            if (playerStack.Block != null)
            {
                if (playerStack.Block.Attributes != null)
                {
                    if (playerStack.Block.Attributes.Exists == true)
                    {
                        if (playerStack.Block.Attributes["placement"].Exists)
                        {
                            var placement = playerStack.Block.Attributes["placement"].ToString();
                            var placetype = playerStack.Block.Attributes["placetype"].ToString();
                            if (placement == "middle" && this.MiddleSlot.Empty)
                            { index = 0; }
                            else if (placement == "top" && this.TopSlot.Empty && !this.MiddleSlot.Empty)
                            {
                                var middletype = this.MiddleStack.Block.Attributes["placetype"].ToString();
                                if (placetype == "statue" && middletype == "cube")
                                { index = 1; }
                                else if (placetype == "book" && middletype == "lectern")
                                {
                                    index = 1;
                                    bookorient = this.MiddleStack.Block.LastCodePart();
                                }
                            }
                        }
                    }
                }
            }
            else if (playerStack.Item != null)
            {
                var path = playerStack.Item.Code.Path;
                if (path.Contains("gear-") && !this.MiddleSlot.Empty)
                {
                    var facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                    var playerFacing = facing.ToString();
                    var middletype = this.MiddleStack.Block.Attributes["placetype"].ToString();
                    if (middletype == "lectern")
                    {
                        var tmpPath = this.MiddleStack.Collectible.Code.Path;
                        if (!tmpPath.Contains(playerFacing))
                        { return false; }  //not facing the one available slot
                    }

                    if (playerFacing == "north")
                    { index = 2; }
                    else if (playerFacing == "east")
                    { index = 3; }
                    else if (playerFacing == "south")
                    { index = 4; }
                    else if (playerFacing == "west")
                    { index = 5; }
                }
            }
            if (index >= 0)
            {
                if (this.inventory[index].Empty)
                {
                    var moved = playerSlot.TryPutInto(this.Api.World, this.inventory[index]);
                    if (moved > 0)
                    {
                        if (index == 0 || index == 1) //middle or top
                        {
                            //try orienting it directly in the inventory
                            var facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                            var playerFacing = facing.ToString();
                            if (playerFacing != "north" && playerFacing != "south" && playerFacing != "east" && playerFacing != "west")
                            { playerFacing = "north"; }
                            var tmpPath = this.inventory[index].Itemstack.Collectible.Code.Path;
                            if (tmpPath.Contains("necronomicon"))
                            { tmpPath = tmpPath.Replace("north", bookorient); }
                            else
                            { tmpPath = tmpPath.Replace("north", playerFacing); }
                            var tmpBlock = this.Api.World.GetBlock(this.Block.CodeWithPath(tmpPath));
                            this.inventory[index].Itemstack = new ItemStack(tmpBlock);
                        }
                        this.MarkDirty(true);
                        return moved > 0;
                    }
                }
            }
            return false;
        }


        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            var facing = byPlayer.CurrentBlockSelection.Face.Opposite;
            var index = -1;
            var playerFacing = facing.ToString();

            if (playerFacing == "north")
            { index = 2; }
            else if (playerFacing == "east")
            { index = 3; }
            else if (playerFacing == "south")
            { index = 4; }
            else if (playerFacing == "west")
            { index = 5; }

            if (index >= 0)
            {
                if (!this.inventory[index].Empty)
                {
                    byPlayer.InventoryManager.TryGiveItemstack(this.inventory[index].Itemstack);
                    this.inventory[index].TakeOutWhole();
                    this.MarkDirty(true);
                    return true;
                }
            }

            index = -1;
            var hasGear = false;
            for (var i = 2; i < this.maxSlots; i++)
            {
                if (!this.inventory[i].Empty)
                { hasGear = true; }
            }

            if (!this.TopSlot.Empty)
            { index = 1; }
            else if (!this.MiddleSlot.Empty && !hasGear)
            { index = 0; }
            if (index >= 0)
            {
                var tmpPath = this.inventory[index].Itemstack.Collectible.Code.Path;
                var lastPart = this.inventory[index].Itemstack.Collectible.LastCodePart();
                tmpPath = tmpPath.Replace(lastPart, "north");
                var tmpBlock = this.Api.World.GetBlock(this.Block.CodeWithPath(tmpPath));
                this.inventory[index].Itemstack = new ItemStack(tmpBlock);

                byPlayer.InventoryManager.TryGiveItemstack(this.inventory[index].Itemstack);
                this.inventory[index].TakeOutWhole();
                this.MarkDirty(true);
                return true;
            }

            return false;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            var gearCount = 0;
            var gearType = "";
            for (var i = 2; i < this.maxSlots; i++)
            {
                if (!this.inventory[i].Empty)
                {
                    gearType = this.inventory[i].Itemstack.Collectible.Code.Path;
                    gearCount++;
                }
            }

            if (!this.TopSlot.Empty && !this.MiddleSlot.Empty)
            {
                var middletype = this.MiddleStack.Block.Attributes["placetype"].ToString();
                if (middletype == "lectern" && gearCount == 1)
                {
                    if (gearType == "psgear-astral")
                    { sb.Append(Lang.Get("The book tells of the Mother Hydra and the Father Dagon.  They're awakened by some sort of cosmic energy and rejuvenated by water. Only then can you feed them and be requited.")); }
                    else if (gearType == "gear-temporal")
                    { sb.Append(Lang.Get("The book tells of the Great Cthulhu.  It's awakened by some sort of transitory energy and feeds off the earth itself.  Once those needs are met you can feed it and be requited.")); }
                    else if (gearType == "psgear-ethereal")
                    { sb.Append(Lang.Get("The book tells of distant worlds.  The specifics are beyond your comprehension.")); }
                    else if (gearType == "gear-rusty")
                    { sb.Append(Lang.Get("It appears to be complete.")); }
                }
                else
                {
                    var gearcount = 0;
                    var toptype = this.TopStack.Block.FirstCodePart(1);
                    if (toptype == "statue")
                    { toptype = this.TopStack.Block.FirstCodePart(); }

                    var areaOK = false;
                    if (toptype == "dagon" || toptype == "hydra")
                    {
                        gearcount = this.GearCount("astral");
                        areaOK = this.SurroundingAreaOK(this.Pos, "water");
                    }
                    else if (toptype == "cthulu")
                    {
                        gearcount = this.GearCount("temporal");
                        areaOK = this.SurroundingAreaOK(this.Pos, "ground");
                    }

                    if (gearcount > 0 && areaOK)
                    {
                        var holesize = Lang.Get("fattened hen");
                        if (gearcount == 2)
                        { holesize = Lang.Get("wolf pack"); }
                        else if (gearcount == 3)
                        { holesize = Lang.Get("fully grown oak"); }
                        else if (gearcount == 4)
                        { holesize = Lang.Get("hovel"); }
                        sb.Append(Lang.Get("You've opened a gateway the size of a") + " " + holesize + " " + Lang.Get("and have unleashed some sort of cosmic horror!"));
                    }
                    else if (gearcount > 0)
                    { sb.Append(Lang.Get("It appears to be complete and you can feel a strange energy in the air, but the surrounding conditions aren't quite right.")); }
                    else
                    { sb.Append(Lang.Get("It appears to be complete.")); }
                }
            }
            else
            { sb.Append(Lang.Get("It looks like something is missing.")); }
            sb.AppendLine().AppendLine();
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            MeshData mesh;
            var shapeBase = "primitivesurvival:shapes/";
            var shapePath = "";
            var index = -1;
            var type = "";

            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos) as BlockTemporalBase;
            Block tmpBlock;
            var texture = tesselator.GetTexSource(block);
            var dir = block.LastCodePart();

            var newPath = "temporalbase";
            shapePath = "block/relic/" + newPath;
            mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
            mesher.AddMeshData(mesh);

            if (this.inventory != null)
            {
                if (!this.MiddleSlot.Empty)
                {
                    newPath = this.MiddleStack.Block.FirstCodePart();
                    dir = this.MiddleStack.Block.LastCodePart();
                    shapePath = "block/relic/" + newPath;
                    type = "";
                    if (newPath.Contains("lectern"))
                    { type = "lectern"; }
                    else if (newPath.Contains("cube"))
                    { type = "cube"; }
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
                    mesher.AddMeshData(mesh);
                }

                var enabled = false;
                dir = "";
                var tmptexture = texture;
                for (var i = 2; i < this.maxSlots; i++)
                {
                    if (!this.inventory[i].Empty) //gear - temporal or rusty
                    {
                        var gearType = this.inventory[i].Itemstack.Item.FirstCodePart(1);
                        tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                        if (gearType != "rusty")
                        {
                            gearType = "temporal";
                            enabled = true;
                        }
                        shapePath = "game:shapes/item/gear-" + gearType;
                        tmptexture = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                        mesh = block.GenMesh(this.Api as ICoreClientAPI, shapePath, tmptexture, i, type, dir, tesselator);
                        mesher.AddMeshData(mesh);
                    }
                }

                if (!this.TopSlot.Empty)
                {
                    shapePath = "block/relic/";
                    newPath = this.TopStack.Block.FirstCodePart();
                    dir = this.TopStack.Block.LastCodePart();
                    if (this.TopStack.Block.Code.Path.Contains("statue"))
                    { shapePath += "statue/"; }
                    shapePath += newPath;
                    if (newPath.Contains("necronomicon"))
                    {
                        if (enabled)
                        { shapePath += "-open"; }
                        else
                        { shapePath += "-closed"; }
                        tmpBlock = this.Api.World.GetBlock(block.CodeWithPath("necronomicon-north"));
                        texture = ((ICoreClientAPI)this.Api).Tesselator.GetTexSource(tmpBlock);
                    }
                    mesh = block.GenMesh(this.Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }
            return true;
        }
    }
}

