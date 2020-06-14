using System;

namespace Blake3Core
{
    [Flags]
    enum Flag : uint
    {
        None                = 0x00,
        ChunkStart          = 0x01,
        ChunkEnd            = 0x02,
        Parent              = 0x04,
        Root                = 0x08,
        KeyedHash           = 0x10,
        DeriveKeyContext    = 0x20,
        DeriveKeyMaterial   = 0x40,
    }
}
