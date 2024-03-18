using System;
using System.Threading;
using System.Threading.Tasks;

namespace Networking.openIAExtension
{
    public class ProtocolNegotiator : ICommandInterpreter
    {
        private readonly SemaphoreSlim _sem = new(0, 1);

        private ulong _protocolVersion;

        private readonly WebSocketClient _ws;

        public ProtocolNegotiator(WebSocketClient ws)
        {
            _ws = ws;
        }
        
        public async Task<(ICommandInterpreter, ICommandSender)> Negotiate()
        {
            var request = new byte[9];
            request[0] = Categories.ProtocolAdvertisement.Value;
            var versionBytes = BitConverter.GetBytes(1L);
            _protocolVersion = 1;
            Buffer.BlockCopy(versionBytes, 0, request, 1, 8);
            await _ws.SendAsync(request);
            await _sem.WaitAsync();
            if (_protocolVersion == 0)
            {
                throw new NoProtocolMatchException();
            }
            var interpreter = new InterpreterV1(_ws);
            return (interpreter, interpreter);
        }
        
        public Task Interpret(byte[] data)
        {
            switch (data[0])
            {
                case 0x0:   // ACK
                    // do nothing on ACK
                    // the right protocol version is already set
                    break;
                case 0x1:   // NAK
                    // set the protocol version to an invalid version number to signal an error
                    _protocolVersion = 0;
                    break;
                default:
                    throw new ArgumentException("Message is neither ACK nor NAK!");
            }

            _sem.Release();
            return Task.CompletedTask;
        }
    }
}