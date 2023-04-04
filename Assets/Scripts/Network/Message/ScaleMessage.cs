namespace Assets.Scripts.Network.Message
{
    [System.Serializable]
    public class ScaleMessage : NetworkMessage
    {
        private readonly float _scaleMultiplier;

        public ScaleMessage(float scaleMultiplier) : base(NetworkOperationCode.Scale)
        {
            _scaleMultiplier = scaleMultiplier;
        }

        public float ScaleMultiplier => _scaleMultiplier;
    }
}
