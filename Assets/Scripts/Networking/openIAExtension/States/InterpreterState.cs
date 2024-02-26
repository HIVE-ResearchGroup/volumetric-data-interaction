using UnityEngine;

namespace Networking.openIAExtension.States
{
    public abstract class InterpreterState
    {
        protected readonly ICommandSender Sender;

        protected InterpreterState(ICommandSender sender)
        {
            Sender = sender;
        }
        
        public virtual InterpreterState ACK()
        {
            Debug.LogWarning($"Message {nameof(ACK)} not implemented");
            return this;
        }
        public virtual InterpreterState NAK()
        {
            Debug.LogWarning($"Message {nameof(NAK)} not implemented");
            return this;
        }
        public virtual InterpreterState ProtocolAdvertisement(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(ProtocolAdvertisement)} not implemented");
            return this;
        }
        public virtual InterpreterState Client(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Client)} not implemented");
            return this;
        }
        public virtual InterpreterState Datasets(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Datasets)} not implemented");
            return this;
        }
        public virtual InterpreterState Objects(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Objects)} not implemented");
            return this;
        }
        public virtual InterpreterState Snapshots(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Snapshots)} not implemented");
            return this;
        }
    }
}