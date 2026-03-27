using Godot;

namespace SakugaEngine
{
    public partial class SakugaNode : Node
	{
        [Export] protected bool StartActive;
        public bool IsActive;
        public virtual void Initialize()
        {
            IsActive = StartActive;
        }
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
		public virtual byte[] GetStateData(){ return []; }
        /// <summary>
        /// Use this to load data from the game state.
        /// </summary>
        /// <param name="br"></param>
		public virtual void SetStateData(byte[] stateBuffer){}
        /// <summary>
        /// Shows the simulation's final result in the view stage.
        /// </summary>
        public virtual void Render() {}
    }
}