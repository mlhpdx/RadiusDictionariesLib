using System.Collections.Specialized;

namespace RadiusDictionariesLib.Types
{
    public enum RadiusAttributeCardinality : byte { ZeroOrMore = 0, ZeroOrOne = 1, None = 2 }
    public struct RadiusAttributeCardinalies
    {
        private static readonly BitVector32.Section _request_section = BitVector32.CreateSection(3);
        private static readonly BitVector32.Section _accept_section = BitVector32.CreateSection(3, _request_section);
        private static readonly BitVector32.Section _reject_section = BitVector32.CreateSection(3, _accept_section);
        private static readonly BitVector32.Section _challenge_section = BitVector32.CreateSection(3, _reject_section);

        public RadiusAttributeCardinalies(RadiusAttributeCardinality request, RadiusAttributeCardinality accept, RadiusAttributeCardinality reject, RadiusAttributeCardinality challenge)
        {
            _flags = new BitVector32();
            Request = request;
        }
        BitVector32 _flags;

        public RadiusAttributeCardinality Request
        {
            get => (RadiusAttributeCardinality)_flags[_reject_section];
            set => _flags[_request_section] = (byte)value;
        }
        public RadiusAttributeCardinality Accept
        {
            get => (RadiusAttributeCardinality)_flags[_reject_section];
            set => _flags[_accept_section] = (byte)value;
        }
        public RadiusAttributeCardinality Reject
        {
            get => (RadiusAttributeCardinality)_flags[_reject_section];
            set => _flags[_reject_section] = (byte)value;
        }
        public RadiusAttributeCardinality Challenge
        {
            get => (RadiusAttributeCardinality)_flags[_reject_section];
            set => _flags[_challenge_section] = (byte)value;
        }

        public static readonly RadiusAttributeCardinalies Unrestricted = new RadiusAttributeCardinalies()
        {
            Request = RadiusAttributeCardinality.ZeroOrMore,
            Accept = RadiusAttributeCardinality.ZeroOrMore,
            Reject = RadiusAttributeCardinality.ZeroOrMore,
            Challenge = RadiusAttributeCardinality.ZeroOrMore
        };
    }
}
