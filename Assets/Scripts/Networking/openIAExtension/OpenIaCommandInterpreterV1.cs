using System;
using Networking.openIAExtension.Commands;
using Networking.openIAExtension.States;
using UnityEngine;

namespace Networking.openIAExtension
{
    public class OpenIaCommandInterpreterV1 : ICommandInterpreter, ICommandSender
    {
        private InterpreterState _state;

        public OpenIaCommandInterpreterV1()
        {
            _state = new DefaultState(this);
        }
        
        public void Interpret(byte[] data)
        {
            switch (data[0])
            {
                case Categories.ACK:
                    _state = _state.ACK();
                    break;
                case Categories.NAK:
                    _state = _state.NAK();
                    break;
                case Categories.ProtocolAdvertisement:
                    _state = _state.ProtocolAdvertisement(data);
                    break;
                case Categories.Client:
                    _state = _state.Client(data);
                    break;
                case Categories.Datasets:
                    _state = _state.Datasets(data);
                    break;
                case Categories.Objects:
                    _state = _state.Objects(data);
                    break;
                case Categories.Snapshots:
                    _state = _state.Snapshots(data);
                    break;
                default:
                    Debug.LogError($"Unknown Category received: {BitConverter.ToString(data, 0, 1)}");
                    break;
            }
        }

        public void Send(ICommand cmd)
        {
            throw new NotImplementedException();
        }
    }
}