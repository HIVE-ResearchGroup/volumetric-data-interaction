namespace Networking.openIAExtension
{
    public interface ICommandInterpreter
    {
        void Interpret(byte[] data);
    }
}