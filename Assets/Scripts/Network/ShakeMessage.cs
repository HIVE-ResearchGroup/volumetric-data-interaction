[System.Serializable]
public class ShakeMessage : NetworkMessage
{
    public ShakeMessage()
    {
        OperationCode = NetworkOperationCode.Shake;
    }

    public int Count { set; get; }
}
