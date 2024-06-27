using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// LocalSession implements a session for a single device to play locally.
    /// </summary>
    public class LocalSession : Session
    {
        internal protected override Device LocalDevice => _localDevice;

        internal protected override Device[] AllDevices => _allDevices;

        private readonly Device[] _allDevices;

        private Sync _sync;
        private Device _localDevice;

        public LocalSession(uint inputSize, uint deviceCount, uint totalPlayerCount) : base(inputSize, deviceCount, totalPlayerCount)
        {
            _allDevices = new Device[deviceCount];
            _sync = new Sync(_allDevices, inputSize);
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(deviceId >= 0 && deviceId < DeviceCount, $"DeviceId {deviceId} should be between [0,  {DeviceCount}[");
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_allDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local);
            _allDevices[deviceId] = LocalDevice;
            _sync.SetLocalDevice(deviceId, playerCount, frameDelay);
        }

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration) {}

        public override void Poll() {}

        public override bool IsRunning()
        {
            //return _allDevices.All(device => device.State == Device.DeviceState.Running);
            return LocalDevice.State == Device.DeviceState.Running;
        }

        public override List<SessionAction> AdvanceFrame(PlayerInputs[] localInput)
        {
            Debug.Assert(IsRunning(), "Session must be running before calling AdvanceFrame");
            Debug.Assert(localInput != null);

            return _sync.AdvanceSync(_localDevice.Id, localInput);
        }

        internal protected override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            return 1;
        }

        internal protected override void AddRemoteInput(uint deviceId, DeviceInputMessage message) {}

        public override int Frame() => _sync.Frame();
        public override int FrameAdvantage() => _sync.FramesAhead();
        public override uint RollbackFrames() => _sync.RollbackFrames();
    }
}
