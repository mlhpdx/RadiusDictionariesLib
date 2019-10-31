using RadiusDictionariesLib.Types;
using RadiusDictionariesLib.Attributes;
using RadiusDictionariesLib.VendorAttributes;

using System;
using Xunit;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RadiusDictionaryLib.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.IsAssignableFrom<IRadiusAttribute>(new UserName());
        }
        [Fact]
        public void Test2()
        {
            Assert.IsAssignableFrom<Aruba.IArubaAttribute>(new Aruba.AirgroupDeviceType());
            Assert.IsAssignableFrom<IRadiusAttribute>(new Aruba.AirgroupDeviceType());
        }
        [Fact]
        public void Test3()
        {
            Assert.Equal((AttributeTypeIdentifier)1, UserName.AttributeId);
            Assert.Equal("User-Name", UserName.Name);
        }
        [Fact]
        public void Test4()
        {
            var type = typeof(IRadiusAttribute);
            var types = type.Assembly.GetTypes()
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsGenericType)
                .Where(p => p.Namespace.EndsWith(".Attributes"));

            var map = types.ToLookup(
                p => p.GetProperty("Name", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null) as string,
                p => ((AttributeTypeIdentifier)p.GetProperty("AttributeId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null)).Numbers[0]);

            Assert.All(map, g => Assert.True(g.Count() == 1));
        }
        [Fact]
        public void Test5()
        {
            var type = typeof(IRadiusAttribute);
            var types = type.Assembly.GetTypes()
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsGenericType)
                .Where(p => p.Namespace.EndsWith(".Attributes"));

            var attrs = types.Select(p => new
            {
                Numbers = ((AttributeTypeIdentifier)p.GetProperty("AttributeId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null)).Numbers,
                Name = p.GetProperty("Name", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null) as string
            });
            var map = attrs.Where(a => a.Numbers.Length == 1).ToLookup(
                a => a.Numbers[0],
                a => a.Name
            );

            Assert.All(map, g => Assert.True(g.Count() == 1));
        }
        [Fact]
        public void Test6()
        {
            var type = typeof(IRadiusAttribute);
            var types = type.Assembly.GetTypes()
                .Where(p => p.IsAbstract && p.IsSealed)
                .Where(p => p.Namespace.EndsWith(".VendorAttributes"));

            Assert.All(types, t => Assert.NotNull(t.GetProperty("VendorId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)));
        }
#if NETCOREAPP3_0
        [Fact]
        public void Test6()
        {
            var attrs = new IRadiusAttribute[] {
                new UserName() { Value = "lee" },
                new UserPassword() { Value = "Password123" },
                new Aruba.AirgroupDeviceType() { },
                new Cisco.FaxAccountIdOrigin() { }
            };
            var results = attrs.Select(attr => attr switch {
                    IVendorSpecificAttribute vsa => vsa switch {
                        IArubaAttribute => "Aruba calling...",
                        {} => "I'm a VSA"
                    },
                    UserName u => $"It's {u.Value}",
                    UserPassword pw => "Shhh.",
                });
            Assert.True(new[] { "It's lee", "Shhh.", "Aruba calling...", "I'm a VSA" }, results);
        }
#endif
    }
}
