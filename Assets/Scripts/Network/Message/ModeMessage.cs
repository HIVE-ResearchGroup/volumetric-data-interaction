namespace Assets.Scripts.Network.Message
{
    [System.Serializable]
    public class ModeMessage : NetworkMessage
    {
        private readonly int _mode;

        public ModeMessage(MenuMode mode) : base(NetworkOperationCode.MenuMode)
        {
            _mode = (int)mode;
        }

        public int Mode => _mode;
    }
}
