using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct ChainingValue
    {
        [FieldOffset( 0)]   public uint h0;
        [FieldOffset( 4)]   public uint h1;
        [FieldOffset( 8)]   public uint h2;
        [FieldOffset(12)]   public uint h3;
        [FieldOffset(16)]   public uint h4;
        [FieldOffset(20)]   public uint h5;
        [FieldOffset(24)]   public uint h6;
        [FieldOffset(28)]   public uint h7;
    }
}
