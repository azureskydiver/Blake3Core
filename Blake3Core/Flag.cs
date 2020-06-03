using System;

namespace Blake3Core
{
    [Flags]
    enum Flag
    {
        ChunkStart          = 1 << 0,
        ChunkEnd            = 1 << 1,
        Parent              = 1 << 2,
        Root                = 1 << 3,
        KeyedHash           = 1 << 4,
        DeriveKeyContext    = 1 << 5,
        DeriveKeyMaterial   = 1 << 6,
        None
    }
}
