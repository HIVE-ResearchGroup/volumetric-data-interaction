#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Networking.openIA.States
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WaitingForServerACK : IInterpreterState
    {
        private readonly IInterpreterState _nextState;
        private readonly Action? _onACK;
        private readonly Action? _onNAK;
        
        public WaitingForServerACK(IInterpreterState nextState, Action? onACK = null, Action? onNAK = null)
        {
            _nextState = nextState;
            _onACK = onACK;
            _onNAK = onNAK;
        }

        public Task<IInterpreterState> ACK()
        {
            _onACK?.Invoke();
            return Task.FromResult(_nextState);
        }

        public Task<IInterpreterState> NAK()
        {
            _onNAK?.Invoke();
            return Task.FromResult(_nextState);
        }
    }
}