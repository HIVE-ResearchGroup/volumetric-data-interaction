namespace Networking.openIAExtension
{
    internal interface ICommandInterpreter
    {
        void Interpret(byte[] data);
    }
}