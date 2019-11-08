using RadiusDictionariesLib.Attributes;
using RadiusDictionariesLib.Types;
using RadiusDictionariesLib.VendorAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace RadiusDictionariesLib.Tests
{
    public class ParsingVsaTests
    {
        [Fact]
        public void Test1()
        {
            var avpair = new byte[] { 26, 9, 0, 0, 0, 9, 1, 3, (byte)'a' };
            var att = Helpers.Parsing.GetSingleAttribute(avpair);
            Assert.IsType<Cisco.Avpair>(att);
            Assert.Equal("a", (att as Cisco.Avpair).Value);
        }
        [Fact]
        public void Test2()
        {
            var avpair = new byte[] { 26, 9, 1, 1, 1, 1, 1, 1, 1 }; // bogus
            var att = Helpers.Parsing.GetSingleAttribute(avpair);
            Assert.IsType<VendorSpecific>(att);
            Assert.Equal(new ArraySegment<byte>(avpair).Slice(6), (att as VendorSpecific).Value);
        }
        [Fact]
        public void Test3()
        {
            var avpair = new byte[] { 26, 11, 1, 1, 1, 1, 1, 1, 1 }; // bogus
            Assert.Throws<ArgumentOutOfRangeException>(() => Helpers.Parsing.GetSingleAttribute(avpair));
        }
        [Fact]
        public void Test4()
        {
            var avpair = new byte[] { 26, 9, 0, 0, 0, 9, 1, 5, (byte)'a' };
            Assert.Throws<ArgumentOutOfRangeException>(() => Helpers.Parsing.GetSingleAttribute(avpair));
        }

        private static byte[] HexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
        [Fact]
        public void Test5()
        {
            var bytes = HexToBytes("2c1335354638413131432d3241373433303030011761646d696e2d5448494e4b573533305c61646d696e04060a010029201443322d48524e2d5343472d436c75737465720506000000011e2744342d36382d34442d32412d37342d33303a5a65747461427974652d50617373706f696e741f1338342d33412d34422d31332d44312d41360606000000025903003d06000000134d13434f4e4e454354203830322e3131612f6e4f08020a0006150018256161612d7631392e656e672e72722e636f6d3b313434313233323734313b34343237351a1b000061dd03155a65747461427974652d50617373706f696e741a0c000061dd0906000003201a0c000061dd0706ac13a04c1a0c000061dd0806ac13a05c501223955e36f3f62e65f184ee5378d5cfeb21043138");
            var attrs = Helpers.Parsing.GetAttributes(bytes).ToArray();
            Assert.True(attrs?.Any());
            Assert.Equal("admin-THINKW530\\admin", (attrs.FirstOrDefault(a => a is UserName) as UserName).Value);
            Assert.Equal(IPAddress.Parse("10.1.0.41"), (attrs.FirstOrDefault(a => a is NasIpAddress) as NasIpAddress).Value);
            Assert.Equal(HexToBytes("020a00061500"), (attrs.FirstOrDefault(a => a is EapMessage) as EapMessage).Value);
        }
        [Fact]
        public void Test6()
        {
            var bytes = HexToBytes("2c1335354638413131432d324137343330303008060ac8005b32266434363834646561373433633834336134623133643161363535663861313163303030323306000000012806000000012d0600000001011761646d696e2d5448494e4b573533305c61646d696e04060a010029201443322d48524e2d5343472d436c75737465720506000000011e2744342d36382d34442d32412d37342d33303a5a65747461427974652d50617373706f696e741f1338342d33412d34422d31332d44312d41363d06000000134d13434f4e4e454354203830322e3131612f6e370655f8a11d1910564953495445444d534f3d545743190d484f4d454d534f3d545743191f5553455249443d6d61696c7573657230303440656e672e72722e636f6d190c41555448454e3d4d534919237272437573746f6d6572547970653d5265736964656e7469616c3a52524d61696c59186d61696c7573657230303440656e672e72722e636f6d1a1b000061dd03155a65747461427974652d50617373706f696e741a0c000061dd0906000003201a0c000061dd0706ac13a04c1a0c000061dd0806ac13a05c210331");
            var attrs = Helpers.Parsing.GetAttributes(bytes).ToArray();
            Assert.True(attrs?.Any());
            Assert.NotNull(attrs.FirstOrDefault(a => a is EventTimestamp));
            
            Assert.Equal(new DateTime(2015, 09, 15, 22, 52, 13, DateTimeKind.Utc), 
                (attrs.FirstOrDefault(a => a is EventTimestamp) as EventTimestamp).Value);
            
            Assert.Equal(NasPortTypeValue.Wireless80211, (attrs.FirstOrDefault(a => a is NasPortType) as NasPortType).Value);
            Assert.Equal("CONNECT 802.11a/n", (attrs.FirstOrDefault(a => a is ConnectInfo) as ConnectInfo).Value);

            Assert.Equal(AcctStatusTypeValue.Start, (attrs.FirstOrDefault(a => a is AcctStatusType) as AcctStatusType).Value);
            Assert.Equal(AcctAuthenticValue.Radius, (attrs.FirstOrDefault(a => a is AcctAuthentic) as AcctAuthentic).Value);

            Assert.Equal(5, attrs.Count(a => a is Class));
        }
    }
}
