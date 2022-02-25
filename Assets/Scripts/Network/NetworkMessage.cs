public static class NetworkOperationCode
{
    public const int None = 0;
    public const int Shake = 1;
    public const int Tilt = 2;
    public const int Tab = 3;
    public const int Swipe = 4;
    public const int Scale = 5;

    public const int Text = 9;
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
