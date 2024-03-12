using System;
using System.Threading.Tasks;
using Networking.openIAExtension.Commands;
using Networking.openIAExtension.States;
using UnityEngine;

namespace Networking.openIAExtension
{
    public class InterpreterV1 : ICommandInterpreter, ICommandSender
    {
        private readonly WebSocketClient _ws;
        private InterpreterState _state;

        public InterpreterV1(WebSocketClient ws)
        {
            _ws = ws;
            _state = new DefaultState(this);
        }
        
        public async Task Interpret(byte[] data)
        {
            switch (data[0])
            {
                case Categories.ACK.Value:
                    _state = await _state.ACK();
                    break;
                case Categories.NAK.Value:
                    _state = await _state.NAK();
                    break;
                case Categories.ProtocolAdvertisement.Value:
                    _state = await _state.ProtocolAdvertisement(data);
                    break;
                case Categories.Client.Value:
                    _state = await _state.Client(data);
                    break;
                case Categories.Datasets.Value:
                    _state = await _state.Datasets(data);
                    break;
                case Categories.Objects.Value:
                    _state = await _state.Objects(data);
                    break;
                case Categories.Snapshots.Value:
                    _state = await _state.Snapshots(data);
                    break;
                default:
                    Debug.LogError($"Unknown Category received: {BitConverter.ToString(data, 0, 1)}");
                    break;
            }
        }

        public async Task Send(ICommand cmd) => await _ws.SendAsync(cmd.ToByteArray());
    }
}