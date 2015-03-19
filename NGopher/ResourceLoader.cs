namespace NGopher
{
    public static class ResourceLoader
    {
        private static readonly Windows.ApplicationModel.Resources.ResourceLoader _instance = new Windows.ApplicationModel.Resources.ResourceLoader();
        public static Windows.ApplicationModel.Resources.ResourceLoader Instance { get { return _instance; } }
    }
}