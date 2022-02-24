[System.Serializable]
public class TiltMessage : NetworkMessage
{
    public TiltMessage()
    {
        OperationCode = NetworkOperationCode.Tilt;
    }

    public bool isLeft { set; get; }
}
