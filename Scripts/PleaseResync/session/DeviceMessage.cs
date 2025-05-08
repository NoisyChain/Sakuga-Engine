using System.IO;

namespace PleaseResync
{

    public abstract class DeviceMessage
    {
        public uint SequenceNumber;
        public uint ID = 0;

        public abstract void Serialize(BinaryWriter bw);
        public abstract void Deserialize(BinaryReader br);
    }

    public class DeviceSyncMessage : DeviceMessage
    {
        public uint DeviceId;
        public uint PlayerCount;
        public uint RandomRequest;

        public DeviceSyncMessage(){ID = 1;}

        public DeviceSyncMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            bw.Write(DeviceId);
            bw.Write(PlayerCount);
            bw.Write(RandomRequest);
        }

        public override void Deserialize(BinaryReader br)
        {
            //ID = br.ReadUInt32();
            SequenceNumber = br.ReadUInt32();
            DeviceId = br.ReadUInt32();
            PlayerCount = br.ReadUInt32();
            RandomRequest = br.ReadUInt32();
        }
        public override string ToString() { return $"{typeof(DeviceSyncMessage)}: {new { DeviceId, PlayerCount, RandomRequest }}"; }
    }

    public class DeviceSyncConfirmMessage : DeviceMessage
    {
        public uint DeviceId;
        public uint PlayerCount;
        public uint RandomResponse;

        public DeviceSyncConfirmMessage(){ID = 2;}

        public DeviceSyncConfirmMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            bw.Write(DeviceId);
            bw.Write(PlayerCount);
            bw.Write(RandomResponse);
        }

        public override void Deserialize(BinaryReader br)
        {
            //ID = br.ReadUInt32();
            SequenceNumber = br.ReadUInt32();
            DeviceId = br.ReadUInt32();
            PlayerCount = br.ReadUInt32();
            RandomResponse = br.ReadUInt32();
        }
        public override string ToString() { return $"{typeof(DeviceSyncConfirmMessage)}: {new { DeviceId, PlayerCount, RandomResponse }}"; }
    }

    public class DeviceInputMessage : DeviceMessage
    {
        public uint StartFrame;
        public uint EndFrame;
        public int Advantage;
        public byte[] Input;

        public DeviceInputMessage(){ID = 3;}

        public DeviceInputMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            bw.Write(StartFrame);
            bw.Write(EndFrame);
            bw.Write(Advantage);
            bw.Write(Input.Length);
            for (int i = 0; i < Input.Length; i++)
                bw.Write(Input[i]);
        }

        public override void Deserialize(BinaryReader br)
        {
            //ID = br.ReadUInt32();
            SequenceNumber = br.ReadUInt32();
            StartFrame = br.ReadUInt32();
            EndFrame = br.ReadUInt32();
            Advantage = br.ReadInt32();
            Input = new byte[br.ReadInt32()];
            for (int i = 0; i < Input.Length; i++)
                Input[i] = br.ReadByte();
        }
        public override string ToString() { return $"{typeof(DeviceInputMessage)}: {new { StartFrame, EndFrame, Input }}"; }
    }

    public class DeviceInputAckMessage : DeviceMessage
    {
        public uint Frame;

        public DeviceInputAckMessage(){ID = 4;}

        public DeviceInputAckMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            bw.Write(Frame);
        }

        public override void Deserialize(BinaryReader br)
        {
            //ID = br.ReadUInt32();
            SequenceNumber = br.ReadUInt32();
            Frame = br.ReadUInt32();
        }
        public override string ToString() { return $"{typeof(DeviceInputAckMessage)}: {new { Frame }}"; }
    }

    public class HealthCheckMessage : DeviceMessage
    {
        public int Frame;
        public uint Checksum;

        public HealthCheckMessage(){ID = 5;}

        public HealthCheckMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            bw.Write(Frame);
            bw.Write(Checksum);
        }
        public override void Deserialize(BinaryReader br)
        {
            SequenceNumber = br.ReadUInt32();
            Frame = br.ReadInt32();
            Checksum = br.ReadUInt32();
        }

        public override string ToString() { return $"{typeof(HealthCheckMessage)}: {new { Frame, Checksum }}"; }
    }

    public class PingMessage : DeviceMessage
    {
        //public int Frame;
        public uint PingTime;
        public bool Returning;

        public PingMessage(){ID = 6;}

        public PingMessage(BinaryReader br)
        {
            Deserialize(br);
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(SequenceNumber);
            //bw.Write(Frame);
            bw.Write(PingTime);
            bw.Write(Returning);
        }
        public override void Deserialize(BinaryReader br)
        {
            SequenceNumber = br.ReadUInt32();
            //Frame = br.ReadInt32();
            PingTime = br.ReadUInt32();
            Returning = br.ReadBoolean();
        }

        public override string ToString() { return $"{typeof(PingMessage)}: {new { PingTime, Returning }}"; }
    }
}
