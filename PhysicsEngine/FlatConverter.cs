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
    }
}
