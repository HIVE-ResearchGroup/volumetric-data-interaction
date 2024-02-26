namespace Networking.openIAExtension
{
    public class OpenIaCommandInterpreterV1 : ICommandInterpreter
    {
        private IInterpreterState _state;
        
        public void Interpret(byte[] data)
        {
            switch (data[0])
            {
                
            }
        }
    }
}