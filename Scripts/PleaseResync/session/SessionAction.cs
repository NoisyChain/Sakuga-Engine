using System.Diagnostics;

namespace PleaseResync
{
    /// <summary>
    /// SessionAction is an action you must fulfill to give a chance to the Session to synchronize with other sessions.
    /// </summary>
    public abstract class SessionAction
    {
        /// <summary>
        /// Frame this action refers to.
        /// </summary>
        public int Frame;
    }

    /// <summary>
    /// SessionLoadGameAction is an action you must fulfill when the Session needs your game to rollback to a previous frame.
    /// </summary>
    public class SessionLoadGameAction : SessionAction
    {
        private readonly StateStorage _storage;

        internal SessionLoadGameAction(int frame, StateStorage storage)
        {
            Debug.Assert(frame >= 0);

            Frame = frame;
            _storage = storage;
        }

        public byte[] Load()
        {
            return _storage.LoadFrame(Frame).Buffer;
        }

        public override string ToString() { return $"{typeof(SessionLoadGameAction)}: {new { Frame, _storage }}"; }
    }

    /// <summary>
    /// SessionSaveGameAction is an action you must fulfill when the Session needs to save your game state if it ever needs to rollback to that frame later.
    /// </summary>
    public class SessionSaveGameAction : SessionAction
    {
        private readonly StateStorage _storage;

        internal SessionSaveGameAction(int frame, StateStorage storage)
        {
            Debug.Assert(frame >= 0);

            Frame = frame;
            _storage = storage;
        }

        public void Save(byte[] stateBuffer, uint checksum = 0)
        {
            _storage.SaveFrame(Frame, stateBuffer, checksum);
        }

        public override string ToString() { return $"{typeof(SessionSaveGameAction)}: {new { Frame, _storage }}"; }
    }

    /// <summary>
    /// SessionAdvanceFrameAction is an action you must fulfill when the session needs the game to advance forward: either to perform a normal update or to resimulate an older frame.
    /// </summary>
    public class SessionAdvanceFrameAction : SessionAction
    {
        public byte[] Inputs;

        internal SessionAdvanceFrameAction(int frame, byte[] inputs)
        {
            Debug.Assert(inputs != null);

            Frame = frame;
            Inputs = inputs;
        }

        public override string ToString() { return $"{typeof(SessionAdvanceFrameAction)}: {new { Frame, Inputs }}"; }
    }
}
