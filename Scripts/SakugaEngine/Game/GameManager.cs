using Godot;
using System;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node
    {
        [Export] public PackedScene[] Spawns;
        public FighterBody[] Fighters;
        private PhysicsWorld World;

        public override void _Ready()
        {
            base._Ready();
            World = new PhysicsWorld();

            Fighters = new FighterBody[2];
            for (int i = 0; i < Spawns.Length; i++)
            {
                Node temp = Spawns[i].Instantiate();
                AddChild(temp);
                Fighters[i] = temp as FighterBody;
                World.AddBody(Fighters[i]);
                Fighters[i].Initialize(Global.StartingPosition * (-1 + (i * 2)));
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].ParseInputs(ReadInputs(i));
            
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Tick();
            
            World.Simulate();
        }

        public ushort ReadInputs(int id)
        {
            ushort input = 0;
            string prexif = "";

            switch (id)
            {
                case 0:
                    prexif = "k1";
                    break;
                case 1:
                    prexif = "k2";
                    break;
            }

            if (Input.IsActionPressed(prexif + "_up") && !Input.IsActionPressed(prexif + "_down"))
                    input |= Global.INPUT_UP;

                if (!Input.IsActionPressed(prexif + "_up") && Input.IsActionPressed(prexif + "_down"))
                    input |= Global.INPUT_DOWN;

                if (Input.IsActionPressed(prexif + "_left") && !Input.IsActionPressed(prexif + "_right"))
                    input |= Global.INPUT_LEFT;

                if (!Input.IsActionPressed(prexif + "_left") && Input.IsActionPressed(prexif + "_right"))
                    input |= Global.INPUT_RIGHT;

                if (Input.IsActionPressed(prexif + "_face_a"))
                    input |= Global.INPUT_FACE_A;

                if (Input.IsActionPressed(prexif + "_face_b"))
                    input |= Global.INPUT_FACE_B;

                if (Input.IsActionPressed(prexif + "_face_c"))
                    input |= Global.INPUT_FACE_C;

                if (Input.IsActionPressed(prexif + "_face_d"))
                    input |= Global.INPUT_FACE_D;

                /*if (Input.IsActionPressed("p1_macro_ab"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B;

                if (Input.IsActionPressed("p1_macro_ac"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_C;
                
                if (Input.IsActionPressed("p1_macro_bc"))
                    input |= Global.INPUT_FACE_B | Global.INPUT_FACE_C;

                if (Input.IsActionPressed("p1_macro_abc"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B | Global.INPUT_FACE_C;

                if (Input.IsActionPressed("p1_macro_abcd"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B | Global.INPUT_FACE_C | Global.INPUT_FACE_D;*/
            

            return input;
        }
    }
}
