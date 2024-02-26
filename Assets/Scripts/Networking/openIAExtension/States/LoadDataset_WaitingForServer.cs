namespace Networking.openIAExtension.States
{
    public class LoadDataset_WaitingForServer : IInterpreterState
    {
        public IInterpreterState Interpret(byte[] data)
        {
            switch (data[0])
            {
                
            }

            return null;
        }
    }
}