namespace CLP.Core
{
    public static class WhitelistManager
    {
        private static readonly List<string> _whitelist = new List<string>
            {
                "*.conf",
                "mirrorlist",
                "*.testfile" // For testing purposes
            };

        public static bool IsFileWhitelisted(string fileName)
        {
            return _whitelist.Any(pattern =>
                pattern == fileName ||
                (pattern.StartsWith("*") && fileName.EndsWith(pattern.Substring(1))));
        }
    }
}
