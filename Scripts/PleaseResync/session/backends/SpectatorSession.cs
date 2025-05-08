using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PleaseResync
{
    public class SpectatorSession : Session
    {
        public class BroadcastStream
        {
            public readonly uint InputSize;
            private readonly int _initialFrameBuffer;
            private int _currentFrame, _availableFrame;
            private readonly List<byte[]> _frameInputs;

            public BroadcastStream(int initialBuffer = 30, uint inputSize = 1)
            {
                InputSize = inputSize;
                _initialFrameBuffer = initialBuffer;
                _currentFrame = 0;
                _availableFrame = -1;
                _frameInputs = new List<byte[]>(_initialFrameBuffer);
            }

            public void AddFrameInput(int frame, byte[] input)
            {
                // we only accept the inputs for the next frame in order.
                if (frame != _availableFrame + 1)
                {
                    return;
                }
                // add the input to the buffer
                _frameInputs.Add(new byte[InputSize]);
                Array.Copy(input, _frameInputs.Last(), InputSize);
                // be ready to accept the following frames.
                _availableFrame++;
            }

            public bool GetFrameInput(out int frame, out byte[] input)
            {
                frame = 0;
                input = null;
                // we want our buffer to build up first for smooth broadcasting
                if (_availableFrame <= _initialFrameBuffer)
                {
                    return false;
                }
                // we can only fetch the input if its available in the buffer
                if (_currentFrame > _availableFrame)
                {
                    return false;
                }
                // copy the input from the buffer into the input array
                frame = _currentFrame;
                input = new byte[InputSize];
                Array.Copy(_frameInputs[_currentFrame], input, InputSize);
                // advance to the next broadcast frame
                _currentFrame++;
                return true;
            }
        }

        public SpectatorSession(uint inputSize, uint playerCount, SessionAdapter adapter, uint frameDelay = 30)
            : base(inputSize, 1, playerCount, false)
        {
            // we only need the remote device to create a spectator session.
            // the remote device is the broadcast device
            // which will provide all the needed inputs to proceed.
            _sessionAdapter = adapter;
            _broadcastStream = new BroadcastStream((int)frameDelay, inputSize * playerCount);
        }
        protected internal override Device LocalDevice => null;

        protected internal override Device[] AllDevices => throw new System.NotImplementedException();

        private Device _broadcastDevice = null;
        private BroadcastStream _broadcastStream = null;
        private readonly SessionAdapter _sessionAdapter;

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            Debug.Assert(_broadcastDevice == null, "Broadcast device was already set.");
            _sessionAdapter.AddRemote(deviceId, remoteConfiguration);
            _broadcastDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Remote);
            _broadcastDevice.StartSyncing();
        }

        public override List<SessionAction> AdvanceFrame(byte[] localInput = null)
        {
            Debug.Assert(IsRunning(), "Session must be running before calling AdvanceFrame");
            Poll();

            var actions = new List<SessionAction>();
            if (_broadcastStream.GetFrameInput(out int frame, out byte[] input))
                actions.Add(new SessionAdvanceFrameAction(frame, input));
            return actions;
        }

        public override uint AverageRollbackFrames()
        {
            throw new System.NotImplementedException();
        }

        public override int Frame()
        {
            throw new System.NotImplementedException();
        }

        public override int FrameAdvantage()
        {
            throw new System.NotImplementedException();
        }

        public override int FrameAdvantageDifference()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsRunning()
        {
            return _broadcastDevice.State == Device.DeviceState.Running;
        }

        public override void Poll()
        {
            if (!IsRunning())
            {
                _broadcastDevice.Sync();
            }

            var messages = _sessionAdapter.ReceiveFrom();
            if (messages.Count == 0)
            {
                _broadcastDevice.TestConnection();
                return;
            }

            foreach (var (_, deviceId, message) in messages)
            {
                if (deviceId != _broadcastDevice.Id) continue; // discard messages from other devices
                _broadcastDevice.HandleMessage(message);
            }
        }

        public override int RemoteFrame()
        {
            throw new System.NotImplementedException();
        }

        public override int RemoteFrameAdvantage()
        {
            throw new System.NotImplementedException();
        }

        public override uint RollbackFrames()
        {
            throw new System.NotImplementedException();
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            throw new System.NotImplementedException();
        }

        public override int State()
        {
            throw new System.NotImplementedException();
        }

        protected internal override void AddRemoteInput(uint deviceId, DeviceInputMessage message)
        {
            if (deviceId != _broadcastDevice.Id) return; // discard messages from other devices
            uint count = (message.EndFrame - message.StartFrame) + 1;
            int inpSize = (int)_broadcastStream.InputSize;
            //GD.Print($"Recieved Inputs For Frames {message.StartFrame} to {message.EndFrame} length: {message.Input.Length}, count: {count}, size per input: {inpSize}");
            for (int i = 0; i < count; i++)
            {
                int newFrame = (int)(i + message.StartFrame);
                _broadcastStream.AddFrameInput(newFrame, [.. message.Input.Skip(i * inpSize)]);
                SendMessageTo(_broadcastDevice.Id, new DeviceInputAckMessage { Frame = (uint)newFrame });
            }
        }

        protected internal override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            if (deviceId != _broadcastDevice.Id) return 0; // dont send messages to other devices
            return _sessionAdapter.SendTo(deviceId, message);
        }

        public override void AddSpectatorDevice(object remoteConfiguration)
        {
            throw new NotImplementedException();
        }
    }
}