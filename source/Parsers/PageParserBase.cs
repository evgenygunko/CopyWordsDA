using System.Net;
using HtmlAgilityPack;

namespace CopyWords.Parsers
{
    public abstract class PageParserBase : IPageParser
    {
        private HtmlDocument? _htmlDocument;

        protected HtmlDocument? PageHtmlDocument
        {
            get { return _htmlDocument; }
        }

        public static string DecodeText(string innerText)
        {
            return WebUtility.HtmlDecode(innerText).Trim();
        }

        public void LoadHtml(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(content);

            if (_htmlDocument.DocumentNode == null)
            {
                throw new PageParserException("DocumentNode is null for the loaded stream, please check that it has a valid html content.");
            }
        }

        public HtmlNode FindElementByClassName(string elementName, string className)
        {
            var elements = FindElementsByClassName(elementName, className);
            return elements.First();
        }

        public HtmlNodeCollection FindElementsByClassName(string elementName, string className)
        {
            if (_htmlDocument == null)
            {
                throw new Exception("Please call LoadHtml() before using lookup");
            }

            var elements = _htmlDocument.DocumentNode.SelectNodes(
                string.Format("//{0}[contains(@class, '{1}')]", elementName, className));

            if (elements == null)
            {
                throw new PageParserException(string.Format("Cannot find any element '{0}' with CSS class '{1}'", elementName, className));
            }

            return elements;
        }

        public HtmlNode FindElementById(string id)
        {
            if (_htmlDocument == null)
            {
                throw new Exception("Please call LoadHtml() before using lookup");
            }

            var element = _htmlDocument.DocumentNode.SelectSingleNode(string.Format("//*[@id='{0}']", id));
            return element;
        }
    }
}
