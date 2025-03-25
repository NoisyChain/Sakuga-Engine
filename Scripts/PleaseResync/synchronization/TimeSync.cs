namespace PleaseResync
{
    internal class TimeSync
    {
        public const int InitialFrame = 0;
        public const int MaxRollbackFrames = 8;
        public const int FrameAdvantageLimit = 2;
        public const int HistorySize = 16;

        public int SyncFrame;
        public int LocalFrame;
        public int RemoteFrame;
        public int LocalFrameAdvantage;
        public int RemoteFrameAdvantage;
        public int FrameAdvantageDifference;

        public int[] LocalFrames;
        public int[] RemoteFrames;

        public TimeSync()
        {
            SyncFrame = InitialFrame;
            LocalFrame = InitialFrame;
            RemoteFrame = InitialFrame;
            RemoteFrameAdvantage = InitialFrame;
            LocalFrameAdvantage = 0;
            LocalFrames = new int[HistorySize];
            RemoteFrames = new int[HistorySize];
        }

        public bool IsTimeSynced(Device[] devices)
        {
            int minRemoteFrame = int.MaxValue;
            int maxRemoteFrameAdvantage = int.MinValue;

            foreach (var device in devices)
            {
                if (device.Type == Device.DeviceType.Remote)
                {
                    // find min remote frame
                    if (device.RemoteFrame < minRemoteFrame)
                    {
                        minRemoteFrame = device.RemoteFrame;
                    }
                    // find max frame advantage
                    if (device.RemoteFrameAdvantage > maxRemoteFrameAdvantage)
                    {
                        maxRemoteFrameAdvantage = device.RemoteFrameAdvantage;
                    }
                }
            }
            // Set variables
            RemoteFrame = minRemoteFrame;
            RemoteFrameAdvantage = maxRemoteFrameAdvantage;
            // How far the client is ahead of the last reported frame by the remote clients
            LocalFrameAdvantage = LocalFrame - RemoteFrame;
            FrameAdvantageDifference = LocalFrameAdvantage - RemoteFrameAdvantage;
            UpdateFrames(LocalFrame);
            // Only allow the local client to get so far ahead of remote.
            return LocalFrameAdvantage < MaxRollbackFrames && FrameAdvantageDifference <= FrameAdvantageLimit;
        }

        public bool ShouldRollback()
        {
            // No need to rollback if we don't have a frame after the previous sync frame to synchronize to.
            return LocalFrame > SyncFrame && RemoteFrame > SyncFrame;
        }

        void UpdateFrames(int frame)
        {
            int updateFrame = frame >= 0 ? frame : 0;

            LocalFrames[updateFrame % HistorySize] = LocalFrameAdvantage;

            /*i8 max = INT8_MIN;
            for (i8 num : _remote_frame_adv) {
                max = std::max(max, num);
            }*/
            RemoteFrames[updateFrame % HistorySize] = RemoteFrameAdvantage >= 0 ? RemoteFrameAdvantage : 0;
        }

        public float GetAverageFrameAdvantage()
        {
            float sumLocal = 0.0f;
            float sumRemote = 0.0f;

            for (int i = 0; i < HistorySize; i++) {
                sumLocal += LocalFrames[i];
                sumRemote += RemoteFrames[i];
            }

            float averageLocal = sumLocal / HistorySize;
            float averageRemote = sumRemote / HistorySize;

            // return the frames ahead
            return (averageLocal - averageRemote) / 2f;
        }
    }
}
