using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Microsoft.Toolkit.HighPerformance;
using Server.Buffers;
using Server.Network;
using Server.Random;

namespace Server
{
    public static class Utility
    {
        private static Dictionary<IPAddress, IPAddress> _ipAddressTable;

        private static readonly SkillName[] m_AllSkills =
        {
            SkillName.Alchemy,
            SkillName.Anatomy,
            SkillName.AnimalLore,
            SkillName.ItemID,
            SkillName.ArmsLore,
            SkillName.Parry,
            SkillName.Begging,
            SkillName.Blacksmith,
            SkillName.Fletching,
            SkillName.Peacemaking,
            SkillName.Camping,
            SkillName.Carpentry,
            SkillName.Cartography,
            SkillName.Cooking,
            SkillName.DetectHidden,
            SkillName.Discordance,
            SkillName.EvalInt,
            SkillName.Healing,
            SkillName.Fishing,
            SkillName.Forensics,
            SkillName.Herding,
            SkillName.Hiding,
            SkillName.Provocation,
            SkillName.Inscribe,
            SkillName.Lockpicking,
            SkillName.Magery,
            SkillName.MagicResist,
            SkillName.Tactics,
            SkillName.Snooping,
            SkillName.Musicianship,
            SkillName.Poisoning,
            SkillName.Archery,
            SkillName.SpiritSpeak,
            SkillName.Stealing,
            SkillName.Tailoring,
            SkillName.AnimalTaming,
            SkillName.TasteID,
            SkillName.Tinkering,
            SkillName.Tracking,
            SkillName.Veterinary,
            SkillName.Swords,
            SkillName.Macing,
            SkillName.Fencing,
            SkillName.Wrestling,
            SkillName.Lumberjacking,
            SkillName.Mining,
            SkillName.Meditation,
            SkillName.Stealth,
            SkillName.RemoveTrap,
            SkillName.Necromancy,
            SkillName.Focus,
            SkillName.Chivalry,
            SkillName.Bushido,
            SkillName.Ninjitsu,
            SkillName.Spellweaving
        };

        private static readonly SkillName[] m_CombatSkills =
        {
            SkillName.Archery,
            SkillName.Swords,
            SkillName.Macing,
            SkillName.Fencing,
            SkillName.Wrestling
        };

        private static readonly SkillName[] m_CraftSkills =
        {
            SkillName.Alchemy,
            SkillName.Blacksmith,
            SkillName.Fletching,
            SkillName.Carpentry,
            SkillName.Cartography,
            SkillName.Cooking,
            SkillName.Inscribe,
            SkillName.Tailoring,
            SkillName.Tinkering
        };

        private static readonly Stack<ConsoleColor> m_ConsoleColors = new();

        public static void Separate(StringBuilder sb, string value, string separator)
        {
            if (sb.Length > 0)
            {
                sb.Append(separator);
            }

            sb.Append(value);
        }

        public static string Intern(string str) => str?.Length > 0 ? string.Intern(str) : str;

        public static void Intern(ref string str)
        {
            str = Intern(str);
        }

        public static IPAddress Intern(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return null;
            }

            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }

            _ipAddressTable ??= new Dictionary<IPAddress, IPAddress>();

            if (!_ipAddressTable.TryGetValue(ipAddress, out var interned))
            {
                interned = ipAddress;
                _ipAddressTable[ipAddress] = interned;
            }

