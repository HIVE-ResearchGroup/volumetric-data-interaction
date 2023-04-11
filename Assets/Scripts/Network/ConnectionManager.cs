using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Network
{
    public enum ConnectionType
    {
        Client,
        Host
    }
    /// <summary>
    /// Firewall for Domain Network needs to be deactivated!
    /// </summary>
    public abstract class ConnectionManager : MonoBehaviour
    {
        protected int BYTE_SIZE = 1024;

        protected int hostId;
        protected int connectionId;
        protected byte reliableChannel;
        protected byte error;

        public void Init(ConnectionType type)
        {
            NetworkTransport.Init();

            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.Reliable);

            HostTopology topo = new HostTopology(cc, 1);

            hostId = type == ConnectionType.Client
                ? NetworkTransport.AddHost(topo)
                : NetworkTransport.AddHost(topo, ConfigurationConstants.DEFAULT_PORT);
        }

        public void Connect(string ip, int port)
        {
            connectionId = NetworkTransport.Connect(hostId, ip, port, 0, out error);
            if (error != 0)
            {
                Debug.LogError($"Error code: {error}, Error: {(NetworkError)error}");
            }
        }

        public void Disconnect()
        {
            NetworkTransport.Disconnect(hostId, connectionId, out error);
        }

        public void Shutdown()
        {
            NetworkTransport.Shutdown();
        }

        public abstract void UpdateMessagePump();

        private void OnDestroy()
        {
            Disconnect();
            Shutdown();
        }
    }
}
