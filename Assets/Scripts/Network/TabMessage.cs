[System.Serializable]
public class TabMessage : NetworkMessage
{
    public TabMessage()
    {
        OperationCode = NetworkOperationCode.Tab;
    }

    public TabMessage(TabType type)
    {
        OperationCode = NetworkOperationCode.Tab;
        TabType = (int)type;
    }

    public int TabType { set; get; }
}
