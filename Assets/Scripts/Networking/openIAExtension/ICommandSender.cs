using System.Threading.Tasks;
using Networking.openIAExtension.Commands;

namespace Networking.openIAExtension
{
    public interface ICommandSender
    {
        Task Send(ICommand cmd);
    }
}