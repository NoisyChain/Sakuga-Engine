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
        public static uint GetChecksum(byte[] byteArray)
        {
            ushort[] newArray = new ushort[(int)Math.Ceiling(byteArray.Length / 2.0)];
            Buffer.BlockCopy(byteArray, 0, newArray, 0, byteArray.Length);
            //return FletcherChecksum(newArray, newArray.Length / 2);
            return (uint)FletcherChecksumAlt(byteArray, 32);
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
        //Another Fletcher's Checksum algorithm for testing
        /// <summary>
		/// Transforms byte array into an enumeration of blocks of 'blockSize' bytes
		/// </summary>
		/// <param name="inputAsBytes"></param>
		/// <param name="blockSize"></param>
		/// <returns></returns>
		private static IEnumerable<UInt64> Blockify(byte[] inputAsBytes, int blockSize)
		{
			int i = 0;

			//UInt64 used since that is the biggest possible value we can return.
			//Using an unsigned type is important - otherwise an arithmetic overflow will result
			UInt64 block = 0;
			
			//Run through all the bytes			
			while(i < inputAsBytes.Length)
			{
				//Keep stacking them side by side by shifting left and OR-ing				
				block = block << 8 | inputAsBytes[i];
				
				i++;
				
				//Return a block whenever we meet a boundary
				if(i % blockSize == 0 || i == inputAsBytes.Length)
				{
					yield return block;
					
					//Set to 0 for next iteration
					block = 0;
				}
			}
		}
		
		/// <summary>
		/// Get Fletcher's checksum, n can be either 16, 32 or 64
		/// </summary>
		/// <param name="inputWord"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		public static ulong FletcherChecksumAlt(byte[] inputAsBytes, int n)
		{
			//Fletcher 16: Read a single byte
			//Fletcher 32: Read a 16 bit block (two bytes)
			//Fletcher 64: Read a 32 bit block (four bytes)
			int bytesPerCycle = n / 16;
			
			//2^x gives max value that can be stored in x bits
			//no of bits here is 8 * bytesPerCycle (8 bits to a byte)
			ulong modValue = (ulong) (Math.Pow(2, 8 * bytesPerCycle) - 1);

			ulong sum1 = 0;
			ulong sum2 = 0;
			foreach (ulong block in Blockify(inputAsBytes, bytesPerCycle))
			{					
				sum1 = (sum1 + block) % modValue;
				sum2 = (sum2 + sum1) % modValue;
			}
				
			return sum1 + (sum2 * (modValue+1));
		}
    }
}
