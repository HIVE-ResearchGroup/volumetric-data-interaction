namespace Constants
{
    public static class ConfigurationConstants
    {
        public const int SNAPSHOT_DISTANCE = 2;
        public const int NEIGHBOUR_DISTANCE = 5; // pixel, except for if it is along x-axis, then it is slices
        public const float BLACK_TRANSPARENT_THRESHOLD = 0.12f; //30;

        private const string DATA_FOLDER_PATH = @"C:\Users\p42652\Desktop\Wels fiber slices\";
        public const string X_STACK_PATH_LOW_RES = DATA_FOLDER_PATH + @"StackYZ";
        public const string IMAGES_FOLDER_PATH = DATA_FOLDER_PATH + @"TempImages";
    }
}
