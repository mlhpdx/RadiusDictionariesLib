using System;
using System.Linq;

namespace RadiusDictionariesLib.Types
{
    public struct AttributeTypeIdentifier
    {
        public byte[] Numbers { get; private set; }

        public override bool Equals(object other)
        {
            if (other == null || !(other is AttributeTypeIdentifier))
                return false;
            return Numbers?.Zip(((AttributeTypeIdentifier)other).Numbers, (n1, n2) => n1 == n2)
               .Aggregate(true, (b1, b2) => b1 && b2) == true;
        }

        public static bool operator ==(AttributeTypeIdentifier @this, AttributeTypeIdentifier other) => @this.Equals(other);
        public static bool operator !=(AttributeTypeIdentifier @this, AttributeTypeIdentifier other) => !@this.Equals(other);

        public override int GetHashCode() => string.Join(", ", Numbers.Select(c => c.ToString())).GetHashCode();

        public static implicit operator AttributeTypeIdentifier(byte number) => new AttributeTypeIdentifier() { Numbers = new[] { number } };

        public static implicit operator AttributeTypeIdentifier(string spec)
            => new AttributeTypeIdentifier()
                {
                    Numbers =
                    spec.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(n =>
                        n.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ? Convert.ToByte(n, 16) : byte.Parse(n)).ToArray()
                };
    }
}
