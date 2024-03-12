using System;
using System.Threading.Tasks;

namespace Networking.openIAExtension.States
{
    public class WaitingForServerACK : InterpreterState
    {
        private readonly Action _onACK;
        private readonly Action _onNAK;
        
        public WaitingForServerACK(ICommandSender sender, Action onACK = null, Action onNAK = null) : base(sender)
        {
            _onACK = onACK;
            _onNAK = onNAK;
        }

        public override async Task<InterpreterState> ACK()
        {
            _onACK?.Invoke();
            return await Task.FromResult<InterpreterState>(new DefaultState(Sender));
        }

        public override async Task<InterpreterState> NAK()
        {
            _onNAK?.Invoke();
            return await Task.FromResult<InterpreterState>(new DefaultState(Sender));
        }
    }
}