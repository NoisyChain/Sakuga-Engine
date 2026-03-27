using Godot;

namespace SakugaEngine.Global
{
    public static class GlobalFunctions
    {
        public static Vector2 ToScaledVector2(Vector2I vector)
        {
            return new Vector2
            (
                vector.X / (float)GlobalVariables.SimulationScale,
                vector.Y / (float)GlobalVariables.SimulationScale
            );
        }
        public static Vector3 ToScaledVector3(Vector2I vector, float zScale = 0f)
        {
            return new Vector3
            (
                vector.X / (float)GlobalVariables.SimulationScale,
                vector.Y / (float)GlobalVariables.SimulationScale,
                zScale
            );
        }
        public static Vector3 ToScaledVector3(Vector3I vector)
        {
            return new Vector3
            (
                vector.X / (float)GlobalVariables.SimulationScale,
                vector.Y / (float)GlobalVariables.SimulationScale,
                vector.Z / (float)GlobalVariables.SimulationScale
            );
        }
        public static float ToScaledFloat(int value)
        {
            return value / (float)GlobalVariables.SimulationScale;
        }

        public static int IntLerp(int from, int to, int numberOfSteps, int currentStep)
        {
            return ((to - from) * currentStep) / numberOfSteps;
        }

        public static bool IsOnRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static Vector2I Distance(Vector2I a, Vector2I b)
        {
            int dx = b.X - a.X;
            int dy = b.Y - a.Y;

            if (b.X < a.X) dx = a.X - b.X;
            if (b.Y < a.Y) dx = a.Y - b.Y;

            return new Vector2I(dx, dy);
        }

        public static int HorizontalDistance(Vector2I a, Vector2I b)
        {
            int dx = a.X - b.X;
            int dy = 0;

            return dx * dx + dy * dy;
        }

        public static int VerticalDistance(Vector2I a, Vector2I b)
        {
            int dx = 0;
            int dy = a.Y - b.Y;

            return dx * dx + dy * dy;
        }

        public static float SineWaveFunction(float amp, float len, int time, float phase)
        {
            return amp * Mathf.Sin(len * ((float)time / GlobalVariables.TicksPerSecond) + phase);
        }

        public static string GetPlayerPrefix(int id)
        {
            switch (id)
            {
                case 0:
                    return "k1";
                case 1:
                    return"k2";
                case 2:
                    return "j1";
                case 3:
                    return "j2";
                default:
                    return "general";
            }
        }
    }
}
