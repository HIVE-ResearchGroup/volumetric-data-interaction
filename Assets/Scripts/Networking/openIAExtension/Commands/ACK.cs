namespace Networking.openIAExtension.Commands
{
    public class ACK : ICommand
    {
        public byte[] ToByteArray()
        {
            return new byte[] { Categories.ACK };
        }
    }
}