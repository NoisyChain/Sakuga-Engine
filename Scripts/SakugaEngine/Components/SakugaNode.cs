using Godot;
using System.IO;

namespace SakugaEngine
{
    public partial class SakugaNode : Node3D
	{
        /// <summary>
        /// Executed before everything in the loop.
        /// </summary>
        public virtual void PreTick(){}
        /// <summary>
        /// The main loop.
        /// </summary>
		public virtual void Tick(){}
        /// <summary>
        /// Executed after everything is finished.
        /// </summary>
		public virtual void LateTick(){}
		/// <summary>
        /// Use this to save data in the game state.
        /// </summary>
        /// <param name="bw"></param>
		public virtual void Serialize(BinaryWriter bw){}
        /// <summary>
        /// Use this to load data from the game state.
        /// </summary>
        /// <param name="br"></param>
		public virtual void Deserialize(BinaryReader br){}
        /// <summary>
        /// Shows the simulation's final result in the view stage.
        /// </summary>
        public virtual void Render() {}
    }
}