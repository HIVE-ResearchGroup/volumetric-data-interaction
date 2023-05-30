namespace NetworkOld.Message
{
    [System.Serializable]
    public class ShakeMessage : NetworkMessage
    {
        private readonly int _count;

        public ShakeMessage(int count) : base(NetworkOperationCode.Shake)
        {
            _count = count;
        }

        public int Count => _count;
    }
}
