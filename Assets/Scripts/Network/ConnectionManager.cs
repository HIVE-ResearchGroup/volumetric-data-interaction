using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Network
{
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

        public void Init()
        {
            NetworkTransport.Init();

            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.Reliable);

            HostTopology topo = new HostTopology(cc, 1);

            hostId = NetworkTransport.AddHost(topo, ConfigurationConstants.DEFAULT_PORT);
        }

        public void Connect(string ip, int port)
        {
            connectionId = NetworkTransport.Connect(hostId, ip, port, 0, out error);
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
