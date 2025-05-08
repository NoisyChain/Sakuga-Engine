using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PleaseResync.Perf")]
[assembly: InternalsVisibleTo("PleaseResync.Test")]

namespace PleaseResync
{
    internal static class Platform
    {
        private readonly static Random RandomNumberGenerator = new Random();
        public enum DebugType{ Log, Warning, Error };

        public static uint GetCurrentTimeMS()
        {
            return (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static ushort GetRandomUnsignedShort()
        {
            return (ushort)RandomNumberGenerator.Next();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] CloneByteArray(byte[] array)
        {
            var newArray = new byte[array.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }
        public static uint GetChecksum(byte[] byteArray)
        {
            ushort[] newArray = new ushort[(int)Math.Ceiling(byteArray.Length / 2.0)];
            Buffer.BlockCopy(byteArray, 0, newArray, 0, byteArray.Length);
            //return FletcherChecksum(newArray, newArray.Length / 2);
            return FletcherChecksum(newArray, newArray.Length);
        }
        //https://en.wikipedia.org/wiki/Fletcher%27s_checksum
        public static uint FletcherChecksum(ushort[] data, int len)
        {
            uint sum1 = 0xffff, sum2 = 0xffff;
            int index  = 0;

            while (len > 0)
            {
                int tlen = len > 360 ? 360 : len;
                len -= tlen;
                do
                {
                    sum1 += data[index++];
                    sum2 += sum1;
                } while (--tlen > 0);

                sum1 = (sum1 & 0xffff) + (sum1 >> 16);
                sum2 = (sum2 & 0xffff) + (sum2 >> 16);
            }

            /* Second reduction step to reduce sums to 16 bits */
            sum1 = (sum1 & 0xffff) + (sum1 >> 16);
            sum2 = (sum2 & 0xffff) + (sum2 >> 16);
            return sum2 << 16 | sum1;
        }

        public static void Log(string message = "", DebugType type = DebugType.Log)
        {
            switch (type)
            {
                case DebugType.Log:
                    GD.Print(message);
                    break;
                case DebugType.Warning:
                    GD.PrintRich($"[color=YELLOW]{message}");
                    break;
                case DebugType.Error:
                    GD.PrintErr(message);
                    break;
            }
        }
    }
}
