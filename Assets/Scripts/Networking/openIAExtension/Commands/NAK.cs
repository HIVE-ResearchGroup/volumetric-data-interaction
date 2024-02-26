namespace Networking.openIAExtension.Commands
{
    public class NAK : ICommand
    {
        public byte[] ToByteArray()
        {
            return new byte[] { Categories.NAK };
        }
    }
}