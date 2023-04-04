namespace Assets.Scripts.Network.Message
{
    [System.Serializable]
    public class RotationMessage : NetworkMessage
    {
        private readonly float _rotRadDelta;

        public RotationMessage(float rotation) : base(NetworkOperationCode.Rotation)
        {
            _rotRadDelta = rotation;
        }

        public float RotationRadiansDelta => _rotRadDelta;
    }
}
