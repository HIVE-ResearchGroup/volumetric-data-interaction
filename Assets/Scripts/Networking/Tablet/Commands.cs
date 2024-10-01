using System;

namespace Networking.Tablet
{
    public interface ICommand
    {
        byte[] ToByteArray();
    }

    public record ScaleCommand(float Value) : ICommand
    {
        public byte[] ToByteArray()
        {
            var request = new byte[Size];
            request[0] = Categories.Scale;
            BitConverter.TryWriteBytes(request.AsSpan(1), Value);
            return request;
        }

        public static int Size => 1 + sizeof(float);

        public static ScaleCommand FromByteArray(byte[] buffer)
        {
            return new ScaleCommand(BitConverter.ToSingle(buffer, 1));
        }
    }
}