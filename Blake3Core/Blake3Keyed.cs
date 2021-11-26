namespace Blake3Core
{
    public class Blake3Keyed : Blake3
    {
        public Blake3Keyed(byte[] key)
            : base(Flag.KeyedHash, key, HashSizeInBits)
        {
        }

        public Blake3Keyed(uint[] key)
            : base(Flag.KeyedHash, key, HashSizeInBits)
        {
        }
    }
}
