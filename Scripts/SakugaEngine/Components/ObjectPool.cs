using Godot;
using SakugaEngine.Game;
using System;
using System.Collections.Generic;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class ObjectPool : Node
    {
        private List<PoolReference> SpawnReferences = new List<PoolReference>();
        private List<VFXPoolReference> VFXReferences = new List<VFXPoolReference>();

        public void Initialize(SakugaActor owner)
        {
            // Initialize Spawns
            CreateSpawnablePool(owner);
            
            // Initialize VFX
            CreateVFXPool(owner);
        }

        private void CreateSpawnablePool(SakugaActor owner)
        {
            if (owner.SpawnablesList == null) return;

            var SpawnsPool = owner.SpawnablesList;
            for (int i = 0; i < SpawnsPool.SpawnObjects.Length; i++)
            {
                SpawnReferences.Add(new PoolReference{
                    Key = SpawnsPool.SpawnObjects[i].Key,
                    Objects = new SakugaActor[SpawnsPool.SpawnObjects[i].PoolSize]
                });
                for (int j = 0; j < SpawnReferences[i].Objects.Length; j++)
                {   
                    Node node = SpawnsPool.SpawnObjects[i].Instance.Instantiate();
                    node.Name = owner.playerID + "_" + SpawnsPool.SpawnObjects[i].Key + "_" + j;
                    SpawnReferences[i].Objects[j] = node as SakugaActor;
                    
                    SpawnReferences[i].Objects[j].Inputs = owner.Inputs;
                    SpawnReferences[i].Objects[j].playerID = owner.playerID;
                    SpawnReferences[i].Objects[j].Initialize();
                    SpawnReferences[i].Objects[j].SetMaster(owner);
                    SpawnReferences[i].Objects[j].SetAllies(owner.GetAllies());
                    SpawnReferences[i].Objects[j].SetOpponents(owner.GetOpponents());
                    GameManager.Instance.AddActor(SpawnReferences[i].Objects[j]);
                    //SpawnReferences[i].Objects[j].IsActive = false;
                    
                    //GD.Print("Element " + References[i].Objects[j].Name + " created");
                }
            }
        }

        private void CreateVFXPool(SakugaActor owner)
        {
            if (owner.VFXList == null) return;

            var vfxPool = owner.VFXList;
            for (int i = 0; i < vfxPool.SpawnObjects.Length; i++)
            {
                VFXReferences.Add(new VFXPoolReference{
                    Key = vfxPool.SpawnObjects[i].Key,
                    Objects = new SakugaVFX[vfxPool.SpawnObjects[i].PoolSize]
                });
                for (int j = 0; j < VFXReferences[i].Objects.Length; j++)
                {   
                    Node node = vfxPool.SpawnObjects[i].Instance.Instantiate();
                    node.Name = owner.playerID + "_" + vfxPool.SpawnObjects[i].Key + "_" + j;
                    VFXReferences[i].Objects[j] = node as SakugaVFX;
                    
                    VFXReferences[i].Objects[j].Initialize();
                    VFXReferences[i].Objects[j].SetMaster(owner);
                    GameManager.Instance.AddActor(VFXReferences[i].Objects[j], false);
                    //VFXReferences[i].Objects[j].IsActive = false;
                    
                    //GD.Print("VFX Element " + VFXReferences[i].Objects[j].Name + " created");
                }
            }
        }

        public void Reset()
        {
            foreach (PoolReference pool in SpawnReferences)
            {
                foreach (SakugaActor obj in pool.Objects)
                {
                    obj.Reset();
                }
            }

            foreach (VFXPoolReference pool in VFXReferences)
            {
                foreach (SakugaVFX obj in pool.Objects)
                {
                    obj.Reset();
                }
            }
        }

        public void SpawnObject(string key, int initialState, Vector2I position)
        {
            if (SpawnReferences.Count == 0) return;
            PoolReference spawnElement = GetSpawnReference(key);
            SpawnObjectCommon(spawnElement, initialState, position);
            //GD.Print(objectToSpawn.Name + " Spawned!");
        }

        public void SpawnObject(int index, int initialState, Vector2I position)
        {
            if (SpawnReferences.Count == 0) return;
            PoolReference spawnElement = GetSpawnReference(index);
            SpawnObjectCommon(spawnElement, initialState, position);
            //GD.Print(objectToSpawn.Name + " Spawned!");
        }

        private void SpawnObjectCommon(PoolReference reference, int initialState, Vector2I position)
        {
            SakugaActor objectToSpawn = GetNextInactiveElement(reference);
            objectToSpawn.StateManager.PlayState(initialState, true);
            objectToSpawn.StateManager.CurrentStateFrame = -1;
            objectToSpawn.Body.MoveTo(position);
            objectToSpawn.Body.UpdateSide(objectToSpawn.GetMaster().Body.IsLeftSide);
            objectToSpawn.Body.FlipSide();
            objectToSpawn.IsActive = true;
        }

        public PoolReference GetSpawnReference(string key)
        {
            foreach(PoolReference reference in SpawnReferences)
            {
                if (reference.Key != key) continue;

                return reference;
            }
            return new PoolReference();
        }

        public PoolReference GetSpawnReference(int index)
        {
            return SpawnReferences[index];
        }

        public SakugaActor GetActiveSpawn(PoolReference reference)
        {
            for (int i = 0; i < reference.Objects.Length; i++)
            {
                if (!reference.Objects[i].IsActive) continue;

                return reference.Objects[i];
            }
            return null;
        }

        public SakugaActor GetActiveSpawnByIndex(int index)
        {
            PoolReference reference = GetSpawnReference(index);
            return GetActiveSpawn(reference);
        }

        public SakugaActor GetNextInactiveElement(PoolReference reference)
        {
            for (int i = 0; i < reference.Objects.Length; i++)
            {
                if (reference.Objects[i].IsActive) continue;

                return reference.Objects[i];
            }
            return reference.Objects[0];
        }

        public void SpawnVFX(string key, Vector2I position, int side, bool attached)
        {
            if (VFXReferences.Count == 0) return;
            VFXPoolReference spawnElement = GetVFXReference(key);
            SakugaVFX objectToSpawn = GetNextInactiveVFXElement(spawnElement);
            objectToSpawn.Spawn(position, side, attached);

            //GD.Print(objectToSpawn.Name + " Spawned!");
        }

        public void SpawnVFX(int index, Vector2I position, int side, bool attached)
        {
            if (VFXReferences.Count == 0) return;
            VFXPoolReference spawnElement = GetVFXReference(index);
            SakugaVFX objectToSpawn = GetNextInactiveVFXElement(spawnElement);
            objectToSpawn.Spawn(position, side, attached);

            //GD.Print(objectToSpawn.Name + " Spawned!");
        }

        public VFXPoolReference GetVFXReference(string key)
        {
            foreach(VFXPoolReference reference in VFXReferences)
            {
                if (reference.Key != key) continue;

                return reference;
            }
            return new VFXPoolReference();
        }

        public VFXPoolReference GetVFXReference(int index)
        {
            return VFXReferences[index];
        }
        public SakugaVFX GetNextInactiveVFXElement(VFXPoolReference reference)
        {
            for (int i = 0; i < reference.Objects.Length; i++)
            {
                if (reference.Objects[i].IsActive) continue;

                return reference.Objects[i];
            }
            return reference.Objects[0];
        }
    }

    [Serializable]
    public struct PoolReference
    {
        public string Key;
        public SakugaActor[] Objects;
    };

    [Serializable]
    public struct VFXPoolReference
    {
        public string Key;
        public SakugaVFX[] Objects;
    };
}
