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

            for (int i = 0; i < bodies.Count; i++)
            {
                PhysicsBody bodyA = bodies[i];
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    PhysicsBody bodyB = bodies[j];

                    HitboxesCheck(bodyA, bodyB);
                }
            }
        }

        private void HitboxesCheck(PhysicsBody bodyA, PhysicsBody bodyB)
		{
			for (int i = 0; i < bodyA.Hitboxes.Length; i++)
			{	
				Collider hitboxA = bodyA.Hitboxes[i];
                if (!hitboxA.Active) continue;

				for (int j = 0; j < bodyB.Hitboxes.Length; j++)
				{
					Collider hitboxB = bodyB.Hitboxes[j];
                    if (!hitboxB.Active) continue;

					if (hitboxA.IsOverlapping(hitboxB))
					{
                        if (bodyA.HitConfirmed) return;
					    if (bodyB.HitConfirmed) return;

                        //Get current hitbox settings to reference the correct hitbox element
						HitboxElement boxSettingsA = bodyA.GetCurrentHitbox().Hitboxes[i];
						HitboxElement boxSettingsB = bodyB.GetCurrentHitbox().Hitboxes[j];

						int myType = (int)boxSettingsA.HitboxType;
						int otType = (int)boxSettingsB.HitboxType;

                        if (myType == 0 && otType == 0) continue;

                        Vector2I ContactPoint = GetContactPoint(hitboxA, hitboxB);

                        //Basic damage
						if (myType == 1 /*Hitbox*/  && otType == 0  /*Hurtbox*/ ) {
							bodyA.Parent.BaseDamage(boxSettingsA, ContactPoint);
						}
                        else if (otType == 1 /*Hitbox*/  && myType == 0  /*Hurtbox*/ ) {
							bodyB.Parent.BaseDamage(boxSettingsB, ContactPoint);
                        }
                        
                        //Hitboxes clash
						if (myType == 1 /*Hitbox*/ && otType == 1 /*Hitbox*/ ) {
							if (boxSettingsA.Priority == boxSettingsB.Priority)
							{
                            	bodyA.Parent.HitboxClash(boxSettingsA, ContactPoint, boxSettingsA.Priority);
                            	bodyB.Parent.HitboxClash(boxSettingsA, ContactPoint, boxSettingsB.Priority);
							}
						}

                        //Counterattack
                        if (myType == 5 /*Counter*/ && otType == 1 /*Hitbox*/) {
                            bodyA.Parent.CounterHit(boxSettingsA, ContactPoint);
                        }
                        else if (otType == 5 /*Counter*/ && myType == 1 /*Hitbox*/) {
                            bodyB.Parent.CounterHit(boxSettingsB, ContactPoint);
                        }

                        //Projectile damage
                        if (myType == 3 /*Projectile*/ && otType == 0 /*Hurtbox*/ ) {
                            bodyA.Parent.BaseDamage(boxSettingsA, ContactPoint);
                        }
                        else if (otType == 3 /*Projectile*/ && myType == 0 /*Hurtbox*/ ) {
                            bodyB.Parent.BaseDamage(boxSettingsB, ContactPoint);
                        }

                        //Projectile clash
                        if (myType == 3 /*Projectile*/ && otType == 3 /*Projectile*/ ) {
                            bodyA.Parent.ProjectileDamage(boxSettingsA, ContactPoint, boxSettingsA.Priority);
                            bodyB.Parent.ProjectileDamage(boxSettingsA, ContactPoint, boxSettingsB.Priority);
                        }
                        else if (otType == 3 /*Projectile*/ && myType == 3 /*Projectile*/ ) {
                            bodyB.Parent.ProjectileDamage(boxSettingsB, ContactPoint, boxSettingsB.Priority);
                            bodyA.Parent.ProjectileDamage(boxSettingsB, ContactPoint, boxSettingsA.Priority);
                        }

                        if (myType == 3 /*Projectile*/ && otType == 6 /*Deflect*/ ) {
                            //Deflect projectiles
                            bodyA.Parent.ProjectileDeflect(boxSettingsA);
                        }
                        else if (otType == 3 /*Projectile*/ && myType == 6 /*Deflect*/ ) {
                            //Deflect projectiles
                            bodyB.Parent.ProjectileDeflect(boxSettingsB);
                        }
                        
                        //Proximity block (Not working yet)
                        if (myType == 2 /*Proximity Block*/ && otType == 0 /*Hurtbox*/ ) {
                            //bodyB.Parent.ProximityBlock();
                        }
                        else if (otType == 2 /*Proximity Block*/ && myType == 0 /*Hurtbox*/ ) {
                            //bodyA.Parent.ProximityBlock();
                        }

						//Throw
                        if (myType == 4 /*Throw*/ && otType == 0 /*Hurtbox*/ ) {
                            bodyA.Parent.ThrowDamage(boxSettingsA);
                        }
                        else if (otType == 4 /*Throw*/ && myType == 0 /*Hurtbox*/ ) {
                            bodyB.Parent.ThrowDamage(boxSettingsB);
                        }

                        //Parry (needs implementation)
                        if (myType == 7 /*Parry*/ && otType == 1 /*Hitbox*/ ) {
							
                        }
                        else if (otType == 7 /*Parry*/ && myType == 1 /*Hitbox*/ ) {

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
