[System.Serializable]
public class TabMessage : NetworkMessage
{
    private readonly int _tabType;

    public TabMessage(TabType type) : base(NetworkOperationCode.Tab)
    {
        _tabType = (int)type;
    }

    public int TabType => _tabType;
}
