using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using System.Collections.Generic;

public class BEHide : BlockEntity
{
   
    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        MeshData mesh;
        string shapeBase = "primitivesurvival:shapes/";
        BlockHide block = Api.World.BlockAccessor.GetBlock(Pos) as BlockHide;
        ITexPositionSource texture = tesselator.GetTexSource(block);
        string shapePath = "block/hide/" + block.FirstCodePart();
        System.Diagnostics.Debug.WriteLine(shapePath);
        mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase + shapePath, texture, tesselator);
        mesher.AddMeshData(mesh);
        return true;
    }
}


