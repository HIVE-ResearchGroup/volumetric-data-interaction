using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.openIAExtension.States
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class InterpreterState
    {
        protected readonly ICommandSender Sender;

        protected InterpreterState(ICommandSender sender)
        {
            Sender = sender;
        }
        
        public virtual Task<InterpreterState> ACK()
        {
            Debug.LogWarning($"Message {nameof(ACK)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> NAK()
        {
            Debug.LogWarning($"Message {nameof(NAK)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> ProtocolAdvertisement(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(ProtocolAdvertisement)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> Client(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Client)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> Datasets(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Datasets)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> Objects(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Objects)} not implemented");
            return Task.FromResult(this);
        }
        public virtual Task<InterpreterState> Snapshots(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Snapshots)} not implemented");
            return Task.FromResult(this);
        }
    }
}