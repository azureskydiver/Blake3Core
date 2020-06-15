using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Blake3Core.Tests
{
    public class Constructors
    {
        [Fact]
        void CanCreateBlake3()
        {
            var hash = new Blake3();
        }

        [Fact]
        void FailsNullBytesPasswordForBlake3Keyed()
        {
            byte[] bytes = null;
            Assert.Throws<ArgumentException>(() => new Blake3Keyed(bytes));
        }

        [Fact]
        void FailsNullUintsPasswordForBlake3Keyed()
        {
            uint[] uints = null;
            Assert.Throws<ArgumentException>(() => new Blake3Keyed(uints));
        }

        [Theory]
        [InlineData( 0, false)]
        [InlineData( 1, false)]
        [InlineData(31, false)]
        [InlineData(32, true)]
        [InlineData(33, false)]
        void CreateBlake3KeyedUsingBytes(int size, bool succeeds)
        {
            var bytes = new byte[size];
            var random = new Random();
            random.NextBytes(bytes);

            Action action = delegate { var hash = new Blake3Keyed(bytes); };
            if (!succeeds)
                Assert.Throws<ArgumentException>(action);
            else
                action();
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(7, false)]
        [InlineData(8, true)]
        [InlineData(9, false)]
        void CreateBlake3KeyedUsingUints(int size, bool succeeds)
        {
            var uints = new uint[size];
            var bytes = MemoryMarshal.Cast<uint, byte>(uints);
            var random = new Random();
            random.NextBytes(bytes);
            Action action = delegate { var hash = new Blake3Keyed(uints); };
            if (!succeeds)
                Assert.Throws<ArgumentException>(action);
            else
                action();
        }

        [Fact]
        void CanCreateBlake3DerivedKey()
        {
            var hash = new Blake3DerivedKey("Blake3Core");
        }
    }
}
