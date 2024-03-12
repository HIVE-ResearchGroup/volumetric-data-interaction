using System.Diagnostics.CodeAnalysis;

namespace Networking.openIAExtension.Commands
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACK : ICommand
    {
        public byte[] ToByteArray()
        {
            return new byte[] { Categories.ACK.Value };
        }
    }
}