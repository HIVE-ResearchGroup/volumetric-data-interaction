namespace Constants
{
    public static class ConfigurationConstants
    {
        public const string DEFAULT_IP = "127.0.0.1";
        public const int DEFAULT_PORT = 7777;

        public const int SNAPSHOT_DISTANCE = 2;
        public const int NEIGHBOUR_DISTANCE = 5; // pixel, except for if it is along x-axis, then it is slices
        public const int BLACK_TRANSPARENT_THRESHOLD = 30;

        private const string DATA_FOLDER_PATH = @"C:\Users\p42652\Desktop\Janine_VIVE_Data\Data\";

        // all paths from here are unused!
        public const string X_STACK_PATH_LOW_RES = DATA_FOLDER_PATH + @"Stack_x_0.2mm_lowRes";
        public const string X_STACK_PATH = DATA_FOLDER_PATH + @"Nähmaschine\Stack_x_1mm";
        public const string Y_STACK_PATH = DATA_FOLDER_PATH + @"Stack_y_1mm";
        public const string Z_STACK_PATH = DATA_FOLDER_PATH + @"Stack_z_1mm";

        public const string IMAGES_FOLDER_PATH = DATA_FOLDER_PATH + @"TempImages";
    }
}
