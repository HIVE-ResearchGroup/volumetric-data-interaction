#nullable enable

using System.Net.Sockets;
using System.Threading.Tasks;

namespace Extensions
{
    public static class NetworkStreamExtensions
    {
        public static void ReadAll(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            var innerOffset = 0;
            while (count != innerOffset)
            {
                var missingBytes = count - innerOffset;
                var bytes = stream.Read(buffer, offset + innerOffset, missingBytes);
                innerOffset += bytes;
            }
        }
        
        /// <summary>
        /// This method wraps around ReadAsync() in a way, where it will automatically combine fragmented packages.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static async Task ReadAllAsync(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            var innerOffset = 0;
            while (count != innerOffset)
            {
                var missingBytes = count - innerOffset;
                var bytes = await stream.ReadAsync(buffer, offset + innerOffset, missingBytes);
                innerOffset += bytes;
            }
        }
    }
}