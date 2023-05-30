namespace NetworkOld.Message
{
    [System.Serializable]
    public class TiltMessage : NetworkMessage
    {
        private readonly bool _isLeft;

        public TiltMessage(bool isLeft) : base(NetworkOperationCode.Tilt)
        {
            _isLeft = isLeft;
        }

        public bool IsLeft => _isLeft;
    }
}
