using System.Diagnostics.CodeAnalysis;

namespace Networking.openIAExtension.Commands
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class NAK : ICommand
    {
        public byte[] ToByteArray()
        {
            return new byte[] { Categories.NAK.Value };
        }
    }
}