using System.IO;

namespace SakugaEngine
{
    public struct FighterButton
    {
        private int input;
        private int pressed;

        public void Parse(int newInput) { this.input = newInput; }
        public void Update() { this.pressed = this.input; }
        public bool wasPressed() { return this.input == 1 && this.pressed == 0; }
        public bool wasPressed(int direction) { return this.input == direction && this.pressed == 0; }
        public bool isBeingPressed() { return this.input == 1; }
        public bool isBeingPressed(int direction) { return this.input == direction; }
        public bool wasReleased() { return this.input == 0 && this.pressed == 1; }
        public bool wasReleased(int direction) { return this.input == 0 && this.pressed == direction; }
        public bool isPositive() { return this.input > 0; }
        public bool isNegative() { return this.input < 0; }
        public bool isNull() { return this.input == 0; }
        public bool Compare(int b) { return this.input == b; }
        public int Value() { return this.input; }

        public void Serialize(BinaryWriter bw) {
            bw.Write(pressed);
        }

        public void Deserialize(BinaryReader br) {
            pressed = br.ReadInt32();
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + pressed.GetHashCode();
            return hashCode;
        }
}
};
