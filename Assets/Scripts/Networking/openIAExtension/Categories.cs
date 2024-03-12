using System.Diagnostics.CodeAnalysis;

namespace Networking.openIAExtension
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Categories
    {
        public static class ACK
        {
            public const byte Value = 0x0;
        }
        
        public static class NAK
        {
            public const byte Value = 0x1;
        }

        public static class ProtocolAdvertisement
        {
            public const byte Value = 0x2;
        }

        public static class Client
        {
            public const byte Value = 0x3;
        }

        public static class Datasets
        {
            public const byte Value = 0x4;
            public const byte Reset = 0x1;
            public const byte LoadDataset = 0x2;
        }

        public static class Objects
        {
            public const byte Value = 0x5;
            public const byte SetMatrix = 0x2;
            public const byte Translate = 0x3;
            public const byte Scale = 0x4;
            public const byte RotateQuaternion = 0x5;
            public const byte RotateEuler = 0x6;
        }

        public static class Snapshots
        {
            public const byte Value = 0x6;
            public const byte Create = 0x0;
            public const byte Remove = 0x2;
            public const byte Clear = 0x3;
            public const byte SlicePosition = 0x4;
        }
    }
}