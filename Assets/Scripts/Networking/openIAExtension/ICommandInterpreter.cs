using System.Threading.Tasks;

namespace Networking.openIAExtension
{
    public interface ICommandInterpreter
    {
        Task Interpret(byte[] data);
    }
}