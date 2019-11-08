using RadiusDictionariesLib.Attributes;
using RadiusDictionariesLib.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace RadiusDictionariesLib.Tests
{
    public class ParsingAvpTests
    {
        [Fact]
        public void Test1()
        {
            var username = new byte[] { 1, 5, (byte)'l', (byte)'e', (byte)'e' };
            var att = Helpers.Parsing.GetSingleAttribute(username);
            Assert.IsType<UserName>(att);
            Assert.Equal("lee", (att as UserName).Value);
        }
        [Fact]
        public void Test2()
        {
            var password = new byte[] { 2, 5, (byte)'p', (byte)'w', (byte)'d' };
            var att = Helpers.Parsing.GetSingleAttribute(password);
            Assert.IsType<UserPassword>(att);
            Assert.Equal("pwd", (att as UserPassword).Value);
        }
        [Fact]
        public void Test3()
        {
            var bytes = Encoding.UTF8.GetBytes("🔐🔐🔐");
            var password = new byte[] { 3, (byte)(bytes.Length + 2) }.Concat(bytes).ToArray();
            var att = Helpers.Parsing.GetSingleAttribute(password);
            Assert.IsType<ChapPassword>(att);
            Assert.Equal(bytes, (att as ChapPassword).Value);
        }
        [Fact]
        public void Test4()
        {
            var service = new byte[] { 6, 6, 0, 0, 0, 1 };
            var att = Helpers.Parsing.GetSingleAttribute(service);
            Assert.IsType<ServiceType>(att);
            Assert.Equal(ServiceTypeValue.LoginUser, (att as ServiceType).Value);
        }
        [Fact]
        public void Test5()
        {
            var bytes = BitConverter.GetBytes(5001u).Reverse().ToArray();
            var timeout = new byte[] { 27, (byte)(bytes.Length + 2) }.Concat(bytes).ToArray();
            var att = Helpers.Parsing.GetSingleAttribute(timeout);
            Assert.IsType<SessionTimeout>(att);
            Assert.Equal(5001u, (att as SessionTimeout).Value);
        }
        [Fact]
        public void Test6()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var bytes = ip.GetAddressBytes();
            var framedip = new byte[] { 8, (byte)(bytes.Length + 2) }.Concat(bytes).ToArray();
            var att = Helpers.Parsing.GetSingleAttribute(framedip);
            Assert.IsType<FramedIpAddress>(att);
            Assert.Equal(ip, (att as FramedIpAddress).Value);
        }
    }
}
