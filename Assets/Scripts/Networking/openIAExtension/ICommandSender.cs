using Networking.openIAExtension.Commands;

namespace Networking.openIAExtension
{
    public interface ICommandSender
    {
        void Send(ICommand cmd);
    }
}