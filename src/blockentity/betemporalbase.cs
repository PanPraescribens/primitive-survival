using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using System.IO;
using System.Threading;

public class BETemporalBase : BlockEntityDisplay
{
    public int tickSeconds = 1;
    public int maxSlots = 6;
    public static Random rnd = new Random();
    public string[] temporalTypes = { "game:vegetable-cabbage", "game:vegetable-carrot", "game:vegetable-onion", "game:vegetable-parsnip", "game:vegetable-turnip", "game:vegetable-pumpkin", "game:nugget-nativegold", "game:nugget-nativegold", "game:nugget-nativegold" };
    public string[] astralTypes = { "primitivesurvival:psfish-trout-raw", "primitivesurvival:psfish-bass-raw", "primitivesurvival:psfish-pike-raw", "primitivesurvival:psfish-arcticchar-raw", "primitivesurvival:psfish-catfish-raw", "primitivesurvival:psfish-bluegill-raw", "primitivesurvival:psfish-mutant-raw", "primitivesurvival:psfish-mutant-raw", "game:nugget-nativegold", "game:nugget-nativegold", "game:nugget-nativegold", "game:nugget-nativegold", "game:nugget-nativegold" };

    public override string InventoryClassName
    {
        get { return "temporalbase"; }
    }
    protected InventoryGeneric inventory;

    public override InventoryBase Inventory
    {
        get { return inventory; }
    }

    public BETemporalBase()
    {
        inventory = new InventoryGeneric(maxSlots, null, null);
        meshes = new MeshData[maxSlots];
    }

    public ItemSlot middleSlot
    {
        get { return inventory[0]; }
    }

    public ItemSlot topSlot
    {
        get { return inventory[1]; }
    }

    public ItemStack middleStack
    {
        get { return inventory[0].Itemstack; }
        set { inventory[0].Itemstack = value; }
    }

