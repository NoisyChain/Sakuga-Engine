using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace PleaseResync
{
    internal class Sync
    {
        private readonly uint _inputSize;
        private readonly Device[] _devices;

        private TimeSync _timeSync;
        private InputQueue[] _deviceInputs;
        private StateStorage _stateStorage;

        public Sync(Device[] devices, uint inputSize)
        {
            _devices = devices;
            _inputSize = inputSize;
            _timeSync = new TimeSync();
            _stateStorage = new StateStorage(TimeSync.MaxRollbackFrames);
            _deviceInputs = new InputQueue[_devices.Length];
        }

        public void AddRemoteInput(uint deviceId, int frame, PlayerInputs[] deviceInput)
        {
            // only allow adding input to the local device
            Debug.Assert(_devices[deviceId].Type == Device.DeviceType.Remote);
            // update device variables if needed
            if (_devices[deviceId].RemoteFrame < frame)
            {
                _devices[deviceId].RemoteFrame = frame;
                _devices[deviceId].RemoteFrameAdvantage = _timeSync.LocalFrame - frame;
                // let them know u recieved the packet
                _devices[deviceId].SendMessage(new DeviceInputAckMessage { Frame = (uint)frame });
            }
            AddDeviceInput(frame, deviceId, deviceInput);
        }

        public int FramesAhead()
        {
            return _timeSync.LocalFrameAdvantage;
        }

        public void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            _deviceInputs[deviceId] = new InputQueue(_inputSize, playerCount, frameDelay);
        }

        public void AddRemoteDevice(uint deviceId, uint playerCount)
        {
            _deviceInputs[deviceId] = new InputQueue(_inputSize, playerCount);
        }

        public List<SessionAction> AdvanceSync(uint localDeviceId, PlayerInputs[] deviceInput)
        {
            // should be called after polling the remote devices for their messages.
            Debug.Assert(deviceInput != null);

            bool isTimeSynced = _timeSync.IsTimeSynced(_devices);

            UpdateSyncFrame();

            var actions = new List<SessionAction>();

            // create savestate at the initialFrame to support rolling back to it
            // for example if initframe = 0 then 0 will be first save option to rollback to.
            if (_timeSync.LocalFrame == TimeSync.InitialFrame)
            {
                actions.Add(new SessionSaveGameAction(_timeSync.LocalFrame, _stateStorage));
            }

            // rollback update
            if (_timeSync.ShouldRollback())
            {
                actions.Add(new SessionLoadGameAction(_timeSync.SyncFrame, _stateStorage));
                for (int i = _timeSync.SyncFrame + 1; i <= _timeSync.LocalFrame; i++)
                {
                    actions.Add(new SessionAdvanceFrameAction(i, GetFrameInput(i).Inputs));
                    actions.Add(new SessionSaveGameAction(i, _stateStorage));
                }

                Godot.GD.Print($"Rollback detect from frame {_timeSync.SyncFrame + 1} to frame {_timeSync.LocalFrame} ({RollbackFrames() + 1} frames)");
            }

            if (isTimeSynced)
            {
                _timeSync.LocalFrame++;

                AddLocalInput(localDeviceId, deviceInput);
                SendLocalInputs(localDeviceId);

                actions.Add(new SessionAdvanceFrameAction(_timeSync.LocalFrame, GetFrameInput(_timeSync.LocalFrame).Inputs));
                actions.Add(new SessionSaveGameAction(_timeSync.LocalFrame, _stateStorage));   
            }

            return actions;
        }

        private void SendLocalInputs(uint localDeviceId)
        {
            foreach (var device in _devices)
            {
                if (device.Type == Device.DeviceType.Remote)
                {
                    //Using a somewhat fixed value for the starting frame to compensate packet loss
                    //TODO: replace it for something more optimized
                    uint limitFrames = TimeSync.MaxRollbackFrames - 1;
                    uint startingFrame = _timeSync.LocalFrame <= limitFrames ? 0 : (uint)_timeSync.LocalFrame - limitFrames;
                    uint finalFrame = (uint)(_timeSync.LocalFrame + _deviceInputs[localDeviceId].GetFrameDelay());

                    var combinedInput = new List<PlayerInputs>();

                    for (uint i = startingFrame; i <= finalFrame; i++)
                    {
                        combinedInput.AddRange(GetDeviceInput((int)i, localDeviceId).Inputs);
                    }

                    device.SendMessage(new DeviceInputMessage
                    {
                        StartFrame = startingFrame,
                        EndFrame = finalFrame,
                        Input = combinedInput.ToArray()
                    });
                }
            }
        }

        private void UpdateSyncFrame()
        {
            int finalFrame = _timeSync.RemoteFrame;
            if (_timeSync.RemoteFrame > _timeSync.LocalFrame)
            {
                finalFrame = _timeSync.LocalFrame;
            }
            bool foundMistake = false;
            int foundFrame = finalFrame;
            for (int i = _timeSync.SyncFrame + 1; i <= finalFrame; i++)
            {
                foreach (var input in _deviceInputs)
                {
                    var predInput = input.GetPredictedInput(i);
                    if (predInput.Frame == i &&
                        input.GetInput(i, false).Frame == i)
                    {
                        // Incorrect Prediction
                        if (!predInput.Equal(input.GetInput(i, false), true))
                        {
                            foundFrame = i - 1;
                            foundMistake = true;
                        }
                        // remove prediction form queue
                        input.ResetPrediction(i);
                    }
                }
                if (foundMistake) break;
            }
            _timeSync.SyncFrame = foundFrame;
        }

        private void AddLocalInput(uint deviceId, PlayerInputs[] deviceInput)
        {
            // only allow adding input to the local device
            Debug.Assert(_devices[deviceId].Type == Device.DeviceType.Local);
            AddDeviceInput(_timeSync.LocalFrame, deviceId, deviceInput);
        }
        
        private void AddDeviceInput(int frame, uint deviceId, PlayerInputs[] deviceInput)
        {
            Debug.Assert(deviceInput.Length == _devices[deviceId].PlayerCount * _inputSize,
             "the length of the given deviceInput isnt correct!");

            var input = new GameInput(frame, _inputSize, _devices[deviceId].PlayerCount);
            input.SetInputs(0, _devices[deviceId].PlayerCount, deviceInput);

            _deviceInputs[deviceId].AddInput(frame, input);
        }

        private GameInput GetDeviceInput(int frame, uint deviceId)
        {
            return _deviceInputs[deviceId].GetInput(frame);
        }

        public GameInput GetFrameInput(int frame)
        {
            uint playerCount = 0;
            foreach (var device in _devices)
            {
                playerCount += device.PlayerCount;
            }
            // add all device inputs into a single GameInput
            var input = new GameInput(frame, _inputSize, playerCount);
            // offset is needed to put the players input in the correct position
            uint playerOffset = 0;
            for (uint i = 0; i < _devices.Length; i++)
            {
                // get the input of the device and add it to the rest of the inputs
                var tmpInput = GetDeviceInput(frame, i);
                input.SetInputs(playerOffset, _devices[i].PlayerCount, tmpInput.Inputs);
                // advance player offset to the position of the next device
                playerOffset += _devices[i].PlayerCount;
            }
            return input;
        }

        public int Frame() => _timeSync.LocalFrame;
        public uint RollbackFrames() => (uint)(_timeSync.LocalFrame - (_timeSync.SyncFrame + 1));
    }
}