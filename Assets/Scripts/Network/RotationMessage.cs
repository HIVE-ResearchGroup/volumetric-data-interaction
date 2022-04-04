[System.Serializable]
public class RotationMessage : NetworkMessage
{
    public RotationMessage()
    {
        OperationCode = NetworkOperationCode.Rotation;
    }

    public RotationMessage(float rotation)
    {
        OperationCode = NetworkOperationCode.Rotation;
        RotationRadiansDelta = rotation;
    }

    public float RotationRadiansDelta { set; get; }
}
