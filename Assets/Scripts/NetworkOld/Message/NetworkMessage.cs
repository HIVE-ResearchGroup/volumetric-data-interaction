namespace NetworkOld.Message
{
    public enum NetworkOperationCode : byte
    {
        None = 0,
        Shake = 1,
        Tilt = 2,
        Tab = 3,
        Swipe = 4,
        Scale = 5,
        Rotation = 6,
        MenuMode = 7,

        Text = 9,
    }

    [System.Serializable]
    public abstract class NetworkMessage
    {
        private readonly byte _opCode;

        public NetworkMessage(NetworkOperationCode operationCode)
        {
            _opCode = (byte)operationCode;
        }

        public byte OperationCode => _opCode;

        public override string ToString() => $"Operation: {(NetworkOperationCode)OperationCode}";
    }
}
