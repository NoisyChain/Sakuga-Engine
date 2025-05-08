using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
//using Godot;

namespace PleaseResync
{
    /// <summary>
    /// Peer2PeerSession implements a session for devices wanting to play your game together via network.
    /// </summary>
    public class Peer2PeerSession : Session
    {
        internal protected override Device LocalDevice => _localDevice;
        internal protected override Device[] AllDevices => _allDevices;

        private readonly Device[] _allDevices;
        private List<Device> _spectators;
        private readonly SessionAdapter _sessionAdapter;

        private Sync _sync;
        private Device _localDevice;
        private uint _numSpectators;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount, bool offline, SessionAdapter adapter) : base(inputSize, deviceCount, totalPlayerCount, offline)
        {
            _allDevices = new Device[deviceCount];
            _spectators = [];
            _sessionAdapter = adapter;
            _sync = new Sync(_allDevices, inputSize, offline, _spectators);
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(deviceId >= 0 && deviceId < DeviceCount, $"DeviceId {deviceId} should be between [0,  {DeviceCount}[");
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_allDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local);
            _allDevices[deviceId] = LocalDevice;
            _sync.SetLocalDevice(deviceId, playerCount, frameDelay);

            if (OfflinePlay) _localDevice.State = Device.DeviceState.Running;
        }

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            Debug.Assert(deviceId >= 0 && deviceId < DeviceCount, $"DeviceId {deviceId} should be between [0,  {DeviceCount}[");
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(_allDevices[deviceId] == null, $"Remote device {deviceId} was already set.");

            _sessionAdapter.AddRemote(deviceId, remoteConfiguration);
            _allDevices[deviceId] = new Device(this, deviceId, playerCount, Device.DeviceType.Remote);
            _allDevices[deviceId].StartSyncing();
            _sync.AddRemoteDevice(deviceId, playerCount);
        }

        public override void AddSpectatorDevice(object remoteConfiguration)
        { 
            uint spectatorId = uint.MaxValue - _numSpectators;
            _sessionAdapter.AddRemote(spectatorId, remoteConfiguration);
            _spectators.Add(new Device(this, spectatorId, 0, Device.DeviceType.Spectator));
            Platform.Log($"New spectator created with Id: {spectatorId}");
            _spectators.Last().StartSyncing();
            _numSpectators++;
        }

        public override void Poll()
        {
            //We don't wanna deal with networking if we're playing offline
            if (OfflinePlay) return;

            Debug.Assert(_allDevices.All(device => device != null), "All devices must be Set/Added before calling Poll");

            if (!IsRunning())
            {
                foreach (var device in _allDevices)
                {
                    device.Sync();
                }

                foreach (var spectator in _spectators)
                {
                    spectator.Sync();
                }
            }

            _sync.LookForDisconnectedDevices();

            var messages = _sessionAdapter.ReceiveFrom();
            if (messages.Count == 0)
            {
                foreach (var device in _allDevices)
                {
                    if (device.Type == Device.DeviceType.Remote)
                        device.TestConnection();
                }

                foreach (var spectator in _spectators)
                {
                    spectator.TestConnection();
                }
                return;
            }

            foreach (var (_, deviceId, message) in messages)
            {
                //GD.Print($"Received message from remote device {deviceId}: {message}");
                if  (deviceId <= DeviceCount) {
                    _allDevices[deviceId].HandleMessage(message);
                } else {
                    int idx = (int)(uint.MaxValue - deviceId);
                    _spectators[idx].HandleMessage(message);
                }
            }
        }

        public override bool IsRunning()
        {
            return 
                _allDevices.All(device => device.State == Device.DeviceState.Running) &&
                _spectators.All(device => device.State != Device.DeviceState.Syncing);
        }

        public override List<SessionAction> AdvanceFrame(byte[] localInput)
        {
            Debug.Assert(IsRunning(), "Session must be running before calling AdvanceFrame");
            Debug.Assert(localInput != null);

            Poll();
            return _sync.AdvanceSync(_localDevice.Id, localInput);
        }

        internal protected override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            //Godot.GD.Print($"Sending message to remote device {deviceId}: {message}");
            return _sessionAdapter.SendTo(deviceId, message);
        }

        internal protected override void AddRemoteInput(uint deviceId, DeviceInputMessage message)
        {
            if (message == null) return;

            uint inputCount = (message.EndFrame - message.StartFrame) + 1;

            if (inputCount <= 0) return;
            
            uint inputSize = (uint)(message.Input.Length / inputCount);

            //Godot.GD.Print($"Recieved Inputs For Frames {message.StartFrame} to {message.EndFrame}. count: {inputCount}. size per input: {inputSize}");

            int inputIndex = 0;
            for (uint i = message.StartFrame; i <= message.EndFrame; i++)
            {
                byte[] inputsForFrame = new byte[message.Input.Length / inputCount];

                System.Array.Copy(message.Input, inputIndex * inputSize, inputsForFrame, 0, inputSize);
                _sync.AddRemoteInput(deviceId, (int)i, message.Advantage, inputsForFrame);

                inputIndex++;
            }
        }

        public override int Frame() => _sync.Frame();
        public override int RemoteFrame() => _sync.RemoteFrame();
        public override int FrameAdvantage() => _sync.FramesAhead();
        public override int RemoteFrameAdvantage() => _sync.FramesAhead();
        public override int FrameAdvantageDifference() => _sync.FrameDifference();
        public override uint RollbackFrames() => _sync.RollbackFrames();
        public override uint AverageRollbackFrames() => _sync.AverageRollbackFrames();
        public override int State() => (int)_sync.State();
    }
}