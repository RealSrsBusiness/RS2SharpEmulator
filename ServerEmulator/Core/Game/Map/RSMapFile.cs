using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerEmulator.Core.Game.Map
{
    class RSMapFile
    {
        struct GameObjectDef
        {
            public int id, x, y, z;
            public int type; //collision type? object type? both?
            public int orientation; //needed for collision
        }

        List<GameObjectDef> objects = new List<GameObjectDef>();
        public sbyte[,,] renderRuleFlags;

        public void DecodeObjectData(byte[] data, int localX, int localY)
        {
            //
        }
    }
}
