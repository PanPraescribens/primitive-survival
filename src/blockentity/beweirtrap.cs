using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

public class BEWeirTrap : BlockEntityDisplay 
{
    public int catchPercent = 4; //5  
    public int escapePercent = 10; //10
    public double updateMinutes = 2.6; //2.6 
    public int rotRemovedPercent = 10; //10

    public int tickSeconds = 3;
    public int maxSlots = 2;
    public string[] fishTypes = { "trout", "perch", "carp", "bass", "pike", "arcticchar", "catfish", "bluegill" };
    public string[] shellStates = { "scallop", "sundial", "turritella", "clam", "conch", "seastar", "volute" };
    public string[] shellColors = { "latte", "plain", "seafoam", "darkpurple", "cinnamon", "turquoise" };
    public string[] relics = { "temporalbase", "temporalcube", "temporallectern", "cthulu-statue", "dagon-statue", "hydra-statue", "necronomicon" };
    public static Random rnd = new Random();


    public override string InventoryClassName
    {
        get { return "weirtrap"; }
    }
    protected InventoryGeneric inventory;

    public override InventoryBase Inventory
    {
        get { return inventory; }
    }


    public BEWeirTrap()
    {
        inventory = new InventoryGeneric(maxSlots, null, null);
        meshes = new MeshData[maxSlots];
    }

    public ItemSlot catch1Slot
    {
        get { return inventory[0]; }
    }

    public ItemSlot catch2Slot
    {
        get { return inventory[1]; }
    }


    public ItemStack catch1Stack
    {
        get { return inventory[0].Itemstack; }
        set { inventory[0].Itemstack = value; }
    }

