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
        public enum DebugType{ Log, Warning, Error};

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
        public static void GodotLog(string message = "", DebugType type = DebugType.Log)
        {
            switch (type)
            {
                case DebugType.Log:
                    GD.Print(message);
                    break;
                case DebugType.Warning:
                    GD.Print(message);
                    break;
                case DebugType.Error:
                    GD.PrintErr(message);
                    break;
            }
        }
    }
}
