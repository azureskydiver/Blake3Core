using System;
using System.Security.Cryptography;

namespace Blake3Core
{
    public class Blake3 : HashAlgorithm
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            throw new NotImplementedException();
        }

        protected override byte[] HashFinal()
        {
            throw new NotImplementedException();
        }
    }
}
