//Credits to bluecelcius in https://stackoverflow.com/questions/27778615/pseudo-random-number-generator-c-sharp
using System;
using System.Collections.Generic;
using System.IO;

namespace SakugaEngine.PRNG
{
    public class PRandom
    {
        int currentIndex = 0;
        List<int> num = new List<int>();
        public PRandom() { }

        public PRandom(int seed, int range)
        {
            currentIndex = 0;
            GenerateList(seed, 0, range);
        }
        public void GenerateList(int seed, int minimum, int range)
        {
            int tbSeed = seed;
            int min = minimum;
            int max = range;
            int number;
            string display = "";

            Random rand = new Random(tbSeed);

            num = new();

            for (int i = min; i < max; i++)
            {
                number = rand.Next(min, max);
                if (num.Contains(number))
                {
                    while (true)
                    {
                        number = rand.Next(min, max);
                        if (num.Contains(number))
                        {
                            // if exist do nothing and then random again while true  
                        }
                        else
                        {
                            num.Add(number);
                            break;
                        }
                    }
                }
                else
                {
                    num.Add(number);
                }

                display += "" + number + " ";
            }
            //Console.WriteLine("Your numbers: " + display + "\n");
        }
        public int Next()
        {
            int number = num[currentIndex++];
            if (currentIndex >= num.Count) currentIndex = 0;
            return number;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(currentIndex);
        }

        public void Deserialize(BinaryReader br)
        {
            currentIndex = br.ReadInt32();
        }
    }
}
