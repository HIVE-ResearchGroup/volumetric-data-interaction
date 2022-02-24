public static class NetworkOperationCode
{
    public const int None = 0;
    public const int Shake = 1;
    public const int Tilt = 2;
}

[System.Serializable]
public abstract class NetworkMessage
{
    public byte OperationCode { get; set; }

    public NetworkMessage()
    {
        OperationCode = NetworkOperationCode.None;
    }
}
