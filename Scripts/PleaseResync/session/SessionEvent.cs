namespace PleaseResync
{
    public abstract class SessionEvent
    {
        public virtual string Desc() => "Base Session Event Class";
    }

    public class WaitSuggestionEvent : SessionEvent
    {
        public uint Frames;

        public override string Desc()
        {
            return "Wait Suggestion Event. Frames: " + Frames;
        }
    }

    public class DeviceSyncedEvent : SessionEvent
    {
        public uint DeviceId;
        public override string Desc()
        {
            return $"Device {DeviceId} Has Been Synced And Ready To Operate";
        }
    }
}
