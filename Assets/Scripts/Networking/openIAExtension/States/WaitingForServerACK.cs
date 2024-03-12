using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Networking.openIAExtension.States
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WaitingForServerACK : InterpreterState
    {
        private readonly Action _onACK;
        private readonly Action _onNAK;
        
        public WaitingForServerACK(ICommandSender sender, Action onACK = null, Action onNAK = null) : base(sender)
        {
            _onACK = onACK;
            _onNAK = onNAK;
        }

        public override Task<InterpreterState> ACK()
        {
            _onACK?.Invoke();
            return Task.FromResult<InterpreterState>(new DefaultState(Sender));
        }

        public override Task<InterpreterState> NAK()
        {
            _onNAK?.Invoke();
            return Task.FromResult<InterpreterState>(new DefaultState(Sender));
        }
    }
}