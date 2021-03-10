using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
 
public class BEMetalBucketFilled : BlockEntityContainer
{
    internal InventoryGeneric inventory;
        
    public override InventoryBase Inventory
    {
        get { return inventory; }
    }

    public override string InventoryClassName
    {
        get { return "metalbucketfilled"; }
    }
    MeshData currentMesh;
    BlockMetalBucketFilled ownBlock;

    public float MeshAngle;

    public BEMetalBucketFilled()
    {
        inventory = new InventoryGeneric(1, null, null);
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        ownBlock = Block as BlockMetalBucketFilled;
        if (Api.Side == EnumAppSide.Client)
        {
            currentMesh = GenMesh();
            MarkDirty(true);
        }
    }


    public override void OnBlockBroken()
    {
        // Don't drop inventory contents
    }


    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);

        if (Api.Side == EnumAppSide.Client)
        {
            currentMesh = GenMesh();
            MarkDirty(true);
        }
    }

    public ItemStack GetContent()
    {
        return inventory[0].Itemstack;
    }


    internal void SetContent(ItemStack stack)
    {
        inventory[0].Itemstack = stack;
        MarkDirty(true);
    }
        


    internal MeshData GenMesh()
    {
        if (ownBlock == null) return null;
            
        MeshData mesh = ownBlock.GenMesh(Api as ICoreClientAPI, GetContent(), Pos);

        if (mesh.CustomInts != null)
        {
            for (int i = 0; i < mesh.CustomInts.Count; i++)
            {
                mesh.CustomInts.Values[i] |= 1 << 27; // Disable water wavy
                mesh.CustomInts.Values[i] |= 1 << 26; // Enabled weak foam
            }
        }

        return mesh;
    }


    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        BlockMetalBucketFilled block = Api.World.BlockAccessor.GetBlock(Pos) as BlockMetalBucketFilled; 
        if (currentMesh != null)
        {
            mesher.AddMeshData(currentMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
        }
        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);

        MeshAngle = tree.GetFloat("meshAngle", MeshAngle);

        if (Api != null)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                currentMesh = GenMesh();
                MarkDirty(true);
            }
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);

        tree.SetFloat("meshAngle", MeshAngle);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        //do nothing to override perishable info
    }

}

