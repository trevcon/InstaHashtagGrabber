namespace InstaHashtagGrabber
{
    public static class Utils
    {
        public static string GetPath(string path)
        {
            string basePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(basePath), path);
        }
    }
}
