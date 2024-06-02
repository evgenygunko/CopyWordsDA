using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CopyWords.Parsers.Tests
{
    public static class Helpers
    {
        public static string GetSimpleHTMLPage(string fileName)
        {
            return GetSimpleHTMLPage(fileName, System.Text.Encoding.UTF8);
        }

        public static string GetSimpleHTMLPage(string fileName, System.Text.Encoding encoding)
        {
            string fileContent;
            using (StreamReader sr = new StreamReader(fileName, encoding))
            {
                fileContent = sr.ReadToEnd();
            }

            Assert.IsFalse(string.IsNullOrEmpty(fileContent), $"Cannot read content of file '{fileName}'.");

            return fileContent;
        }
    }
}
