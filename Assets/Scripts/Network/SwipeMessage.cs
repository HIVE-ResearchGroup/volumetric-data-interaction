[System.Serializable]
public class SwipeMessage : NetworkMessage
{
    public SwipeMessage()
    {
        OperationCode = NetworkOperationCode.Swipe;
    }

    public bool IsInwardSwipe { get; set; }
    public float EndPointX { get; set; }
    public float EndPointY { get; set; }
}
