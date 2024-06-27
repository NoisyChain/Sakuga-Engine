using System.Diagnostics;

namespace PleaseResync
{
    internal class StateStorage
    {
        private readonly int _size;
        private readonly StateStorageEntry[] _states;

        public StateStorage(int maxRollbackFrames)
        {
            _size = maxRollbackFrames + 2; // +2 for extra space for now. might be removed later
            _states = new StateStorageEntry[_size];

            for (var i = 0; i < _size; i++)
            {
                _states[i] = new StateStorageEntry();
            }
        }

        public void SaveFrame(int frame, byte[] stateBuffer, uint stateChecksum = 0)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(stateBuffer != null, "Trying to save an empty state!");

            _states[frame % _size].Buffer = Platform.CloneByteArray(stateBuffer);
            _states[frame % _size].Checksum = stateChecksum;
        }

        public StateStorageEntry LoadFrame(int frame)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(_states[frame % _size] != null, "Trying to load an empty state!");

            return _states[frame % _size];
        }
    }

    internal class StateStorageEntry
    {
        public byte[] Buffer;
        public uint Checksum;
    }
}
