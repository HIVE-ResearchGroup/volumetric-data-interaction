[System.Serializable]
public class ScaleMessage : NetworkMessage
{
    public ScaleMessage()
    {
        OperationCode = NetworkOperationCode.Scale;
    }

    public ScaleMessage(float scaleMultiplier)
    {
        OperationCode = NetworkOperationCode.Scale;
        ScaleMultiplier = scaleMultiplier;
    }

    public float ScaleMultiplier { set; get; }
}
