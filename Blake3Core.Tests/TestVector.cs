using Xunit.Abstractions;

namespace Blake3Core.Tests
{
    public class TestVector : IXunitSerializable
    {
        public int InputLength { get; set; }
        public string Hash { get; set; }
        public string KeyedHash { get; set; }
        public string DerivedKeyHash { get; set; }
        public string Key { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            InputLength = info.GetValue<int>(nameof(InputLength));
            Hash = info.GetValue<string>(nameof(Hash));
            KeyedHash = info.GetValue<string>(nameof(KeyedHash));
            DerivedKeyHash = info.GetValue<string>(nameof(DerivedKeyHash));
            Key = info.GetValue<string>(nameof(Key));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(InputLength), InputLength);
            info.AddValue(nameof(Hash), Hash);
            info.AddValue(nameof(KeyedHash), KeyedHash);
            info.AddValue(nameof(DerivedKeyHash), DerivedKeyHash);
            info.AddValue(nameof(Key), Key);
        }

        public override string ToString()
        {
            return $"{{ InputLength={InputLength,6}, Hash={Hash}, KeyedHash={KeyedHash}, DerivedKeyHash={DerivedKeyHash}, Key={Key} }}";
        }
    }
}
