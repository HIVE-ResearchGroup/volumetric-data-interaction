namespace Networking.openIAExtension
{
    public interface IInterpreterState
    {
        IInterpreterState Interpret(byte[] data);
    }
}