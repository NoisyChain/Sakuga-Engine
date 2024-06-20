using Godot;
using System.Collections.Generic;

namespace SakugaEngine.Collision
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> bodies;
            
        public int BodyCount => bodies.Count;
        private uint CreatedBodies;

        private int MinSteps = 1;
        private int MaxSteps = 64;
        
        public PhysicsWorld()
        {
            bodies = new List<PhysicsBody>();
            CreatedBodies = 0;
        }
        
        public void AddBody(PhysicsBody newBody)
        {
            newBody.SetID(CreatedBodies);
            CreatedBodies++;
            bodies.Add(newBody);
        }
        
        public bool RemoveBody(PhysicsBody body)
        {
            return bodies.Remove(body);
        }

        public bool GetBody(int index, out PhysicsBody body)
        {
            body = null;

            if(index < 0 || index >= bodies.Count)
            {
                return false;
            }

            body = bodies[index];
            return true;
        }

        public void Simulate()
        {
            int Steps = Mathf.Clamp(Global.SubSteps, MinSteps, MaxSteps);

            for (int st = 0; st < Steps; st++)
            {
                for (int i = 0; i < bodies.Count; i++)
                    bodies[i].Move();
                
                for (int i = 0; i < bodies.Count; i++)
                {
                    PhysicsBody bodyA = bodies[i];
                    for (int j = i + 1; j < bodies.Count; j++)
                    {
                        PhysicsBody bodyB = bodies[j];
                        //if (i == j) continue;
                        
                        if (bodyA.IsStatic && bodyB.IsStatic) continue;
                        
                        if (bodyA.Pushbox.IsOverlapping(bodyB.Pushbox))
                        {
                            Vector2I depth = GetDepth(bodyA.Pushbox, bodyB.Pushbox);
                            Vector2I normal = Vector2I.Zero;
                            
                            if (bodyA.FixedPosition.X > bodyB.FixedPosition.X)
                            {
                                normal.X = -1;
                                //bodyA.PlayerSide = -1;
                                //bodyB.PlayerSide = 1;
                            }
                            else if (bodyA.FixedPosition.X < bodyB.FixedPosition.X)
                            {
                                normal.X = 1;
                                //bodyA.PlayerSide = 1;
                                //bodyB.PlayerSide = -1;
                            }
                            
                            //Vector2I pushDirectionA = new Vector2I(bodyA.PlayerSide, 0);
                            //Vector2I pushDirectionB = new Vector2I(bodyB.PlayerSide, 0);

                            if (bodyA.IsStatic)
                                bodyB.Resolve(-normal * depth);
                                //bodyB.Resolve(normal * depth);
                            else if (bodyB.IsStatic)
                                bodyA.Resolve(normal * depth);
                                //bodyA.Resolve(-normal * depth);
                            else
                            {
                                //bodyA.Resolve(-normal * (depth / Fix64.Two));
                                //bodyB.Resolve(normal * (depth / Fix64.Two));
                                if (Unpushable(bodyA, bodyB))
                                    bodyA.Resolve(normal * depth);
                                else
                                    bodyA.Resolve(normal * (depth / 2));

                                if (Unpushable(bodyB, bodyA))
                                    bodyB.Resolve(-normal * depth);
                                else
                                    bodyB.Resolve(-normal * (depth / 2));
                            }
                        }
                    }
                }
            }
        }

        public Vector2I GetContactPoint(Collider A, Collider B)
        {
            int minPointX = Mathf.Min(A.Center.X + A.Size.X, B.Center.X + B.Size.X);
            int maxPointX = Mathf.Max(A.Center.X - A.Size.X, B.Center.X - B.Size.X);
            int minPointY = Mathf.Min(A.Center.Y + A.Size.Y, B.Center.Y + B.Size.Y);
            int maxPointY = Mathf.Max(A.Center.Y - A.Size.Y, B.Center.Y - B.Size.Y);
            int mediantX = (minPointX + maxPointX) / 2;
            int mediantY = (minPointY + maxPointY) / 2;
            return new Vector2I(mediantX, mediantY);
        }

        public Vector2I GetDepth(Collider A, Collider B)
        {
            Vector2I length = A.Center - B.Center;

            Vector2I Depth = (A.Size + B.Size) / 2;
            Depth.X -= Mathf.Abs(length.X);
            Depth.Y -= Mathf.Abs(length.Y);

            return Depth;
        }

        private bool Unpushable(PhysicsBody bodyA, PhysicsBody bodyB)
        {
            bool LeftWall = bodyB.IsOnLeftWall && bodyA.FixedPosition.X >= bodyB.FixedPosition.X;
            bool RightWall = bodyB.IsOnRightWall && bodyA.FixedPosition.X <= bodyB.FixedPosition.X;
            return LeftWall || RightWall;
        }
    }
}
