[System.Serializable]
public class TextMessage : NetworkMessage
{
    private readonly string _text;

    public TextMessage(string text) : base(NetworkOperationCode.Text)
    {
        _text = text;
    }

    public string Text => _text;
}
