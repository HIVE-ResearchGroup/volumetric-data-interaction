[System.Serializable]
public class TextMessage : NetworkMessage
{
    public TextMessage(string text)
    {
        OperationCode = NetworkOperationCode.Text;
        Text = text;
    }

    public string Text { set; get; }
}
