namespace NetworkOld.Message
{
    [System.Serializable]
    public class SwipeMessage : NetworkMessage
    {
        private readonly bool _inward;
        private readonly float _endpointX;
        private readonly float _endpointY;
        private readonly double _angle;

        public SwipeMessage(bool inward, float endpointX, float endpointY, double angle) : base(NetworkOperationCode.Swipe)
        {
            _inward = inward;
            _endpointX = endpointX;
            _endpointY = endpointY;
            _angle = angle;
        }

        public bool IsInwardSwipe => _inward;
        public float EndPointX => _endpointX;
        public float EndPointY => _endpointY;
        public double Angle => _angle;
    }
}
