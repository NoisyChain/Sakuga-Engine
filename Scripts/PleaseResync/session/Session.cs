using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System;

namespace PleaseResync
{
    /// <summary>
    /// Session is responsible for managing a pool of devices wanting to play your game together.
    /// Each device must create a Session on their end and make sure all devices IDs, numbers of players for each device, are the same on every device.
    /// <br />
    /// <listheader>Terminology</listheader>
    /// <list type="bullet">
    ///     <item>Frame: a single update of your game logic, to simulate correctly a frame of your game, you will need all player inputs for that frame.</item>
    ///     <item>Device: a device where your game runs (computer, xbox, ...) where one or more players can play your game.</item>
    ///     <item>Player: a human (or AI) on a device producing inputs to change the state of the game.</item>
    ///     <item>DeviceID: an unique number used to uniquely identify a device, starting at zero (eg. 0 = John's Xbox, 1 = Tom's Switch)</item>
    ///     <item>Rollback: while the Session is waiting for remote device inputs, it may use placeholder inputs for that your game can continue, but if the remote player inputs are different, we need to resimulate the game from where we left with the correct inputs: that's a rollback</item>
    ///     <item>PlayerInput: a piece of information describing how a player would like to change the state of the game for a given frame. (eg. left = false, right = true, jumping = true)</item>
    ///     <item>PlayerCount: a number of players on a given device, usually one for most of the games, but can be greater for a game with multiple local players (eg. splitscreen).</item>
    /// </list>
    /// </summary>
    public abstract class Session
    {
        public const uint LIMIT_INPUT_SIZE = 32;
        public const uint LIMIT_DEVICE_COUNT = 4;
        public const uint LIMIT_TOTAL_PLAYER_COUNT = 24;

        /// <summary>
        /// connectionState returns the connection between the devices.
        /// </summary>
        
        /// <summary>
        /// OfflinePlay defines if it's a network game or not.
        /// </summary>
        protected bool OfflinePlay;
        /// <summary>
        /// InputSize is the size in bits of the input for one player.
        /// </summary>
        protected uint InputSize;
        /// <summary>
        /// DeviceCount is the number of devices taking part in this session.
        /// </summary>
        protected readonly uint DeviceCount;
        /// <summary>
        /// TotalPlayerCount is the total number of players accross all devices taking part in this session.
        /// </summary>
        protected readonly uint TotalPlayerCount;

        /// <summary>
        /// LocalDevice represents the device that is local to this Session.
        /// </summary>
        internal protected abstract Device LocalDevice { get; }
        /// <summary>
        /// AllDevices is an array of every devices (local and remotes) taking part in this Session, indexed by their device ID.
        /// </summary>
        internal protected abstract Device[] AllDevices { get; }

        /// <param name="inputSize">The size in bits of the input for one player.</param>
        /// <param name="deviceCount">The number of devices taking part in this session.</param>
        /// <param name="totalPlayerCount">The total number of players accross all devices taking part in this session.</param>
        public Session(uint inputSize, uint deviceCount, uint totalPlayerCount, bool offline)
        {
            Debug.Assert(inputSize > 0);
            Debug.Assert(inputSize <= LIMIT_INPUT_SIZE);
            Debug.Assert(deviceCount >= 1);
            Debug.Assert(deviceCount <= LIMIT_DEVICE_COUNT);
            Debug.Assert(totalPlayerCount >= 2);
            Debug.Assert(totalPlayerCount <= LIMIT_TOTAL_PLAYER_COUNT);

            OfflinePlay = offline;
            InputSize = inputSize;
            DeviceCount = deviceCount;
            TotalPlayerCount = totalPlayerCount;
        }
        /// <summary>
        /// SetLocalDevice tells that the given device is local to this Session and hosts the given player count.
        /// You must call SetLocalDevice before calling AddRemoteDevice
        /// </summary>
        /// <param name="deviceId">Unique number used to identify this local device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="playerCount">Number of players playing on this device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="frameDelay">Number of frames to skip before registering local input, used to avoid rollbacking every frame.</param>
        public abstract void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay);
        /// <summary>
        /// AddRemoteDevice tells that the given device is remote to this Session and hosts the given player count.
        /// You must call SetLocalDevice before calling AddRemoteDevice
        /// </summary>
        /// <param name="deviceId">Unique number used to identify this local device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="playerCount">Number of players playing on this device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="remoteConfiguration">As the given device is not local to the Session, we must provide a way to communicate with that given device, this configuration will be passed to the session adapter</param>
        public abstract void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration);

        public abstract void AddSpectatorDevice(object remoteConfiguration);

        /// <summary>
        /// Poll must be called periodically to give the Session a chance to perform some work and synchronize devices.
        /// </summary>
        public abstract void Poll();
        /// <summary>
        /// IsRunning returns true when all the Sessions are synchronized and ready to accept inputs.
        /// </summary>
        public abstract bool IsRunning();
        /// <summary>
        /// AdvanceFrame will tell the session to increment the current frame and that you are ready to work on the next frame.
        /// </summary>
        /// <param name="localInput">the local device input for the current frame</param>
        /// <returns>a list of actions to perform in order before calling AdvanceFrame again</returns>
        public abstract List<SessionAction> AdvanceFrame(byte[] localInput);
        internal protected abstract uint SendMessageTo(uint deviceId, DeviceMessage message);
        internal protected abstract void AddRemoteInput(uint deviceId, DeviceInputMessage message);

        public bool IsOffline() => OfflinePlay;
        public abstract int Frame();
        public abstract int RemoteFrame();
        public abstract int FrameAdvantage();
        public abstract int RemoteFrameAdvantage();
        public abstract int FrameAdvantageDifference();
        public abstract uint RollbackFrames();
        public abstract uint AverageRollbackFrames();
        public abstract int State();
    }
}