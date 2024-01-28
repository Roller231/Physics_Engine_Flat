using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FlatPhysics;
using Microsoft.Xna.Framework;

namespace PhysicsEngine
{
    public static class FlatConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(FlatVector v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static void ToVector2Array(FlatVector[] src, ref Vector2[] dst)
        {
            if(dst is null || src.Length != dst.Length)
            {
                dst = new Vector2[src.Length];
            }

            for(int i = 0; i < src.Length; i++)
            {
                FlatVector v = src[i];
                dst[i] = new Vector2(v.X, v.Y);
            }
        }

    }
}
