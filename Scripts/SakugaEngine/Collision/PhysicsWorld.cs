using Godot;
using SakugaEngine.Resources;
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
                        
                        if (bodyA.IsStatic && bodyB.IsStatic) continue;
                        
                        if (bodyA.Pushbox.IsOverlapping(bodyB.Pushbox))
                        {
                            Vector2I depth = GetDepth(bodyA.Pushbox, bodyB.Pushbox);
                            Vector2I normal = Vector2I.Zero;
                            
                            if (bodyA.FixedPosition.X > bodyB.FixedPosition.X)
                                normal.X = -1;
                            else if (bodyA.FixedPosition.X < bodyB.FixedPosition.X)
                                normal.X = 1;
                            else
                                normal.X = bodyA.IsLeftSide ? 1 : -1;

                            if (bodyA.IsStatic)
                                bodyB.Resolve(-normal * depth);
                            else if (bodyB.IsStatic)
                                bodyA.Resolve(normal * depth);
                            else
                            {
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
            //Setup for proximity blocking
            for (int i = 0; i < bodies.Count; i++)
                bodies[i].ProximityBlocked = false;

            for (int i = 0; i < bodies.Count; i++)
            {
                PhysicsBody bodyA = bodies[i];
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    PhysicsBody bodyB = bodies[j];

                    HitboxesCheck(bodyA, bodyB);
                }
            }

            //Check proximity blocking exit
            for (int i = 0; i < bodies.Count; i++)
                if (!bodies[i].ProximityBlocked)
                    bodies[i].Parent.OnHitboxExit();
        }

        private void HitboxesCheck(PhysicsBody bodyA, PhysicsBody bodyB)
		{
			for (int i = 0; i < bodyA.Hitboxes.Length; i++)
			{	
				Collider hitboxA = bodyA.Hitboxes[i];

				for (int j = 0; j < bodyB.Hitboxes.Length; j++)
				{
					Collider hitboxB = bodyB.Hitboxes[j];

                    if (bodyA.HitConfirmed) return;
			        if (bodyB.HitConfirmed) return;

					if (hitboxA.IsOverlapping(hitboxB))
					{
                        //Get current hitbox settings to reference the correct hitbox element
						HitboxElement boxSettingsA = bodyA.GetCurrentHitbox().Hitboxes[i];
						HitboxElement boxSettingsB = bodyB.GetCurrentHitbox().Hitboxes[j];

						int myType = (int)boxSettingsA.HitboxType;
						int otType = (int)boxSettingsB.HitboxType;

                        if (myType == 0 && otType == 0) continue;

                        SakugaActor body1 = bodyA.Parent as SakugaActor;
                        SakugaActor body2 = bodyB.Parent as SakugaActor;

                        Vector2I ContactPoint = GetContactPoint(hitboxA, hitboxB);

                        //Basic damage
						if (myType == 1 /*Hitbox*/  && otType == 0  /*Hurtbox*/ ) {
                            if (!body1.AllowHitCheck(body2)) return;

                            bodyA.Parent.BaseDamage(body2, boxSettingsA, ContactPoint);
						}
                        else if (otType == 1 /*Hitbox*/  && myType == 0  /*Hurtbox*/ ) {
                            if (!body2.AllowHitCheck(body1)) return;

                            bodyB.Parent.BaseDamage(body1, boxSettingsB, ContactPoint);
                        }
                        
                        //Hitboxes clash
						if (myType == 1 /*Hitbox*/ && otType == 1 /*Hitbox*/ ) {
							if (boxSettingsA.Priority != boxSettingsB.Priority) return;

                            bodyA.Parent.HitboxClash(boxSettingsA, ContactPoint);
                            bodyB.Parent.HitboxClash(boxSettingsB, Vector2I.Zero);
						}

                        //Projectile damage
                        if (myType == 3 /*Projectile*/ && otType == 0 /*Hurtbox*/ ) {
                            if (!body1.AllowHitCheck(body2)) return;

                            bodyA.Parent.BaseDamage(body2, boxSettingsA, ContactPoint);
                        }
                        else if (otType == 3 /*Projectile*/ && myType == 0 /*Hurtbox*/ ) {
                            if (!body2.AllowHitCheck(body1)) return;

                            bodyB.Parent.BaseDamage(body1, boxSettingsB, ContactPoint);
                        }

                        //Projectile clash
                        if (myType == 3 /*Projectile*/ && otType == 3 /*Projectile*/ ) {
                            if (boxSettingsB.Priority >= boxSettingsA.Priority)
                                bodyA.Parent.ProjectileClash(boxSettingsA, ContactPoint);
                            if (boxSettingsA.Priority >= boxSettingsB.Priority)
                                bodyB.Parent.ProjectileClash(boxSettingsB, ContactPoint);
                        }

                        //Deflect projectiles
                        if (myType == 3 /*Projectile*/ && otType == 6 /*Deflect*/ ) {
                            if (!body1.AllowHitCheck(body2)) return;
                            bodyA.Parent.ProjectileDeflect(body2, boxSettingsA, ContactPoint);
                        }
                        else if (otType == 3 /*Projectile*/ && myType == 6 /*Deflect*/ ) {
                            if (!body2.AllowHitCheck(body1)) return;
                            bodyB.Parent.ProjectileDeflect(body1, boxSettingsB, ContactPoint);
                        }
                        
                        //Proximity block
                        if (myType == 2 /*Proximity Block*/ && otType == 0 /*Hurtbox*/ ) {
                            bodyB.ProximityBlocked = true;
                            bodyB.Parent.ProximityBlock();
                        }
                        else if (otType == 2 /*Proximity Block*/ && myType == 0 /*Hurtbox*/ ) {
                            bodyA.ProximityBlocked = true;
                            bodyA.Parent.ProximityBlock();
                        }

						//Throws
                        if (myType == 4 /*Throw*/ && otType == 0 /*Hurtbox*/ ) {
                            bodyA.Parent.ThrowDamage(body2, boxSettingsA, ContactPoint);
                        }
                        else if (otType == 4 /*Throw*/ && myType == 0 /*Hurtbox*/ ) {
                            bodyB.Parent.ThrowDamage(body1, boxSettingsB, ContactPoint);
                        }

                        //Counterattack (Not implemented yet)
                        if (myType == 5 /*Counter*/ && otType == 1 /*Hitbox*/) {
                            bodyA.Parent.CounterHit(body2, boxSettingsA, ContactPoint);
                        }
                        else if (otType == 5 /*Counter*/ && myType == 1 /*Hitbox*/) {
                            bodyB.Parent.CounterHit(body1, boxSettingsB, ContactPoint);
                        }

                        //Parry (Not implemented yet)
                        if (myType == 7 /*Parry*/ && otType == 1 /*Hitbox*/ ) {
							bodyA.Parent.ParryHit(body2, boxSettingsA, ContactPoint);
                        }
                        else if (otType == 7 /*Parry*/ && myType == 1 /*Hitbox*/ ) {
                            bodyB.Parent.ParryHit(body1, boxSettingsB, ContactPoint);
                        }
						//GD.Print("Hitbox Collided at " + (Vector2)hitboxA.ContactPoint);
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
            int MedianX = (minPointX + maxPointX) / 2;
            int MedianY = (minPointY + maxPointY) / 2;
            return new Vector2I(MedianX, MedianY);
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
