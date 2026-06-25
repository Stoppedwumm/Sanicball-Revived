namespace SanicballCore
{
    public class GameVersion
    {
        //The current version as a float, for checking which version is newest
        public const float AS_FLOAT = 0.101f;

        //To differentiate between testing builds and release builds
        public const bool IS_TESTING = true;
#if DEBUG
        //As a string, for displaying on the UI
        public const string AS_STRING = "alpha-revived debug build";
        //Something stupid, usually unique for every version
        public const string TAGLINE = "building from bleeding aren't we?";
#else
        //As a string, for displaying on the UI
        public const string AS_STRING = "alpha-revived 0.10.1";
        //Something stupid, usually unique for every version
        public const string TAGLINE = "lettemball";
#endif
    }
}
