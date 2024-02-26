using System;
using Networking.openIAExtension.Commands;
using UnityEngine;

namespace Networking.openIAExtension.States
{
    public class DefaultState : InterpreterState
    {
        public DefaultState(ICommandSender sender) : base(sender)
        {
        }
        
        public override InterpreterState Datasets(byte[] data)
        {
            switch (data[1])
            {
                case 0x0:
                    // TODO reset
                    return this;
                case 0x1:
                    var nameLength = BitConverter.ToInt32(data, 2);
                    var name = BitConverter.ToString(data, 6, nameLength);
                    // TODO check if exists
                    var exists = false;
                    if (exists)
                    {
                        Sender.Send(new ACK());
                        // TODO add ACK Action as parameter
                        return new WaitingForServerACK(Sender);
                    }
                    else
                    {
                        Sender.Send(new NAK());
                        return this;
                    }
                default:
                    Debug.LogError($"Unhandled Subcategory in Dataset Operation: {BitConverter.ToString(data, 1, 1)}");
                    return this;
            }
        }
    }
}