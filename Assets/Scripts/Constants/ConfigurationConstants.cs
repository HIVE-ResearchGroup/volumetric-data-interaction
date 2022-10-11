public static class ConfigurationConstants
{
    public const string DEFAULT_IP = "127.0.0.1";
    public const string HOST_IP = "192.168.0.101"; //"192.168.208.1"; //"192.168.44.1"; //"10.42.1.4"; // IP Adress of PC
    public const int DEFAULT_CONNECTING_PORT = 7777;

    public const int SNAPSHOT_DISTANCE = 2;

    private const string DATA_FOLDER_PATH = @"C:\Users\P42542\Desktop\Data\";
    public const string X_STACK_PATH_LOW_RES = DATA_FOLDER_PATH + @"Stack_x_0.2mm_lowRes";
    public const string X_STACK_PATH = DATA_FOLDER_PATH + @"Nähmaschine\Stack_x_1mm";
    public const string Y_STACK_PATH = DATA_FOLDER_PATH + @"Stack_y_1mm";
    public const string Z_STACK_PATH = DATA_FOLDER_PATH + @"Stack_z_1mm";

    public const string IMAGES_FOLDER_PATH = DATA_FOLDER_PATH + @"TempImages";
}