            return interned;
        }

        public static void Intern(ref IPAddress ipAddress)
        {
            ipAddress = Intern(ipAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint IPv4ToAddress(IPAddress ipAddress)
        {
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return 0;
            }

            Span<byte> integer = stackalloc byte[4];
            ipAddress.TryWriteBytes(integer, out var bytesWritten);
            return bytesWritten != 4 ? 0 : BinaryPrimitives.ReadUInt32BigEndian(integer);
        }

        public static bool IPMatchClassC(IPAddress ip1, IPAddress ip2)
        {
            var a = IPv4ToAddress(ip1);
            var b = IPv4ToAddress(ip2);

            return a == 0 || b == 0 ? ip1.Equals(ip2) : (a & 0xFFFFFF) == (b & 0xFFFFFF);
        }

        public static bool IPMatchCIDR(IPAddress cidrAddress, IPAddress address, int cidrLength)
        {
            if (cidrAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return false;
                }

                cidrLength += 96;
            }

            cidrAddress = cidrAddress.MapToIPv6();
            address = address.MapToIPv6();

            cidrLength = Math.Clamp(cidrLength, 0, 128);

            Span<byte> cidrBytes = stackalloc byte[16];
            cidrAddress.TryWriteBytes(cidrBytes, out var _);

            Span<byte> addrBytes = stackalloc byte[16];
            address.TryWriteBytes(addrBytes, out var _);

            var i = 0;
            int offset;

            if (cidrLength < 32)
            {
                offset = cidrLength;
            }
            else
            {
                var index = Math.DivRem(cidrLength, 32, out offset);
                while (index > 0)
                {
                    if (
                        BinaryPrimitives.ReadInt32BigEndian(cidrBytes.Slice(i, 4)) !=
                        BinaryPrimitives.ReadInt32BigEndian(addrBytes.Slice(i, 4))
                    )
                    {
                        return false;
                    }

                    i += 4;
                    --index;
                }
            }

            if (offset == 0)
            {
                return true;
            }

            var c = BinaryPrimitives.ReadInt32BigEndian(cidrBytes.Slice(i, 4));
            var a = BinaryPrimitives.ReadInt32BigEndian(addrBytes.Slice(i, 4));

            var mask = (1 << (32 - offset)) - 1;
            var min = ~mask & c;
            var max = c | mask;

            return a >= min && a <= max;
        }

        public static bool IsValidIP(string val) => IPMatch(val, IPAddress.Any, out var valid) || valid;

        public static bool IPMatch(string val, IPAddress ip) => IPMatch(val, ip, out _);

        public static bool IPMatch(string val, IPAddress ip, out bool valid)
        {
            var family = ip.AddressFamily;
            var useIPv6 = family == AddressFamily.InterNetworkV6 || val.ContainsOrdinal(':');

            ip = useIPv6 ? ip.MapToIPv6() : ip.MapToIPv4();

            Span<byte> ipBytes = stackalloc byte[useIPv6 ? 16 : 4];
            ip.TryWriteBytes(ipBytes, out _);

            return useIPv6 ? IPv6Match(val, ipBytes, out valid) : IPv4Match(val, ipBytes, out valid);
        }

        public static bool IPv4Match(ReadOnlySpan<char> val, ReadOnlySpan<byte> ip, out bool valid)
        {
            var match = true;
            valid = true;
            var end = val.Length;
            var byteIndex = 0;
            var section = 0;
            var number = 0;
            var isRange = false;
            var intBase = 10;
            var endOfSection = false;
            var sectionStart = 0;

            var num = ip[byteIndex++];

            for (var i = 0; i < end; i++)
            {
                var chr = val[i];
                if (section >= 4)
                {
                    valid = false;
                    return false;
                }

                switch (chr)
                {
                    default:
                        {
                            if (!Uri.IsHexDigit(chr))
                            {
                                valid = false;
                                return false;
                            }

                            number = number * intBase + Uri.FromHex(chr);
                            break;
                        }
                    case 'x':
                    case 'X':
                        {
                            if (i == sectionStart)
                            {
                                intBase = 16;
                                break;
                            }

                            valid = false;
                            return false;
                        }
                    case '-':
                        {
                            if (i == sectionStart || i + 1 == end || val[i + 1] == '.')
                            {
                                valid = false;
                                return false;
                            }

                            // Only allows a single range in a section
                            if (isRange)
                            {
                                valid = false;
                                return false;
                            }

                            isRange = true;
                            match = match && num >= number;
                            number = 0;
                            break;
                        }
                    case '*':
                        {
                            if (i != sectionStart || i + 1 < end && val[i + 1] != '.')
                            {
                                valid = false;
                                return false;
                            }

                            isRange = true;
                            number = 255;
                            break;
                        }
                    case '.':
                        {
                            endOfSection = true;
                            break;
                        }
                }

                if (endOfSection || i + 1 == end)
                {
                    if (number < 0 || number > 255)
                    {
                        valid = false;
                        return false;
                    }

                    match = match && (isRange ? num <= number : number == num);

                    if (++section < 4)
                    {
                        num = ip[byteIndex++];
                    }

                    intBase = 10;
                    number = 0;
                    endOfSection = false;
                    sectionStart = i + 1;
                    isRange = false;
                }
            }

            return match;
        }

        public static bool IPv6Match(ReadOnlySpan<char> val, ReadOnlySpan<byte> ip, out bool valid)
        {
            valid = true;

            // Start must be two `::` or a number
            if (val[0] == ':' && val[1] != ':')
            {
                valid = false;
                return false;
            }

            var match = true;
            var end = val.Length;
            var byteIndex = 2;
            var section = 0;
            var number = 0;
            var isRange = false;
            var endOfSection = false;
            var sectionStart = 0;
            var hasCompressor = false;

            var num = BinaryPrimitives.ReadUInt16BigEndian(ip[..2]);

            for (int i = 0; i < end; i++)
            {
                if (section > 7)
                {
                    valid = false;
                    return false;
                }

                var chr = val[i];
                // We are starting a new sequence, check the previous one then continue
                switch (chr)
                {
                    default:
                        {
                            if (!Uri.IsHexDigit(chr))
                            {
                                valid = false;
                                return false;
                            }

                            number = number * 16 + Uri.FromHex(chr);
                            break;
                        }
                    case '?':
                        {
                            Console.WriteLine("IP Match '?' character is not supported.");
                            valid = false;
                            return false;
                        }
                    // Range
                    case '-':
                        {
                            if (i == sectionStart || i + 1 == end || val[i + 1] == ':')
                            {
                                valid = false;
                                return false;
                            }

                            // Only allows a single range in a section
                            if (isRange)
                            {
                                valid = false;
                                return false;
                            }

                            isRange = true;

                            // Check low part of the range
                            match = match && num >= number;
                            number = 0;
                            break;
                        }
                    // Wild section
                    case '*':
                        {
                            if (i != sectionStart || i + 1 < end && val[i + 1] != ':')
                            {
                                valid = false;
                                return false;
                            }

                            isRange = true;
                            number = 65535;
                            break;
                        }
                    case ':':
                        {
                            endOfSection = true;
                            break;
                        }
                }

                if (!endOfSection && i + 1 != end)
                {
                    continue;
                }

                if (++i == end || val[i] != ':' || section > 0)
                {
                    match = match && (isRange ? num <= number : number == num);

                    // IPv4 matching at the end
                    if (section == 6 && num == 0xFFFF)
                    {
                        var ipv4 = val[(i + 1)..];
                        if (ipv4.Contains('.'))
                        {
                            return IPv4Match(ipv4, ip[^4..], out valid);
                        }
                    }

                    if (i == end)
                    {
                        break;
                    }


                    num = BinaryPrimitives.ReadUInt16BigEndian(ip.Slice(byteIndex, 2));
                    byteIndex += 2;

                    ++section;
                }

                if (i < end && val[i] == ':')
                {
                    if (hasCompressor)
                    {
                        valid = false;
                        return false;
                    }

                    int newSection;

                    if (i + 1 < end)
                    {
                        var remainingColons = val[(i + 1)..].Count(':');
                        // double colon must be at least 2 sections
                        // we need at least 1 section remaining out of 8
                        // This means 8 - 2 would be 6 sections (5 colons)
                        newSection = section + 2 + (5 - remainingColons);
                        if (newSection > 7)
                        {
                            valid = false;
                            return false;
                        }
                    }
                    else
                    {
                        newSection = 7;
                    }

                    var zeroEnd = (newSection + 1) * 2;
                    do
                    {
                        if (match)
                        {
                            if (num != 0)
                            {
                                match = false;
                            }

                            num = BinaryPrimitives.ReadUInt16BigEndian(ip.Slice(byteIndex, 2));
                        }

                        byteIndex += 2;
                    } while (byteIndex < zeroEnd);

                    section = newSection;
                    hasCompressor = true;
                }
                else
                {
                    i--;
                }

                number = 0;
                endOfSection = false;
                sectionStart = i + 1;
                isRange = false;
            }

            return match;
        }

        public static string FixHtml(string str)
        {
            if (str == null)
            {
                return "";
            }

            using var sb = new ValueStringBuilder(str, stackalloc char[Math.Min(40960, str.Length)]);
            ReadOnlySpan<char> invalid = stackalloc []{ '<', '>', '#' };
            ReadOnlySpan<char> replacement = stackalloc []{ '(', ')', '-' };
            sb.ReplaceAny(invalid, replacement, 0, sb.Length);

            return sb.ToString();
        }

        public static int InsensitiveCompare(string first, string second) => first.InsensitiveCompare(second);

        public static bool InsensitiveStartsWith(string first, string second) => first.InsensitiveStartsWith(second);

        public static Direction GetDirection(IPoint2D from, IPoint2D to)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;

            var adx = Abs(dx);
            var ady = Abs(dy);

            if (adx >= ady * 3)
            {
                return dx > 0 ? Direction.East : Direction.West;
            }

            if (ady >= adx * 3)
            {
                return dy > 0 ? Direction.South : Direction.North;
            }

            if (dx > 0)
            {
                return dy > 0 ? Direction.Down : Direction.Right;
            }

            return dy > 0 ? Direction.Left : Direction.Up;
        }

        public static object GetArrayCap(Array array, int index, object emptyValue = null) =>
            array.Length > 0 ? array.GetValue(Math.Clamp(index, 0, array.Length - 1)) : emptyValue;

        public static SkillName RandomSkill() =>
            m_AllSkills[Random(
                m_AllSkills.Length - (Core.ML ? 0 :
                    Core.SE ? 1 :
                    Core.AOS ? 3 : 6)
            )];

        public static SkillName RandomCombatSkill() => m_CombatSkills.RandomElement();

        public static SkillName RandomCraftSkill() => m_CraftSkills.RandomElement();

        public static void FixPoints(ref Point3D top, ref Point3D bottom)
        {
            if (bottom.m_X < top.m_X)
            {
                var swap = top.m_X;
                top.m_X = bottom.m_X;
                bottom.m_X = swap;
            }

            if (bottom.m_Y < top.m_Y)
            {
                var swap = top.m_Y;
                top.m_Y = bottom.m_Y;
                bottom.m_Y = swap;
            }

            if (bottom.m_Z < top.m_Z)
            {
                var swap = top.m_Z;
                top.m_Z = bottom.m_Z;
                bottom.m_Z = swap;
            }
        }

        public static bool RangeCheck(IPoint2D p1, IPoint2D p2, int range) =>
            p1.X >= p2.X - range
            && p1.X <= p2.X + range
            && p1.Y >= p2.Y - range
            && p2.Y <= p2.Y + range;

        public static void FormatBuffer(TextWriter output, Stream input, int length)
        {
            output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            var byteIndex = 0;

            var whole = length >> 4;
            var rem = length & 0xF;

            for (var i = 0; i < whole; ++i, byteIndex += 16)
            {
                var bytes = new StringBuilder(49);
                var chars = new StringBuilder(16);

                for (var j = 0; j < 16; ++j)
                {
                    var c = input.ReadByte();

                    bytes.Append(c.ToString("X2"));

                    if (j != 7)
                    {
                        bytes.Append(' ');
                    }
                    else
                    {
                        bytes.Append("  ");
                    }

                    if (c >= 0x20 && c < 0x7F)
                    {
                        chars.Append((char)c);
                    }
                    else
                    {
                        chars.Append('.');
                    }
                }

                output.Write(byteIndex.ToString("X4"));
                output.Write("   ");
                output.Write(bytes.ToString());
                output.Write("  ");
                output.WriteLine(chars.ToString());
            }

            if (rem != 0)
            {
                var bytes = new StringBuilder(49);
                var chars = new StringBuilder(rem);

                for (var j = 0; j < 16; ++j)
                {
                    if (j < rem)
                    {
                        var c = input.ReadByte();

                        bytes.Append(c.ToString("X2"));

                        if (j != 7)
                        {
                            bytes.Append(' ');
                        }
                        else
                        {
                            bytes.Append("  ");
                        }

                        if (c >= 0x20 && c < 0x7F)
                        {
                            chars.Append((char)c);
                        }
                        else
                        {
                            chars.Append('.');
                        }
                    }
                    else
                    {
                        bytes.Append("   ");
                    }
                }

                output.Write(byteIndex.ToString("X4"));
                output.Write("   ");
                output.Write(bytes.ToString());
                output.Write("  ");
                output.WriteLine(chars.ToString());
            }
        }

        public static void FormatBuffer(TextWriter output, params Memory<byte>[] mems)
        {
            output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            var byteIndex = 0;

            var length = 0;
            for (var i = 0; i < mems.Length; i++)
            {
                length += mems[i].Length;
            }

            var position = 0;
            var memIndex = 0;
            var span = mems[memIndex].Span;

            var whole = length >> 4;
            var rem = length & 0xF;

            for (var i = 0; i < whole; ++i, byteIndex += 16)
            {
                var bytes = new StringBuilder(49);
                var chars = new StringBuilder(16);

                for (var j = 0; j < 16; ++j)
                {
                    var c = span[position++];
                    if (position > span.Length)
                    {
                        span = mems[memIndex++].Span;
                        position = 0;
                    }

                    bytes.Append(c.ToString("X2"));

                    if (j != 7)
                    {
                        bytes.Append(' ');
                    }
                    else
                    {
                        bytes.Append("  ");
                    }

                    if (c >= 0x20 && c < 0x7F)
                    {
                        chars.Append((char)c);
                    }
                    else
                    {
                        chars.Append('.');
                    }
                }

                output.Write(byteIndex.ToString("X4"));
                output.Write("   ");
                output.Write(bytes.ToString());
                output.Write("  ");
                output.WriteLine(chars.ToString());
            }

            if (rem != 0)
            {
                var bytes = new StringBuilder(49);
                var chars = new StringBuilder(rem);

                for (var j = 0; j < 16; ++j)
                {
                    if (j < rem)
                    {
                        var c = span[position++];
                        if (position > span.Length)
                        {
                            span = mems[memIndex++].Span;
                            position = 0;
                        }

                        bytes.Append(c.ToString("X2"));

                        if (j != 7)
                        {
                            bytes.Append(' ');
                        }
                        else
                        {
                            bytes.Append("  ");
                        }

                        if (c >= 0x20 && c < 0x7F)
                        {
                            chars.Append((char)c);
                        }
                        else
                        {
                            chars.Append('.');
                        }
                    }
                    else
                    {
                        bytes.Append("   ");
                    }
                }

                output.Write(byteIndex.ToString("X4"));
                output.Write("   ");
                output.Write(bytes.ToString());
                output.Write("  ");
                output.WriteLine(chars.ToString());
            }
        }

        public static void PushColor(ConsoleColor color)
        {
            try
            {
                m_ConsoleColors.Push(Console.ForegroundColor);
                Console.ForegroundColor = color;
            }
            catch
            {
                // ignored
            }
        }

        public static void PopColor()
        {
            try
            {
                Console.ForegroundColor = m_ConsoleColors.Pop();
            }
            catch
            {
                // ignored
            }
        }

        public static bool NumberBetween(double num, int bound1, int bound2, double allowance)
        {
            if (bound1 > bound2)
            {
                var i = bound1;
                bound1 = bound2;
                bound2 = i;
            }

            return num < bound2 + allowance && num > bound1 - allowance;
        }

        public static void AssignRandomHair(Mobile m, int hue)
        {
            m.HairItemID = m.Race.RandomHair(m);
            m.HairHue = hue;
        }

        public static void AssignRandomHair(Mobile m, bool randomHue = true)
        {
            m.HairItemID = m.Race.RandomHair(m);

            if (randomHue)
            {
                m.HairHue = m.Race.RandomHairHue();
            }
        }

        public static void AssignRandomFacialHair(Mobile m, int hue)
        {
            m.FacialHairItemID = m.Race.RandomFacialHair(m);
            m.FacialHairHue = hue;
        }

        public static void AssignRandomFacialHair(Mobile m, bool randomHue = true)
        {
            m.FacialHairItemID = m.Race.RandomFacialHair(m);

            if (randomHue)
            {
                m.FacialHairHue = m.Race.RandomHairHue();
            }
        }

        // Using this instead of Linq Cast<> means we can ditch the yield and enforce contravariance
        public static HashSet<TOutput> SafeConvertSet<TInput, TOutput>(this IEnumerable<TInput> coll)
            where TOutput : TInput => coll.SafeConvert<HashSet<TOutput>, TInput, TOutput>();

        public static List<TOutput> SafeConvertList<TInput, TOutput>(this IEnumerable<TInput> coll)
            where TOutput : TInput => coll.SafeConvert<List<TOutput>, TInput, TOutput>();

        public static TColl SafeConvert<TColl, TInput, TOutput>(this IEnumerable<TInput> coll)
            where TOutput : TInput where TColl : ICollection<TOutput>, new()
        {
            var outputList = new TColl();

            foreach (var entry in coll)
            {
                if (entry is TOutput outEntry)
                {
                    outputList.Add(outEntry);
                }
            }

            return outputList;
        }

        public static bool ToBoolean(string value)
        {
#pragma warning disable CA1806 // Do not ignore method results
            bool.TryParse(value, out var b);
#pragma warning restore CA1806 // Do not ignore method results

            return b;
        }

        public static double ToDouble(string value)
        {
#pragma warning disable CA1806 // Do not ignore method results
            double.TryParse(value, out var d);
#pragma warning restore CA1806 // Do not ignore method results

            return d;
        }

        public static TimeSpan ToTimeSpan(string value)
        {
#pragma warning disable CA1806 // Do not ignore method results
            TimeSpan.TryParse(value, out var t);
#pragma warning restore CA1806 // Do not ignore method results

            return t;
        }

        public static int ToInt32(ReadOnlySpan<char> value)
        {
            int i;

#pragma warning disable CA1806 // Do not ignore method results
            if (value.StartsWithOrdinal("0x"))
            {
                int.TryParse(value[2..], NumberStyles.HexNumber, null, out i);
            }
            else
            {
                int.TryParse(value, out i);
            }
#pragma warning restore CA1806 // Do not ignore method results

            return i;
        }

        public static uint ToUInt32(ReadOnlySpan<char> value)
        {
            uint i;

#pragma warning disable CA1806 // Do not ignore method results
            if (value.InsensitiveStartsWith("0x"))
            {
                uint.TryParse(value[2..], NumberStyles.HexNumber, null, out i);
            }
            else
            {
                uint.TryParse(value, out i);
            }
#pragma warning restore CA1806 // Do not ignore method results

            return i;
        }

        public static bool ToInt32(ReadOnlySpan<char> value, out int i) =>
            value.InsensitiveStartsWith("0x")
                ? int.TryParse(value[2..], NumberStyles.HexNumber, null, out i)
                : int.TryParse(value, out i);

        public static bool ToUInt32(ReadOnlySpan<char> value, out uint i) =>
            value.InsensitiveStartsWith("0x")
                ? uint.TryParse(value[2..], NumberStyles.HexNumber, null, out i)
                : uint.TryParse(value, out i);

        public static int GetXMLInt32(string intString, int defaultValue)
        {
            try
            {
                return XmlConvert.ToInt32(intString);
            }
            catch
            {
                return int.TryParse(intString, out var val) ? val : defaultValue;
            }
        }

        public static uint GetXMLUInt32(string uintString, uint defaultValue)
        {
            try
            {
                return XmlConvert.ToUInt32(uintString);
            }
            catch
            {
                return uint.TryParse(uintString, out var val) ? val : defaultValue;
            }
        }

        public static DateTime GetXMLDateTime(string dateTimeString, DateTime defaultValue)
        {
            try
            {
                return XmlConvert.ToDateTime(dateTimeString, XmlDateTimeSerializationMode.Utc);
            }
            catch
            {
                return DateTime.TryParse(dateTimeString, out var d) ? d : defaultValue;
            }
        }

        public static TimeSpan GetXMLTimeSpan(string timeSpanString, TimeSpan defaultValue)
        {
            try
            {
                return XmlConvert.ToTimeSpan(timeSpanString);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string GetAttribute(XmlElement node, string attributeName, string defaultValue = null) =>
            node?.Attributes[attributeName]?.Value ?? defaultValue;

        public static string GetText(XmlElement node, string defaultValue) => node?.InnerText ?? defaultValue;

        public static bool InRange(Point3D p1, Point3D p2, int range) =>
            p1.m_X >= p2.m_X - range
            && p1.m_X <= p2.m_X + range
            && p1.m_Y >= p2.m_Y - range
            && p1.m_Y <= p2.m_Y + range;

        public static bool InUpdateRange(Point3D p1, Point3D p2) =>
            p1.m_X >= p2.m_X - 18
            && p1.m_X <= p2.m_X + 18
            && p1.m_Y >= p2.m_Y - 18
            && p1.m_Y <= p2.m_Y + 18;

        public static bool InUpdateRange(Point2D p1, Point2D p2) =>
            p1.m_X >= p2.m_X - 18
            && p1.m_X <= p2.m_X + 18
            && p1.m_Y >= p2.m_Y - 18
            && p1.m_Y <= p2.m_Y + 18;

        public static bool InUpdateRange(IPoint2D p1, IPoint2D p2) =>
            p1.X >= p2.X - 18
            && p1.X <= p2.X + 18
            && p1.Y >= p2.Y - 18
            && p1.Y <= p2.Y + 18;

        // 4d6+8 would be: Utility.Dice( 4, 6, 8 )
        public static int Dice(uint amount, uint sides, int bonus)
        {
            var total = 0;

            for (var i = 0; i < amount; ++i)
            {
                total += (int)RandomSources.Source.Next(1, sides);
            }

            return total + bonus;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var r = RandomMinMax(i, count - 1);
                var swap = list[r];
                list[r] = list[i];
                list[i] = swap;
            }
        }

        public static void Shuffle<T>(this Span<T> list)
        {
            var count = list.Length;
            for (var i = 0; i < count; i++)
            {
                var r = RandomMinMax(i, count - 1);
                var swap = list[r];
                list[r] = list[i];
                list[i] = swap;
            }
        }

        /**
     * Gets a random sample from the source list.
     * Not meant for unbounded lists. Does not shuffle or modify source.
     */
        public static T[] RandomSample<T>(this T[] source, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<T>();
            }

            var length = source.Length;
            Span<bool> list = stackalloc bool[length];
            var sampleList = new T[count];

            var i = 0;
            do
            {
                var rand = Random(length);
                if (!(list[rand] && (list[rand] = true)))
                {
                    sampleList[i++] = source[rand];
                }
            } while (i < count);

            return sampleList;
        }

        public static List<T> RandomSample<T>(this List<T> source, int count)
        {
            if (count <= 0)
            {
                return new List<T>();
            }

            var length = source.Count;
            Span<bool> list = stackalloc bool[length];
            var sampleList = new List<T>(count);

            var i = 0;
            do
            {
                var rand = Random(length);
                if (!(list[rand] && (list[rand] = true)))
                {
                    sampleList[i++] = source[rand];
                }
            } while (i < count);

            return sampleList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomList<T>(params T[] list) => list.RandomElement();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomElement<T>(this IList<T> list) => list.RandomElement(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomElement<T>(this IList<T> list, T valueIfZero) =>
            list.Count == 0 ? valueIfZero : list[Random(list.Count)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RandomBool() => RandomSources.Source.NextBool();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RandomMinMax(uint min, uint max)
        {
            if (min > max)
            {
                var copy = min;
                min = max;
                max = copy;
            }
            else if (min == max)
            {
                return min;
            }

            return min + RandomSources.Source.Next(max - min + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandomMinMax(int min, int max)
        {
            if (min > max)
            {
                var copy = min;
                min = max;
                max = copy;
            }
            else if (min == max)
            {
                return min;
            }

            return min + (int)RandomSources.Source.Next((uint)(max - min + 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Random(int from, int count) => RandomSources.Source.Next(from, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Random(int count) => RandomSources.Source.Next(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Random(uint count) => RandomSources.Source.Next(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomBytes(Span<byte> buffer) => RandomSources.Source.NextBytes(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RandomDouble() => RandomSources.Source.NextDouble();

        /// <summary>
        ///     Random pink, blue, green, orange, red or yellow hue
        /// </summary>
        public static int RandomNondyedHue()
        {
            return Random(6) switch
            {
                0 => RandomPinkHue(),
                1 => RandomBlueHue(),
                2 => RandomGreenHue(),
                3 => RandomOrangeHue(),
                4 => RandomRedHue(),
                5 => RandomYellowHue(),
                _ => 0
            };
        }

        /// <summary>
        ///     Random hue in the range 1201-1254
        /// </summary>
        public static int RandomPinkHue() => Random(1201, 54);

        /// <summary>
        ///     Random hue in the range 1301-1354
        /// </summary>
        public static int RandomBlueHue() => Random(1301, 54);

        /// <summary>
        ///     Random hue in the range 1401-1454
        /// </summary>
        public static int RandomGreenHue() => Random(1401, 54);

        /// <summary>
        ///     Random hue in the range 1501-1554
        /// </summary>
        public static int RandomOrangeHue() => Random(1501, 54);

        /// <summary>
        ///     Random hue in the range 1601-1654
        /// </summary>
        public static int RandomRedHue() => Random(1601, 54);

        /// <summary>
        ///     Random hue in the range 1701-1754
        /// </summary>
        public static int RandomYellowHue() => Random(1701, 54);

        /// <summary>
        ///     Random hue in the range 1801-1908
        /// </summary>
        public static int RandomNeutralHue() => Random(1801, 108);

        /// <summary>
        ///     Random hue in the range 2001-2018
        /// </summary>
        public static int RandomSnakeHue() => Random(2001, 18);

        /// <summary>
        ///     Random hue in the range 2101-2130
        /// </summary>
        public static int RandomBirdHue() => Random(2101, 30);

        /// <summary>
        ///     Random hue in the range 2201-2224
        /// </summary>
        public static int RandomSlimeHue() => Random(2201, 24);

        /// <summary>
        ///     Random hue in the range 2301-2318
        /// </summary>
        public static int RandomAnimalHue() => Random(2301, 18);

        /// <summary>
        ///     Random hue in the range 2401-2430
        /// </summary>
        public static int RandomMetalHue() => Random(2401, 30);

        public static int ClipDyedHue(int hue) => hue < 2 ? 2 :
            hue > 1001 ? 1001 : hue;

        /// <summary>
        ///     Random hue in the range 2-1001
        /// </summary>
        public static int RandomDyedHue() => Random(2, 1000);

        /// <summary>
        ///     Random hue from 0x62, 0x71, 0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59
        /// </summary>
        public static int RandomBrightHue() =>
            RandomDouble() < 0.1
                ? RandomList(0x62, 0x71)
                : RandomList(0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> =>
            val.CompareTo(min) < 0 ? min :
            val.CompareTo(max) > 0 ? max : val;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(T val, T min) where T : IComparable<T> => val.CompareTo(min) < 0 ? val : min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(T val, T max) where T : IComparable<T> => val.CompareTo(max) > 0 ? val : max;

        public static string TrimMultiline(this string str, string lineSeparator = "\n")
        {
            var parts = str.Split(lineSeparator);
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return string.Join(lineSeparator, parts);
        }

        public static string IndentMultiline(this string str, string indent = "\t", string lineSeparator = "\n")
        {
            var parts = str.Split(lineSeparator);
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = $"{indent}{parts[i]}";
            }

            return string.Join(lineSeparator, parts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tidy<T>(this List<T> list) where T : ISerializable
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var entry = list[i];
                if (entry?.Deleted != false)
                {
                    list.RemoveAt(i);
                }
            }

            list.TrimExcess();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tidy<T>(this HashSet<T> set) where T : ISerializable
        {
            set.RemoveWhere(entry => entry?.Deleted != false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NumberOfSetBits(this ulong i)
        {
            i -= (i >> 1) & 0x5555555555555555UL;
            i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this int value)
        {
            int mask = value >> 31;
            return (value + mask) ^ mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Abs(this long value)
        {
            long mask = value >> 63;
            return (value + mask) ^ mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(this uint value)
        {
            int digits = 1;
            if (value >= 100000)
            {
                value /= 100000;
                digits += 5;
            }

            if (value < 10)
            {
                // no-op
            }
            else if (value < 100)
            {
                digits++;
            }
            else if (value < 1000)
            {
                digits += 2;
            }
            else if (value < 10000)
            {
                digits += 3;
            }
            else
            {
                digits += 4;
            }

            return digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(this int value)
        {
            int absValue = Abs(value);

            int digits = 1;
            if (absValue >= 100000)
            {
                absValue /= 100000;
                digits += 5;
            }

            if (absValue < 10)
            {
                // no-op
            }
            else if (absValue < 100)
            {
                digits++;
            }
            else if (absValue < 1000)
            {
                digits += 2;
            }
            else if (absValue < 10000)
            {
                digits += 3;
            }
            else
            {
                digits += 4;
            }

            if (value < 0)
            {
                digits += 1; // negative
            }

            return digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DivRem(uint a, uint b, out uint result)
        {
            uint div = a / b;
            result = a - div * b;
            return div;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTimeStamp() => Core.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    }
}
