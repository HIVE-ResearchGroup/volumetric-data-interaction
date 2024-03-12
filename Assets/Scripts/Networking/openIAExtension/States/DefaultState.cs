using System;
using System.Threading.Tasks;
using Model;
using Networking.openIAExtension.Commands;
using Snapshots;
using UnityEngine;

namespace Networking.openIAExtension.States
{
    public class DefaultState : InterpreterState
    {
        public DefaultState(ICommandSender sender) : base(sender)
        {
        }
        
        public override async Task<InterpreterState> Datasets(byte[] data)
        {
            switch (data[1])
            {
                case 0x0:
                    ModelManager.Instance.ResetState();
                    SnapshotManager.Instance.ResetState();
                    return this;
                case 0x1:
                    var nameLength = BitConverter.ToInt32(data, 2);
                    var name = BitConverter.ToString(data, 6, nameLength);
                    if (ModelManager.Instance.ModelExists(name))
                    {
                        await Sender.Send(new ACK());
                        return new WaitingForServerACK(Sender, () => ModelManager.Instance.ChangeModel(name));
                    }
                    else
                    {
                        await Sender.Send(new NAK());
                        return this;
                    }
                default:
                    Debug.LogError($"Unhandled Subcategory in Dataset Operation: {BitConverter.ToString(data, 1, 1)}");
                    return this;
            }
        }
    }
}