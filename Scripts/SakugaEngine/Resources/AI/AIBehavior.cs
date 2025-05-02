using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIBehavior : Resource
    {
        [ExportCategory("Blackboard")]
        /// <summary>
        /// A random threshold on which the CPU can make decisions when idle.
        /// </summary>
        [Export] public Vector2I DecisionRateFree;
        /// <summary>
        /// A random threshold on which the CPU can make decisions when doing an action.
        /// </summary>
        [Export] public Vector2I DecisionRateBusy;
        /// <summary>
        /// A random threshold to CPU inputs.
        /// </summary>
        [Export] public Vector2I InputRandomness;
        [Export] public int BlockingRate;
        [Export] public int TechingRate;
        [Export] public int PredictionQuality;
        /// <summary>
        /// If the character's health is smaller than this, the CPU will enter a defensive state.
        /// </summary>
        [Export] public int LowHealth = 4000;

        [ExportCategory("Behavior Tree")]
        [Export] public AIActionPack NearActions;
        [Export] public AIActionPack MidActions;
        [Export] public AIActionPack FarActions;
        [Export] public AIActionPack DistantActions;
    }
}
