using System;
using System.Collections.Generic;

namespace PleaseResync
{
    public static class Fletcher
    {
        public static ulong FletcherChecksum(byte[] inputAsBytes, int n)
        {
            //Fletcher 16: Read a single byte
            //Fletcher 32: Read a 16 bit block (two bytes)
            //Fletcher 64: Read a 32 bit block (four bytes)
            int bytesPerCycle = n / 16;
            
            //2^x gives max value that can be stored in x bits
            //no of bits here is 8 * bytesPerCycle (8 bits to a byte)
            ulong modValue = (ulong)(Math.Pow(2, 8 * bytesPerCycle) - 1);
                        
            //ASCII encoding conveniently gives us 1 byte per character
            ulong sum1 = 0;
            ulong sum2 = 0;
            foreach (ulong block in Blockify(inputAsBytes, bytesPerCycle))
            {					
                sum1 = (sum1 + block) % modValue;
                sum2 = (sum2 + sum1) % modValue;
            }
                
            return sum1 + (sum2 * (modValue+1));
        }

        private static IEnumerable<ulong> Blockify(byte[] inputAsBytes, int blockSize)
        {
            int i = 0;

            //UInt64 used since that is the biggest possible value we can return.
            //Using an unsigned type is important - otherwise an arithmetic overflow will result
            ulong block = 0;
            
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
    }
}