using System;

namespace SakugaEngine.Global
{
    public static class RNG
    {
        public static string baseSeed = "Sakuga Engine"; //You can change this if you want

        public static Random random;

        public static void Set(int seed)
        {
            random = new Random(seed);
        }

        public static int Next()
        {
            return random.Next();
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
