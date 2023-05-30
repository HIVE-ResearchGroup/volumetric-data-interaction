namespace Interaction
{
    public class InputTracker
    {
        public InputTracker() { }

        public float Threshold { get; set; }
        public float TimeSinceFirst { get; set; }
        public float TimeSinceLast { get; set; }
    }
}