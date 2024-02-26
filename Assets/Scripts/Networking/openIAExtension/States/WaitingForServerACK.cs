using System;

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

        public override InterpreterState ACK()
        {
            _onACK?.Invoke();
            return new DefaultState(Sender);
        }

        public override InterpreterState NAK()
        {
            _onNAK?.Invoke();
            return new DefaultState(Sender);
        }
    }
}