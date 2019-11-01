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
            var avpair = new byte[] { 26, 9, 0, 0, 0, 9, 1, 1, (byte)'a' };
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
            var avpair = new byte[] { 26, 9, 0, 0, 0, 9, 1, 3, (byte)'a' };
            Assert.Throws<ArgumentOutOfRangeException>(() => Helpers.Parsing.GetSingleAttribute(avpair));
        }
        [Fact]
        public void Test5()
        {
        }
        [Fact]
        public void Test6()
        {
        }
    }
}
