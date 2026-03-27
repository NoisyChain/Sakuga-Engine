using MessagePack;

namespace PleaseResync
{
    [Union(0, typeof(DeviceSyncMessage))]
    [Union(1, typeof(DeviceSyncConfirmMessage))]
    [Union(2, typeof(DeviceInputMessage))]
    [Union(3, typeof(DeviceInputAckMessage))]
    [Union(4, typeof(HealthCheckMessage))]
    [Union(5, typeof(PingMessage))]
    [MessagePackObject]
    public abstract class DeviceMessage
    {
        [Key(0)]
        public uint SequenceNumber; // currently unused // TODO
    }

    [MessagePackObject]
    public class DeviceSyncMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;
        [Key(3)]
        public uint RandomRequest;

        public override string ToString() { return $"{typeof(DeviceSyncMessage)}: {new { DeviceId, PlayerCount, RandomRequest }}"; }
    }

    [MessagePackObject]
    public class DeviceSyncConfirmMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;
        [Key(3)]
        public uint RandomResponse;

        public override string ToString() { return $"{typeof(DeviceSyncConfirmMessage)}: {new { DeviceId, PlayerCount, RandomResponse }}"; }
    }

    [MessagePackObject]
    public class DeviceInputMessage : DeviceMessage
    {
        [Key(1)]
        public int Advantage;
        [Key(2)]
        public uint StartFrame;
        [Key(3)]
        public uint EndFrame;
        [Key(4)]
        public byte[] Input;

        public override string ToString() { return $"{typeof(DeviceInputMessage)}: {new { Advantage, StartFrame, EndFrame, Input }}"; }
    }

    [MessagePackObject]
    public class DeviceInputAckMessage : DeviceMessage
    {
        [Key(1)]
        public uint Frame;

        public override string ToString() { return $"{typeof(DeviceInputAckMessage)}: {new { Frame }}"; }
    }

    [MessagePackObject]
    public class HealthCheckMessage : DeviceMessage
    {
        [Key(1)]
        public int Frame;
        [Key(2)]
        public uint Checksum;

        public override string ToString() { return $"{typeof(HealthCheckMessage)}: {new { Frame, Checksum }}"; }
    }

    [MessagePackObject]
    public class PingMessage : DeviceMessage
    {
        [Key(1)]
        public uint PingTime;
        [Key(2)]
        public bool Returning;

        public override string ToString() { return $"{typeof(PingMessage)}: {new { PingTime, Returning }}"; }
    }
}