    public ItemStack catch2Stack
    {
        get { return inventory[1].Itemstack; }
        set { inventory[1].Itemstack = value; }
    }


    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side.IsServer())
        {
            RegisterGameTickListener(ParticleUpdate, tickSeconds * 1000);
            RegisterGameTickListener(WeirTrapUpdate, (int)(updateMinutes * 60000));
        }
    }


    private void GenerateWaterParticles(int slot, string type, BlockPos pos, IWorldAccessor world)
    {
        BlockWeirTrap block = Api.World.BlockAccessor.GetBlock(Pos) as BlockWeirTrap;
        string dir = block.LastCodePart();
        float minQuantity = 1;
        float maxQuantity = 8;
        if (type == "seashell")
            maxQuantity = 2;
        int color = ColorUtil.ToRgba(40, 125, 185, 255);
        Vec3d minPos = new Vec3d();
        Vec3d addPos = new Vec3d();
        Vec3f minVelocity = new Vec3f(0.2f, 0.2f, 0.2f);
        Vec3f maxVelocity = new Vec3f(0.6f, 0.6f, 0.6f);
        float lifeLength = 5f;
        float gravityEffect = -0.1f;
        float minSize = 0.1f;
        float maxSize = 0.7f;

        SimpleParticleProperties waterParticles = new SimpleParticleProperties(
            minQuantity, maxQuantity,
            color,
            minPos, addPos,
            minVelocity, maxVelocity,
            lifeLength,
            gravityEffect,
            minSize, maxSize,
            EnumParticleModel.Quad
        );
        if (slot == 0)
        {

            if (type == "fish")
            {
                Vec3f min;
                if (dir == "north") min = new Vec3f(1.3f, 0.2f, 1.3f);
                else if (dir == "south") min = new Vec3f(-0.3f, 0.2f, -0.3f);
                else if (dir == "east") min = new Vec3f(-0.3f, 0.2f, 1.3f);
                else min = new Vec3f(1.3f, 0.2f, -0.3f);
                waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(min));
                waterParticles.AddPos.Set(new Vec3d(0.1, 0, 0));
            }
            else
            {
                Vec3f min;
                if (dir == "north") min = new Vec3f(1f, 0.2f, 0.7f);
                else if (dir == "south") min = new Vec3f(0f, 0.2f, 0.3f);
                else if (dir == "east") min = new Vec3f(0.3f, 0.2f, 1f);
                else min = new Vec3f(0.7f, 0.2f, 0f);
                waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(min));
            }
        }
        else
        {
            if (type == "fish")
            {
                Vec3f min;
                if (dir == "north") min = new Vec3f(-0.3f, 0.2f, 1.3f);
                else if (dir == "south") min = new Vec3f(1.3f, 0.2f, -0.3f);
                else if (dir == "east") min = new Vec3f(-0.3f, 0.2f, -0.3f);
                else min = new Vec3f(1.3f, 0.2f, 1.3f);
                waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(min));
                waterParticles.AddPos.Set(new Vec3d(0.1, 0, 0));
            }
            else
            {
                Vec3f min;
                if (dir == "north") min = new Vec3f(0f, 0.2f, 0.4f);
                else if (dir == "south") min = new Vec3f(1f, 0.2f, 0.6f);
                else if (dir == "east") min = new Vec3f(0.6f, 0.2f, 0f);
                else min = new Vec3f(0.4f, 0.2f, 1f);
                waterParticles.MinPos.Set(pos.ToVec3d().AddCopy(min));
            }
        }
        waterParticles.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARINCREASE, 0.7f);
        waterParticles.ShouldDieInAir = true;
        waterParticles.SelfPropelled = true;
        world.SpawnParticles(waterParticles);
    }


    public void ParticleUpdate(float par)
    {
        int slot = rnd.Next(3);
        if (slot <= 1)
        {
            if (!inventory[slot].Empty)
            {
                if (inventory[slot].Itemstack.Item != null) //fish
                    GenerateWaterParticles(slot, "fish", Pos, Api.World);
                else if (inventory[slot].Itemstack.Block != null) //seashell
                    GenerateWaterParticles(slot, "seashell", Pos, Api.World);
            }
        }
    }


    private void WeirTrapUpdate(float par)
    {
        int escaped = rnd.Next(100);
        int caught = rnd.Next(100);
        if (caught < catchPercent)
            WorldPut(0, Pos);
        else if (!catch1Slot.Empty || !catch2Slot.Empty)
        {
            if (escaped < escapePercent)
                WorldTake(1, Pos);
        }
        else
        {
            caught = rnd.Next(100);
            if (caught < catchPercent)
                WorldPut(1, Pos);
            else
            {
                if (escaped < escapePercent)
                    WorldTake(0, Pos);
            }
        }

        if (!catch1Slot.Empty)
        {
            if (catch1Stack.Item != null)
            {
                if (catch1Stack.Item.Code.Path == "rot")
                {
                    escaped = rnd.Next(100);
                    if (escaped < rotRemovedPercent)
                    { WorldTake(0, Pos); } //remove rot from slot 0
                }
            }
        }
        if (!catch2Slot.Empty)
        {
            if (catch2Stack.Item != null)
            {
                if (catch2Stack.Item.Code.Path == "rot")
                {
                    escaped = rnd.Next(100);
                    if (escaped < rotRemovedPercent)
                    { WorldTake(1, Pos); }  //remove rot from slot 1
                }
            }
        }
    }


    public bool WorldPut(int slot, BlockPos pos)
    {
        ItemStack newStack = null;
        if (slot == 0 || slot == 1)
        {
            int rando = rnd.Next(10);
            if (rando < 1) //10% chance of a seashell (or relic) 
            {
                rando = rnd.Next(10);
                if (rando < 1) //10% chance of a relic
                {
                    string thisRelic = relics[rnd.Next(relics.Count())];
                    newStack = new ItemStack(Api.World.GetBlock(new AssetLocation("primitivesurvival:" + thisRelic + "-North")), 1);
                }
                else
                    newStack = new ItemStack(Api.World.GetBlock(new AssetLocation("game:seashell-" + shellStates[rnd.Next(shellStates.Count())] + "-" + shellColors[rnd.Next(shellColors.Count())])), 1);
            }
            else
            {
                newStack = new ItemStack(Api.World.GetItem(new AssetLocation("primitivesurvival:psfish-" + fishTypes[rnd.Next(fishTypes.Count())] + "-raw")), 1);
            }
        }
        if (newStack == null) return false;
        if (inventory[slot].Empty)
        {
            inventory[slot].Itemstack = newStack;
            //Api.World.BlockAccessor.MarkBlockDirty(pos);
            MarkDirty(true);
            return true;
        }
        return false;
    }


    public bool WorldTake(int slot, BlockPos pos)
    {
        if (!inventory[slot].Empty)
        {
            inventory[slot].TakeOutWhole();
            Api.World.BlockAccessor.MarkBlockDirty(pos);
            MarkDirty();
            return true;
        }
        return false;
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
            CollectibleObject colObj = playerSlot.Itemstack.Collectible;
            if (colObj.Attributes != null)
            {
                if (TryPut(playerSlot, blockSel))
                { return true; }
                return false;
            }
        }
        return false;
    }


    private bool TryPut(ItemSlot playerSlot, BlockSelection blockSel)
    {
        int index = -1;
        int moved = 0;
        ItemStack playerStack = playerSlot.Itemstack;
        if (inventory != null)
        {
            ItemStack[] stacks = inventory.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
            if (stacks.Count() >= maxSlots)
            { return false; }
        }

        if (playerStack.Item != null)
        {
            if (playerStack.Item.Code.Path.Contains("psfish"))
            {
                if (catch1Slot.Empty) index = 0;
                else if (catch2Slot.Empty) index = 1;
            }
        }
        else if (playerStack.Block != null)
        {
            if (playerStack.Block.Code.Path.Contains("seashell"))
            {
                if (catch1Slot.Empty) index = 0;
                else if (catch2Slot.Empty) index = 1;
            }
        }

        if (index > -1)
        {
            moved = playerSlot.TryPutInto(Api.World, inventory[index]);
            if (moved > 0)
            {
                MarkDirty(true);
                return moved > 0;
            }
        }
        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        if (!catch2Slot.Empty)
        {
            int rando = rnd.Next(3);
            if (rando < 2 && catch2Stack.Item != null) //fish
            {
                ItemStack drop = catch2Stack.Clone();
                drop.StackSize = 1;
                Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 2, Pos.Z + 0.5), null);
                //Api.World.SpawnItemEntity(catch2Stack, Pos.ToVec3d().Add(0.5, 2, 0.5)); //slippery
            }
            else
            {
                byPlayer.InventoryManager.TryGiveItemstack(catch2Stack);
            }
            catch2Slot.TakeOut(1);
            MarkDirty(true);
            return true;
        }
        else if (!catch1Slot.Empty)
        {
            int rando = rnd.Next(3);
            if (rando < 2 && catch1Stack.Item != null) //fish
            {
                ItemStack drop = catch1Stack.Clone();
                drop.StackSize = 1;
                Api.World.SpawnItemEntity(drop, new Vec3d(Pos.X + 0.5, Pos.Y + 2, Pos.Z + 0.5), null);
                //Api.World.SpawnItemEntity(catch1Stack, Pos.ToVec3d().Add(0.5, 2, 0.5)); //slippery
            }
            else
                byPlayer.InventoryManager.TryGiveItemstack(catch1Stack);
            catch1Slot.TakeOut(1);
            MarkDirty(true);
            return true;
        }
        return false;
    }


    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        bool rot = false;
        if (!catch1Slot.Empty || !catch2Slot.Empty)
        {
            sb.Append(Lang.Get("There's something in your trap."));
            if (!catch1Slot.Empty)
            {
                if (catch1Stack.Block != null)
                { }
                else if (!catch1Stack.Item.Code.Path.Contains("psfish"))
                { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
                rot = true;
            }
            else if (!catch2Slot.Empty && !rot)
            {
                if (catch2Stack.Block != null)
                { }
                else if (!catch2Stack.Item.Code.Path.Contains("psfish"))
                { sb.Append(" " + Lang.Get("It smells a little funky in there.")); }
            }
            sb.AppendLine().AppendLine();
        }

    }


    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        MeshData mesh;
        string shapePath;
        Block tmpBlock;
        bool alive;
        BlockWeirTrap block = Api.World.BlockAccessor.GetBlock(Pos) as BlockWeirTrap;
        ITexPositionSource texture = tesselator.GetTexSource(block);
        ITexPositionSource tmpTextureSource = texture;

        if (inventory != null)
        {
            for (int i = 0; i <= 1; i++)
            {
                shapePath = "";
                alive = false;
                if (!inventory[i].Empty) //fish, shell, or rot
                {
                    if (inventory[i].Itemstack.Block != null) //shell or relic
                    {
                        if (inventory[i].Itemstack.Block.Code.Path.Contains("temporal"))
                        {
                            shapePath = "primitivesurvival:shapes/block/relic/" + inventory[i].Itemstack.Block.FirstCodePart(0);
                            //System.Diagnostics.Debug.WriteLine(shapePath);
                        }
                        else if (inventory[i].Itemstack.Block.Code.Path.Contains("statue"))
                        {
                            shapePath = "primitivesurvival:shapes/block/relic/statue/" + inventory[i].Itemstack.Block.FirstCodePart(0);
                        }
                        else if (inventory[i].Itemstack.Block.Code.Path.Contains("necronomicon"))
                        {
                            shapePath = "primitivesurvival:shapes/block/relic/necronomicon-closed";
                        }
                        else
                        {
                            shapePath = "game:shapes/block/seashell/" + inventory[i].Itemstack.Block.FirstCodePart(1);
                        }
                        tmpTextureSource = tesselator.GetTexSource(inventory[i].Itemstack.Block);

                    }

                    else if (!inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                    {
                        tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturerot"));
                        tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-pike";
                    }
                    else if (inventory[i].Itemstack.Item.Code.Path.Contains("psfish"))
                    {
                        shapePath = "primitivesurvival:shapes/item/fishing/fish-" + inventory[i].Itemstack.Item.LastCodePart(1).ToString();
                        if (inventory[i].Itemstack.Item.Code.Path.Contains("cooked"))
                        {
                            tmpBlock = Api.World.GetBlock(block.CodeWithPath("texturecooked"));
                            tmpTextureSource = ((ICoreClientAPI)Api).Tesselator.GetTexSource(tmpBlock);
                        }
                        else
                        {
                            alive = true;
                            tmpTextureSource = texture;
                        }
                    }
                    if (shapePath != "")
                    {
                        mesh = block.GenMesh(Api as ICoreClientAPI, shapePath, tmpTextureSource, i, alive, tesselator);
                        mesher.AddMeshData(mesh);
                    }
                }
            }
        }
        return true;
    }
}
