using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// SessionAdapter is the interface used to implement a way for the Session to communicate with remote devices.
    /// </summary>
    public interface SessionAdapter
    {
        /// <summary>
        /// SendTo sends the given message to the given remote device and returns the number of bytes sent.
        /// </summary>
        /// <param name="deviceId">The remote device</param>
        /// <param name="message">The message to send</param>
        /// <returns>The number of bytes sent, or 0 on failure</returns>
        uint SendTo(uint deviceId, DeviceMessage message);

        /// <summary>
        /// ReceiveFrom receives all available messages sent by remote devices.
        /// </summary>
        /// <returns>A list of messages received</returns>
        List<(uint size, uint deviceId, DeviceMessage message)> ReceiveFrom();

        /// <summary>
        /// AddRemote is called by the Session when a remote device is added.
        /// Use the given remote configuration to establish a connection with the remote device.
        /// </summary>
        /// <param name="deviceId">The remote device id</param>
        /// <param name="remoteConfiguration">The configuration used to establish a connection with the given remote device</param>
        void AddRemote(uint deviceId, object remoteConfiguration);
    }
}
