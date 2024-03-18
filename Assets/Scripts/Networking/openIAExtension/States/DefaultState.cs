using System;
using System.Threading.Tasks;
using Helper;
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
                {
                    ModelManager.Instance.ResetState();
                    SnapshotManager.Instance.ResetState();
                    return this;
                }
                case Categories.Datasets.LoadDataset:
                {
                    var nameLength = BitConverter.ToInt32(data, 2);
                    var name = BitConverter.ToString(data, 6, nameLength);
                    if (ModelManager.Instance.ModelExists(name))
                    {
                        await Sender.Send(new ACK());
                        return new WaitingForServerACK(Sender, () => ModelManager.Instance.ChangeModel(name));
                    }

                    await Sender.Send(new NAK());
                    return this;
                }
                default:
                {
                    Debug.LogError($"Unhandled Subcategory in Dataset Operation: {BitConverter.ToString(data, 1, 1)}");
                    return this;
                }
            }
        }

        public override Task<InterpreterState> Objects(byte[] data)
        {
            switch (data[1])
            {
                case Categories.Objects.SetMatrix:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var matrix = new Matrix4x4();
                    for (var i = 0; i < 16; i++)
                    {
                        matrix[i] = BitConverter.ToSingle(data, 10 + (i * 4));
                    }
                    MatrixToObject(id, matrix);
                    break;
                }
                case Categories.Objects.Translate:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var x = BitConverter.ToSingle(data, 10);
                    var y = BitConverter.ToSingle(data, 14);
                    var z = BitConverter.ToSingle(data, 18);

                    TranslateObject(id, new Vector3(x, y, z));
                    break;
                }
                case Categories.Objects.Scale:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var x = BitConverter.ToSingle(data, 10);
                    var y = BitConverter.ToSingle(data, 14);
                    var z = BitConverter.ToSingle(data, 18);
                    ScaleObject(id, new Vector3(x, y, z));
                    break;
                }
                case Categories.Objects.RotateQuaternion:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var x = BitConverter.ToSingle(data, 10);
                    var y = BitConverter.ToSingle(data, 14);
                    var z = BitConverter.ToSingle(data, 18);
                    var w = BitConverter.ToSingle(data, 22);
                    RotateObject(id, new Quaternion(x, y, z, w));
                    break;
                }
                case Categories.Objects.RotateEuler:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var axis = data[8];
                    var amount = BitConverter.ToSingle(data, 9);
                    RotateObject(id, axis, amount);
                    break;
                }
            }

            return Task.FromResult<InterpreterState>(this);
        }

        public override Task<InterpreterState> Snapshots(byte[] data)
        {
            switch (data[1])
            {
                case Categories.Snapshots.Create:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var x = BitConverter.ToSingle(data, 10);
                    var y = BitConverter.ToSingle(data, 14);
                    var z = BitConverter.ToSingle(data, 18);
                    var position = new Vector3(x, y, z);
                    x = BitConverter.ToSingle(data, 22);
                    y = BitConverter.ToSingle(data, 26);
                    z = BitConverter.ToSingle(data, 30);
                    var w = BitConverter.ToSingle(data, 34);
                    var rotation = new Quaternion(x, y, z, w);
                    SnapshotManager.Instance.CreateSnapshot(id, position, rotation);
                    break;
                }
                case Categories.Snapshots.Remove:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    SnapshotManager.Instance.DeleteSnapshot(id);
                    break;
                }
                case Categories.Snapshots.Clear:
                {
                    SnapshotManager.Instance.ResetState();
                    break;
                }
                case Categories.Snapshots.SlicePosition:
                {
                    var id = BitConverter.ToUInt64(data, 2);
                    var axis = data[6];
                    var value = BitConverter.ToSingle(data, 7);

                    switch ((Axis)axis)
                    {
                        case Axis.X:
                        {
                            var snapshot = SnapshotManager.Instance.GetSnapshot(id);
                            if (snapshot == null)
                            {
                                Debug.LogWarning($"Snapshot with ID {id} not found.");
                                break;
                            }
                            snapshot.MoveSliceX(value);
                            break;
                        }
                        case Axis.Y:
                        {
                            var snapshot = SnapshotManager.Instance.GetSnapshot(id);
                            if (snapshot == null)
                            {
                                Debug.LogWarning($"Snapshot with ID {id} not found.");
                                break;
                            }
                            snapshot.MoveSliceY(value);
                            break;
                        }
                        case Axis.Z:
                        {
                            var snapshot = SnapshotManager.Instance.GetSnapshot(id);
                            if (snapshot == null)
                            {
                                Debug.LogWarning($"Snapshot with ID {id} not found.");
                                break;
                            }
                            snapshot.MoveSliceZ(value);
                            break;
                        }
                        default:
                            Debug.LogError($"Axis {axis} not specified in protocol!");
                            break;
                    }
                    break;
                }
            }
            
            return Task.FromResult<InterpreterState>(this);
        }

        private static void MatrixToObject(ulong id, Matrix4x4 matrix)
        {
            if (id != 0)
            {
                // my slicing plane is the tablet...
                // how would this work?
                Debug.LogWarning("The SlicingPlane is the Tablet. It cannot be changed.");
                return;
            }

            var transform = ModelManager.Instance.CurrentModel.transform;
            transform.SetPositionAndRotation(matrix.GetPosition(), matrix.rotation);
            transform.localScale = matrix.lossyScale;
        }

        private static void TranslateObject(ulong id, Vector3 translation)
        {
            if (id != 0)
            {
                // my slicing plane is the tablet...
                // how would this work?
                Debug.LogWarning("The SlicingPlane is the Tablet. It cannot be changed.");
                return;
            }
            
            ModelManager.Instance.CurrentModel.transform.position += translation;
        }

        private static void ScaleObject(ulong id, Vector3 scale)
        {
            if (id != 0)
            {
                // my slicing plane is the tablet...
                // how would this work?
                Debug.LogWarning("The SlicingPlane is the Tablet. It cannot be changed.");
                return;
            }

            ModelManager.Instance.CurrentModel.transform.localScale += scale;
        }
        
        private static void RotateObject(ulong id, int axis, float value)
        {
            if (id != 0)
            {
                // my slicing plane is the tablet...
                // how would this work?
                Debug.LogWarning("The SlicingPlane is the Tablet. It cannot be changed.");
                return;
            }

            var vec = axis switch
            {
                0 => new Vector3(1, 0, 0),
                1 => new Vector3(0, 1, 0),
                2 => new Vector3(0, 0, 1),
                _ => throw new ArgumentException($"Invalid axis: {axis}")
            };
            
            ModelManager.Instance.CurrentModel.transform.Rotate(vec, value);
        }

        private static void RotateObject(ulong id, Quaternion quaternion)
        {
            if (id != 0)
            {
                // my slicing plane is the tablet...
                // how would this work?
                Debug.LogWarning("The SlicingPlane is the Tablet. It cannot be changed.");
                return;
            }

            ModelManager.Instance.CurrentModel.transform.rotation *= quaternion;
        }
    }
}