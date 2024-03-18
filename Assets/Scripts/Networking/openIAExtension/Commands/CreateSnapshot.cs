using UnityEngine;

namespace Networking.openIAExtension.Commands
{
    public class CreateSnapshot : ICommand
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        
        public CreateSnapshot(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
        
        public byte[] ToByteArray()
        {
            throw new System.NotImplementedException();
        }
    }
}