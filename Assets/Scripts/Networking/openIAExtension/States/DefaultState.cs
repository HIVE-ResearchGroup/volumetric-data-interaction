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

        public override Task<InterpreterState> Client(byte[] data)
        {
            var id = BitConverter.ToUInt64(data, 1);
            // INFO id is entirely unused on the client
            return Task.FromResult<InterpreterState>(this);
        }

        public override async Task<InterpreterState> Datasets(byte[] data)
        {
            switch (data[1])
            {
                case Categories.Datasets.Reset:
                    ModelManager.Instance.ResetState();
                    SnapshotManager.Instance.ResetState();
                    return this;
                case Categories.Datasets.LoadDataset:
                    var nameLength = BitConverter.ToInt32(data, 2);
                    var name = BitConverter.ToString(data, 6, nameLength);
                    if (ModelManager.Instance.ModelExists(name))
                    {
                        await Sender.Send(new ACK());
                        return new WaitingForServerACK(Sender, () => ModelManager.Instance.ChangeModel(name));
                    }
                    await Sender.Send(new NAK());
                    return this;
                default:
                    Debug.LogError($"Unhandled Subcategory in Dataset Operation: {BitConverter.ToString(data, 1, 1)}");
                    return this;
            }
        }

        public override Task<InterpreterState> Objects(byte[] data)
        {
            switch (data[1])
            {
                case Categories.Objects.SetMatrix:
                    break;
                case Categories.Objects.Translate:
                    break;
                case Categories.Objects.Scale:
                    break;
                case Categories.Objects.RotateQuaternion:
                    break;
                case Categories.Objects.RotateEuler:
                    break;
            }

            return Task.FromResult<InterpreterState>(this);
        }

        public override Task<InterpreterState> Snapshots(byte[] data)
        {
            switch (data[1])
            {
                case Categories.Snapshots.Create:
                    break;
                case Categories.Snapshots.Remove:
                    break;
                case Categories.Snapshots.Clear:
                    break;
                case Categories.Snapshots.SlicePosition:
                    break;
            }
            
            return Task.FromResult<InterpreterState>(this);
        }
    }
}