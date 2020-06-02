using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Blake3Core
{
    public class Blake3Keyed : Blake3
    {
        public Blake3Keyed(byte [] key)
            : this(key, Flag.KeyedHash)
        {
        }

        protected Blake3Keyed(byte [] key, Flag defaultFlag)
            : base(defaultFlag)
        {
            Key = new uint[8];
            MemoryMarshal.Cast<byte, uint>(key).Slice(0, 8).CopyTo(Key);
        }
    }
}
