using RadiusDictionariesLib.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace RadiusDictionariesLib.Helpers
{
    public static class Parsing
    {
        public static IEnumerable<IRadiusAttribute> GetAttributes(ArraySegment<byte> bytes)
        {
            return SliceAttributes(bytes).Select(GetSingleAttribute);
        }

        public static IEnumerable<ArraySegment<byte>> SliceAttributes(ArraySegment<byte> bytes)
        {
            while (bytes.Count > 0)
            {
                yield return bytes.Slice(0, bytes[1]);
                bytes = bytes.Slice(bytes[1]);
            }
        }

        public static IRadiusAttribute GetSingleAttribute(ArraySegment<byte> bytes)
        {
            if (bytes.Count < 2)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Too few bytes (attributes are at least two bytes long).");

            if (bytes[1] != bytes.Count)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Mismatch between buffer size and size denoted for attribute (second byte).");

            var type = bytes[0];
            var length = bytes[1];

            if (type == 26 && length < 4)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Mismatch between buffer size and size needed for a VSA (at least 4 bytes).");

            Type cls;
            var value = bytes.Slice(2);
            if (type == 26)
            {
                var vendor = BitConverter.ToUInt32(value.Slice(0, 4).Reverse().ToArray());
                value = value.Slice(4); // skip 4 bytes for vendor id

                if (value.Count > 1 && (cls = FindVsaClass(vendor, value[0])) != null)
                {
                    if (value[1] > value.Count - 2)
                        throw new ArgumentOutOfRangeException(nameof(bytes), "VSA sub-type found but second byte (length) exceeds remaining buffer length .");
                    value = value.Slice(2); // skip 2 bytes for type and length
                }
                else cls = typeof(Attributes.VendorSpecific);
            }
            else cls = FindAvpClass(type);

            var attr = Activator.CreateInstance(cls) as IRadiusAttribute;
            if (!TrySetValue(attr, value))
                throw new InvalidCastException("Could not find suitable way to set value from the bytes provided.");

            return attr;
        }

        static IDictionary<byte, Type> _avpTypes;
        static Dictionary<uint, ILookup<byte, Type>> _vsaTypes;


        static Func<Type, uint> vendorIdSelector =
            p => (ushort)p.DeclaringType.GetProperty("VendorId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
        static Func<Type, byte[]> attIdSelector =
            p => (byte[])((AttributeTypeIdentifier)p.GetProperty("AttributeId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null)).Numbers;

        static IDictionary<byte, Type> GetAvpTypeMap()
        {
            return _avpTypes ?? (_avpTypes = typeof(IRadiusAttribute).Assembly.GetTypes()
                .Where(p => typeof(IRadiusAttribute).IsAssignableFrom(p) && !p.IsInterface && !p.IsGenericType)
                .Where(p => p.Namespace.EndsWith($".{nameof(Attributes)}"))
                .GroupBy(
                    p => ((AttributeTypeIdentifier)p.GetProperty("AttributeId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null)).Numbers[0])
                .ToDictionary(g => g.Key, g => g.Select(a => new { a, id = attIdSelector(a) })
                    .Where(a => a.id.Length == 1) // only top-level attributes, not sub-types
                    .First().a
                )
            );
        }
        static Dictionary<uint, ILookup<byte, Type>> GetVsaTypeMap()
        {
            if (_vsaTypes == null)
            {
                _vsaTypes = typeof(IVendorSpecificAttribute).Assembly.GetTypes()
                    .Where(p => typeof(IVendorSpecificAttribute).IsAssignableFrom(p) && !p.IsInterface && !p.IsGenericType)
                    .Where(p => p.Namespace.EndsWith($".{nameof(VendorAttributes)}"))
                    .GroupBy(vendorIdSelector)
                    .ToDictionary(g => g.Key, g => g.Select(a => new { a, id = attIdSelector(a) })
                        .Where(a => a.id.Length == 1) // only top-level attributes, not sub-types
                        .ToLookup(a => a.id[0], a => a.a)
                    );
            }
            return _vsaTypes;
        }

        static DateTime _epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static IDictionary<Type, Func<ArraySegment<byte>, object>> _valueTypeConverters;
        static IDictionary<Type, Func<ArraySegment<byte>, object>> GetValueTypeConverters()
        {
            return _valueTypeConverters ?? (_valueTypeConverters = new Dictionary<Type, Func<ArraySegment<byte>, object>>() {
                { typeof(Enum), bytes => bytes[0] },
                { typeof(byte[]), bytes => bytes.ToArray() },
                { typeof(ArraySegment<byte>), bytes => bytes },
                { typeof(string), bytes => Encoding.UTF8.GetString(bytes) },
                { typeof(IPAddress), bytes => new IPAddress(bytes) },
                { typeof(DateTime), bytes => _epochStart.AddSeconds(BitConverter.ToInt64(bytes.Reverse().ToArray())) },
                { typeof(short), bytes => BitConverter.ToInt16(bytes.Reverse().ToArray()) },
                { typeof(ushort), bytes => BitConverter.ToUInt16(bytes.Reverse().ToArray()) },
                { typeof(long), bytes => BitConverter.ToInt64(bytes.Reverse().ToArray()) },
                { typeof(ulong), bytes => BitConverter.ToUInt64(bytes.Reverse().ToArray()) },
                { typeof(int), bytes => BitConverter.ToInt32(bytes.Reverse().ToArray()) },
                { typeof(uint), bytes => BitConverter.ToUInt32(bytes.Reverse().ToArray()) },
                { typeof(byte), bytes => bytes[0] },
                { typeof((byte type, byte length, ArraySegment<byte> value)), bytes => (bytes[0], bytes[1], bytes.Slice(2)) }
            });
        }

        public static Type FindVsaClass(uint vendor_id, byte vendor_type)
        {
            var map = GetVsaTypeMap();
            return map.ContainsKey(vendor_id) && map[vendor_id].Contains(vendor_type) ? map[vendor_id][vendor_type].First() : null;
        }

        public static Type FindAvpClass(byte type) {
            var map = GetAvpTypeMap();
            return map.ContainsKey(type) ? map[type] : null;
        }

        public static bool TrySetValue(IRadiusAttribute att, ArraySegment<byte> bytes)
        {
            // given the type of the attribute (which defines the type of the attributes Value member)
            // attempt to coerce the bytes to that value using Parse, Convert, etc. 
            
            // TODO: 
            // Keep in mind that the  value may be a TLV so we can go recursive here. One thing to 
            // look for could be a fields called Values (plural) versus the singular. Another
            // could be that the type of att has nested types.
            if (att is ITypeLengthValue)
            {
                // read the type and length
                // lookup the sub-type
                // try to set the value of the sub-type (potentially recurse)
            }

            var prop = att.GetType().GetField("Value");
            if (prop == null)
                return false;

            var ft = prop.FieldType;
            var map = GetValueTypeConverters();
            var converter = map.ContainsKey(ft) ? map[ft]
                : map.FirstOrDefault(g => g.Key.IsAssignableFrom(ft)).Value;

            if (converter == null)
                return false;

            var value = converter(bytes);
            if (value == null)
                return false;

            prop.SetValue(att, value);

            return true;
        }
    }
}
