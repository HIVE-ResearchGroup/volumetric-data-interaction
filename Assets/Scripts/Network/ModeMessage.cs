[System.Serializable]
public class ModeMessage : NetworkMessage
{
    public ModeMessage()
    {
        OperationCode = NetworkOperationCode.MenuMode;
    }

    public ModeMessage(MenuMode mode) : this()
    {
        Mode = (int)mode;
    }

    public int Mode { set; get; }
}
