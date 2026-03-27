using System;
using MessagePack;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PleaseResync
{
    public class UdpSessionAdapter : SessionAdapter
    {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly UdpClient _udpClient;
        private readonly IPEndPoint[] _remoteEndpoints;

        private IPEndPoint _remoteReceiveEndpoint;

        public UdpSessionAdapter(IPEndPoint endpoint)
        {
            _udpClient = new UdpClient();
            _udpClient.Client.Blocking = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(endpoint);
            _remoteEndpoints = new IPEndPoint[Session.LIMIT_DEVICE_COUNT];
            _remoteReceiveEndpoint = default(IPEndPoint);
        }

        public UdpSessionAdapter(ushort localPort) : this(new IPEndPoint(IPAddress.Any, localPort))
        {
        }

        public UdpSessionAdapter(string localAddress, ushort localPort) : this(new IPEndPoint(IPAddress.Parse(localAddress), localPort))
        {
        }

        public uint SendTo(uint deviceId, DeviceMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            return (uint)_udpClient.Send(packet, packet.Length, _remoteEndpoints[deviceId]);
        }

        public List<(uint size, uint deviceId, DeviceMessage message)> ReceiveFrom()
        {
            var messages = new List<(uint size, uint deviceId, DeviceMessage message)>();
            if (_udpClient.Available > 0)
            {
                var packet = _udpClient.Receive(ref _remoteReceiveEndpoint);
                var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
                messages.Add(((uint)packet.Length, FindDeviceIdFromEndpoint(_remoteReceiveEndpoint), message));
            }
            return messages;
        }

        public void AddRemote(uint deviceId, object remoteConfiguration)
        {
            if (remoteConfiguration is IPEndPoint remoteEndpoint)
            {
                _remoteEndpoints[deviceId] = remoteEndpoint;
            }
            else
            {
                throw new Exception($"Remote configuration must be of type {typeof(IPEndPoint)}");
            }
        }

        private uint FindDeviceIdFromEndpoint(IPEndPoint endpoint)
        {
            for (uint deviceId = 0; deviceId < _remoteEndpoints.Length; deviceId++)
            {
                if (endpoint.Equals(_remoteEndpoints[deviceId]))
                {
                    return deviceId;
                }
            }
            throw new Exception($"Device ID not found for endpoint {endpoint}");
        }

        #region Remote Configuration

        public static IPEndPoint CreateRemoteConfig(IPEndPoint endpoint)
        {
            return endpoint;
        }

        public static IPEndPoint CreateRemoteConfig(string remoteAddress, ushort remotePort)
        {
            return new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
        }

        public static IPEndPoint CreateRemoteConfig(IPAddress remoteAddress, ushort remotePort)
        {
            return new IPEndPoint(remoteAddress, remotePort);
        }

        public void Close()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }

        #endregion
    }
}
