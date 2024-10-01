#nullable enable

using System.Threading.Tasks;
using UnityEngine;

namespace Networking.openIA.States
{
    public class ProtocolNegotiator : IInterpreterState
    {
        private readonly ICommandSender _sender;

        public ProtocolNegotiator(ICommandSender sender)
        {
            _sender = sender;
        }
        
        public async Task Negotiate()
        {
            await _sender.Send(new ProtocolAdvertisement(1L));
        }

        public Task<IInterpreterState> ACK()
        {
            Debug.Log($"ACK received in {nameof(ProtocolNegotiator)}");
            return Task.FromResult<IInterpreterState>(new DefaultStateV1(_sender));
        }
        public Task<IInterpreterState> NAK()
        {
            Debug.Log($"NAK received in {nameof(ProtocolNegotiator)}");
            throw new NoProtocolMatchException();
        }
    }
}