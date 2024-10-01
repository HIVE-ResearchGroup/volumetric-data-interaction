using System.Threading.Tasks;

namespace Networking.openIA
{
    public interface ICommandInterpreter
    {
        Task Interpret(byte[] data);
    }
}