using System;
using System.Net;
using LiteNetLib;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PleaseResync
{
    public class LiteNetLibSessionAdapter : SessionAdapter, INetEventListener
    {
        private readonly Dictionary<uint, IPEndPoint> _remoteEndpoints = [];

        private NetManager _netManager;
        private List<(uint size, uint deviceId, DeviceMessage message)> _messages;

        public LiteNetLibSessionAdapter(IPEndPoint endpoint)
        {
            _messages = new List<(uint size, uint deviceId, DeviceMessage message)>();
            _netManager = new NetManager(this);
            _netManager.Start(endpoint.Port);
            _netManager.ReuseAddress = true;
            _remoteEndpoints = new();
        }

        public LiteNetLibSessionAdapter(ushort localPort) : this(new IPEndPoint(IPAddress.Any, localPort))
        {
        }

        public LiteNetLibSessionAdapter(string localAddress, ushort localPort) : this(new IPEndPoint(IPAddress.Parse(localAddress), localPort))
        {
        }

        public uint SendTo(uint deviceId, DeviceMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            foreach (var peer in _netManager.ConnectedPeerList)
            {
                if (peer.Port == _remoteEndpoints[deviceId].Port)
                {
                    peer.Send(packet, DeliveryMethod.Unreliable);
                    return (uint)packet.Length;
                }
            }
            return 0;
        }

        public List<(uint size, uint deviceId, DeviceMessage message)> ReceiveFrom()
        {
            _netManager.PollEvents();

            var copy = new List<(uint size, uint deviceId, DeviceMessage message)>(_messages);
            _messages.Clear();
            return copy;
        }

        public void AddRemote(uint deviceId, object remoteConfiguration)
        {
            if (remoteConfiguration is IPEndPoint remoteEndpoint)
            {
                _remoteEndpoints[deviceId] = remoteEndpoint;
                _netManager.Connect(remoteEndpoint.Address.ToString(), remoteEndpoint.Port, "ok");
            }
            else
            {
                throw new Exception($"Remote configuration must be of type {typeof(IPEndPoint)}");
            }
        }

        private uint FindDeviceIdFromEndpoint(IPEndPoint endpoint)
        {
            return _remoteEndpoints.First(x => x.Value.Equals(endpoint)).Key;
        }

        public void Close()
        {
            _netManager.DisconnectAll();
            _netManager.Stop();
        }

        #region Netmanager events

        public void OnPeerConnected(NetPeer peer)
        {
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader NetReader, byte channel, DeliveryMethod deliveryMethod)
        {
            var packet = NetReader.GetRemainingBytes();
            var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
            _messages.Add(((uint)packet.Length, FindDeviceIdFromEndpoint(peer), message));
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.Accept();
        }

        #endregion

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

        #endregion
    }
}
