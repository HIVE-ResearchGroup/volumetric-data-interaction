#nullable enable

using System;
using System.Threading.Tasks;
using Networking.openIA.States;
using UnityEngine;

namespace Networking.openIA
{
    public class InterpreterV1 : ICommandInterpreter, ICommandSender
    {
        private readonly WebSocketClient ws;
        private readonly ProtocolNegotiator negotiator;
        private IInterpreterState state;

        public InterpreterV1(WebSocketClient ws)
        {
            this.ws = ws;
            negotiator = new ProtocolNegotiator(this);
            state = negotiator;
        }

        public async Task Start() => await negotiator.Negotiate();
        
        public async Task Interpret(byte[] data)
        {
            switch (data[0])
            {
                case Categories.ACK.Value:
                    state = await state.ACK();
                    break;
                case Categories.NAK.Value:
                    state = await state.NAK();
                    break;
                case Categories.ProtocolAdvertisement.Value:
                    state = await state.ProtocolAdvertisement(data);
                    break;
                case Categories.Client.Value:
                    state = await state.Client(data);
                    break;
                case Categories.Datasets.Value:
                    state = await state.Datasets(data);
                    break;
                case Categories.Objects.Value:
                    state = await state.Objects(data);
                    break;
                case Categories.Snapshots.Value:
                    state = await state.Snapshots(data);
                    break;
                default:
                    Debug.LogError($"Unknown Category received: {BitConverter.ToString(data, 0, 1)}");
                    break;
            }
        }

        public async Task Send(ICommand cmd) => await ws.SendAsync(cmd.ToByteArray());
    }
}