#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;
// ReSharper disable IdentifierTypo

namespace Networking.openIA.States
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IInterpreterState
    {
        public Task<IInterpreterState> ACK()
        {
            Debug.LogWarning($"Message {nameof(ACK)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> NAK()
        {
            Debug.LogWarning($"Message {nameof(NAK)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> ProtocolAdvertisement(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(ProtocolAdvertisement)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> Client(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Client)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> Datasets(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Datasets)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> Objects(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Objects)} not implemented");
            return Task.FromResult(this);
        }
        public Task<IInterpreterState> Snapshots(byte[] data)
        {
            Debug.LogWarning($"Message {nameof(Snapshots)} not implemented");
            return Task.FromResult(this);
        }
    }
}