    public ItemStack topStack
    {
        get { return inventory[1].Itemstack; }
        set { inventory[1].Itemstack = value; }
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side.IsServer())
        {
            RegisterGameTickListener(TemporalUpdate, tickSeconds * 1000);
        }
    }

    private void GenerateTerrorParticles(BlockPos pos, IWorldAccessor world, int color, int gearcount, float gravityEffect)
    {
        if (gearcount > 0)
        {
            float minQuantity = 1;
            float maxQuantity = gearcount * gearcount * 5;
            //color = ColorUtil.ToRgba(255, 205, 10, 10);
            Vec3d minPos = new Vec3d();
            Vec3d addPos = new Vec3d();
            Vec3f minVelocity = new Vec3f(0.1f, 0.0f, 0.1f);
            Vec3f maxVelocity = new Vec3f(0.5f, 0.5f, 0.5f);
            float lifeLength = 2 * gearcount;
            //float gravityEffect = -0.01f;
            float minSize = 0.01f;
            float maxSize = 0.5f;

            SimpleParticleProperties terrorparticles = new SimpleParticleProperties(
                minQuantity, maxQuantity,
                color,
                minPos, addPos,
                minVelocity, maxVelocity,
                lifeLength,
                gravityEffect,
                minSize, maxSize,
                EnumParticleModel.Cube
            );
            terrorparticles.MinPos.Set( pos.ToVec3d().AddCopy( -(gearcount-1), 0f, -(gearcount-1) ) );
            terrorparticles.AddPos.Set( new Vec3d(gearcount+gearcount-1, 0.2f, gearcount+gearcount-1) );
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
        int count = 0;
        for (int i = 2; i <= 5; i++)
        {
            if (!inventory[i].Empty) //a gear
            {
                if (inventory[i].Itemstack.Item.FirstCodePart(1).Contains(type))
                    count++;
            }
        }
        return count;
    }

    public bool surroundingAreaOK(BlockPos pos, string type)
    {
        bool areaOK = true;
        Block testBlock;
        BlockPos downpos = pos.DownCopy();
        BlockPos[] neibPos = new BlockPos[] { downpos.NorthCopy(), downpos.SouthCopy(), downpos.EastCopy(), downpos.WestCopy(), downpos.NorthCopy().EastCopy(), downpos.SouthCopy().WestCopy(), downpos.SouthCopy().EastCopy(), downpos.NorthCopy().WestCopy() };
        foreach (BlockPos neib in neibPos)
        {
            testBlock = Api.World.BlockAccessor.GetBlock(neib);
            if (type == "water")
            {
                if (testBlock.LiquidCode != "water")
                { areaOK = false; }
            }
            else //fertile ground
                if (testBlock.Fertility <= 0)
                { areaOK = false; }
        }
        return areaOK;
    }

    public void TemporalUpdate(float par)
    {
        string toptype = "";
        int gearcount = 0;
        if (!inventory[1].Empty)
        {
            toptype = topStack.Block.FirstCodePart(1);
            if (toptype == "statue")
                toptype = topStack.Block.FirstCodePart();
        }

        if (toptype == "dagon" || toptype == "hydra")
        {
            bool areaOK = surroundingAreaOK(Pos, "water");
            if (areaOK)
            {
                gearcount = GearCount("astral");
                GenerateTerrorParticles(Pos, Api.World, ColorUtil.ToRgba(80, 30, 30, 30), gearcount, 1.0f);
                Entity entity = Api.World.GetNearestEntity(Pos.ToVec3d(), 1 + gearcount, 1 + gearcount, null);
                if (entity != null)
                {
                    int dmg = rnd.Next(3) + 1;
                    bool damaged = entity.ReceiveDamage(new DamageSource()
                    {
                        Source = EnumDamageSource.Void,
                        Type = EnumDamageType.PiercingAttack
                    }, dmg);
                    if (damaged)
                    {
                        //drop astralType
                        string dropType = astralTypes[rnd.Next(astralTypes.Count())];
                        int dropCount = 1;
                        if (dropType == "game:nugget-nativegold")
                            dropCount = rnd.Next(5) + 1;
                        ItemStack newStack = new ItemStack(Api.World.GetItem(new AssetLocation(dropType)), dropCount);
                        Api.World.SpawnItemEntity(newStack, Pos.ToVec3d().Add(0.5, 10, 0.5)); 
                    }
                }
            }
        }
        else if (toptype == "cthulu")
        {
            bool areaOK = surroundingAreaOK(Pos, "ground");
            if (areaOK)
            {
                gearcount = GearCount("temporal");
                GenerateTerrorParticles(Pos, Api.World, ColorUtil.ToRgba(80, 50, 120, 50), gearcount, 0.5f);
                Entity entity = Api.World.GetNearestEntity(Pos.ToVec3d(), 1+gearcount, 1+gearcount, null);
                if (entity != null)
                {
                    int dmg = rnd.Next(3)+1;
                    bool damaged = entity.ReceiveDamage(new DamageSource()
                    {
                        Source = EnumDamageSource.Void,
                        Type = EnumDamageType.PiercingAttack
                    }, dmg);
                    if (damaged)
                    {
                        //drop temporalType
                        string dropType = temporalTypes[rnd.Next(temporalTypes.Count())];
                        int dropCount = 1;
                        if (dropType == "game:nugget-nativegold")
                            dropCount = rnd.Next(5) + 1;
                        ItemStack newStack = new ItemStack(Api.World.GetItem(new AssetLocation(dropType)), dropCount);
                        Api.World.SpawnItemEntity(newStack, Pos.ToVec3d().Add(0.5, 10, 0.5));
                    }
                }
            }
        }
    }


    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (playerSlot.Empty)
        {
            if (TryTake(byPlayer, blockSel))
            { return true; }
            return false;
        }
        else
        {
            if (TryPut(byPlayer, blockSel))
            { return true; }
            return false;
        }
    }


    internal void OnBreak(IPlayer byPlayer, BlockPos pos)
    {
        string tmpPath;
        string lastPart;
        for (int index = maxSlots - 1; index >= 0; index--)
        {
            if (!inventory[index].Empty)
            {
                if (index <= 1)
                {
                    tmpPath = inventory[index].Itemstack.Collectible.Code.Path;
                    lastPart = inventory[index].Itemstack.Collectible.LastCodePart();
                    tmpPath = tmpPath.Replace(lastPart, "north");
                    Block tmpBlock = Api.World.GetBlock(Block.CodeWithPath(tmpPath));
                    inventory[index].Itemstack = new ItemStack(tmpBlock);
                }
                ItemStack stack = inventory[index].TakeOut(1);
                Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
            MarkDirty(true);
        }
    }


    private bool TryPut(IPlayer byPlayer, BlockSelection blockSel)
    {
        int index = -1;
        string bookorient = "north";
        ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        ItemStack playerStack = playerSlot.Itemstack;
        if (inventory != null)
        {
            ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
            if (stacks.Count() >= maxSlots)
            { return false; }
        }

        if (playerStack.Block != null)
        {
            if (playerStack.Block.Attributes.Exists == true)
            {
                if (playerStack.Block.Attributes["placement"].Exists == true)
                {
                    string placement = playerStack.Block.Attributes["placement"].ToString();
                    string placetype = playerStack.Block.Attributes["placetype"].ToString();
                    if (placement == "middle" && middleSlot.Empty)
                    { index = 0; }
                    else if (placement == "top" && topSlot.Empty && !middleSlot.Empty)
                    {
                        string middletype = middleStack.Block.Attributes["placetype"].ToString();
                        if (placetype == "statue" && middletype == "cube")
                            index = 1;
                        else if (placetype == "book" && middletype == "lectern")
                        {
                            index = 1;
                            bookorient = middleStack.Block.LastCodePart();
                        }
                    }
                }
            }
        }
        else if (playerStack.Item != null)
        {
            //System.Diagnostics.Debug.WriteLine("Putting item: " + playerStack.Item.Code.Path);
            string path = playerStack.Item.Code.Path;
            if (path.Contains("gear-") && !middleSlot.Empty)
            {
                BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                string playerFacing = facing.ToString();

                //see what's in the middle slot, cube or lectern
                string middletype = middleStack.Block.Attributes["placetype"].ToString();
                if (middletype == "lectern")
                {
                    string tmpPath = middleStack.Collectible.Code.Path;
                    if (!tmpPath.Contains(playerFacing))
                        return false;  //not facing the one available slot
                }

                if (playerFacing == "north")
                    index = 2;
                else if (playerFacing == "east")
                    index = 3;
                else if (playerFacing == "south")
                    index = 4;
                else if (playerFacing == "west")
                    index = 5;
            }
        }
        if (index >= 0)
        {
            if (inventory[index].Empty)
            {
                int moved = playerSlot.TryPutInto(Api.World, inventory[index]);
                if (moved > 0)
                {

                    if (index == 0 || index == 1) //middle or top
                    {
                        //try orienting it directly in the inventory
                        BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
                        string playerFacing = facing.ToString();
                        if (playerFacing != "north" && playerFacing != "south" && playerFacing != "east" && playerFacing != "west")
                            playerFacing = "north";
                        string tmpPath = inventory[index].Itemstack.Collectible.Code.Path;
                        if (tmpPath.Contains("necronomicon"))
                            tmpPath = tmpPath.Replace("north", bookorient);
                        else
                            tmpPath = tmpPath.Replace("north", playerFacing);
                        Block tmpBlock = Api.World.GetBlock(Block.CodeWithPath(tmpPath));
                        inventory[index].Itemstack = new ItemStack(tmpBlock);
                    }
                    MarkDirty(true);
                    return moved > 0;
                }
            }
        }
        return false;
    }


    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        BlockFacing facing = byPlayer.CurrentBlockSelection.Face.Opposite;
        int index = -1;
        //System.Diagnostics.Debug.WriteLine(facing.ToString());
        string playerFacing = facing.ToString();

        if (playerFacing == "north")
            index = 2;
        else if (playerFacing == "east")
            index = 3;
        else if (playerFacing == "south")
            index = 4;
        else if (playerFacing == "west")
            index = 5;

        if (index >= 0)
        {
            if (!inventory[index].Empty)
            {
                byPlayer.InventoryManager.TryGiveItemstack(inventory[index].Itemstack);
                inventory[index].TakeOutWhole();
                MarkDirty(true);
                return true;
            }
        }

        index = -1;
        bool hasGear = false;
        for (int i = 2; i < maxSlots; i++)
        { if (!inventory[i].Empty) hasGear = true; }

        if (!topSlot.Empty)
        { index = 1; }
        else if (!middleSlot.Empty && !hasGear)
        { index = 0; }
        if (index >= 0)
        {
            string tmpPath = inventory[index].Itemstack.Collectible.Code.Path;
            string lastPart = inventory[index].Itemstack.Collectible.LastCodePart();
            tmpPath = tmpPath.Replace(lastPart, "north");
            Block tmpBlock = Api.World.GetBlock(Block.CodeWithPath(tmpPath));
            inventory[index].Itemstack = new ItemStack(tmpBlock);

            byPlayer.InventoryManager.TryGiveItemstack(inventory[index].Itemstack);
            inventory[index].TakeOutWhole();
            MarkDirty(true);
            return true;
        }

        return false;
    }


    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        int gearCount = 0;
        string gearType = "";
        for (int i = 2; i < maxSlots; i++)
        {
            if (!inventory[i].Empty)
            {
                gearType = inventory[i].Itemstack.Collectible.Code.Path;
                gearCount++;
            }
        }

        if (!topSlot.Empty && !middleSlot.Empty)
        {
            string middletype = middleStack.Block.Attributes["placetype"].ToString();
            if (middletype == "lectern" && gearCount == 1)
            {
                if (gearType == "psgear-astral")
                    sb.Append("The book tells of the Mother Hydra and the Father Dagon.  They're awakened by some sort of cosmic energy and rejuvenated by water. Only then can you feed them and be requited.");
                else if (gearType == "gear-temporal")
                    sb.Append("The book tells of the Great Cthulhu.  It's awakened by some sort of transitory energy and feeds off the earth itself.  Once those needs are met you can feed it and be requited.");
                else if (gearType == "psgear-ethereal")
                    sb.Append("The book tells of distant worlds.  The specifics are beyond your comprehension.");
                else if (gearType == "gear-rusty")
                    sb.Append("It appears to be complete.");
            }
            else 
            {
                int gearcount = 0;
                string toptype = topStack.Block.FirstCodePart(1);
                if (toptype == "statue")
                    toptype = topStack.Block.FirstCodePart();

                bool areaOK = false;
                if (toptype == "dagon" || toptype == "hydra")
                {
                    gearcount = GearCount("astral");
                    areaOK = surroundingAreaOK(Pos, "water");
                }
                else if (toptype == "cthulu")
                {
                    gearcount = GearCount("temporal");
                    areaOK = surroundingAreaOK(Pos, "ground");
                }
                if (gearcount > 0 && areaOK)
                {
                    string holesize = "fattened hen";
                    if (gearcount == 2)
                        holesize = "wolf pack";
                    else if (gearcount == 3)
                        holesize = "fully grown oak";
                    else if (gearcount == 4)
                        holesize = "hovel";
                    sb.Append("You've opened a gateway the size of a " + holesize + " and have unleashed some sort of cosmic horror!");
                }
                else if (gearcount > 0)
                {
                    sb.Append("It appears to be complete and you can feel a strange energy in the air, but the surrounding conditions aren't quite right.");
                }
                else
                    sb.Append("It appears to be complete.");
            }
        }
        else
            sb.Append("It looks like something is missing.");
        sb.AppendLine().AppendLine();
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        MeshData mesh;
        string shapeBase = "primitivesurvival:shapes/";
        string shapePath = "";
        int index = -1;
        string type = "";

        BlockTemporalBase block = Api.World.BlockAccessor.GetBlock(Pos) as BlockTemporalBase;
        Block tmpBlock;
        ITexPositionSource texture = tesselator.GetTexSource(block);
        string dir = block.LastCodePart();

        string newPath = "temporalbase";
        shapePath = "block/relic/" + newPath;
        mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
        mesher.AddMeshData(mesh);

        if (inventory != null)
        {
            if (!middleSlot.Empty)
            {
                newPath = middleStack.Block.FirstCodePart();
                dir = middleStack.Block.LastCodePart();
                shapePath = "block/relic/" + newPath;
                type = "";
                if (newPath.Contains("lectern"))
                    type = "lectern";
                else if (newPath.Contains("cube"))
                    type = "cube";
                //System.Diagnostics.Debug.WriteLine("middle slot:" + shapePath);
                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
                mesher.AddMeshData(mesh);

            }

            bool enabled = false;
            dir = "";
            ITexPositionSource tmptexture = texture;
            for (int i = 2; i < maxSlots; i++)
            {
                if (!inventory[i].Empty) //gear - temporal or rusty
                {
                    string gearType = inventory[i].Itemstack.Item.FirstCodePart(1);
                    tmpBlock = Api.World.GetBlock(block.CodeWithPath("texture" + gearType));
                    if (gearType != "rusty")
                    {
                        gearType = "temporal";
                        enabled = true;
                    }
                    shapePath = "game:shapes/item/gear-" + gearType;
                    tmptexture = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                    mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, tmptexture, i, type, dir, tesselator);
                    mesher.AddMeshData(mesh);
                }
            }

            if (!topSlot.Empty)
            {
                shapePath = "block/relic/";
                newPath = topStack.Block.FirstCodePart();
                dir = topStack.Block.LastCodePart();
                if (topStack.Block.Code.Path.Contains("statue"))
                    shapePath += "statue/";
                shapePath += newPath;
                if (newPath.Contains("necronomicon"))
                {
                    if (enabled)
                        shapePath += "-open";
                    else
                        shapePath += "-closed";
                    tmpBlock = Api.World.GetBlock(block.CodeWithPath("necronomicon-north"));
                    texture = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                }

                mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, index, type, dir, tesselator);
                mesher.AddMeshData(mesh);
            }
        }
        return true;
    }
}


