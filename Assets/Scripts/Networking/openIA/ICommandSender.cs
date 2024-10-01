using System.Threading.Tasks;

namespace Networking.openIA
{
    public interface ICommandSender
    {
        Task Send(ICommand cmd);
    }
}