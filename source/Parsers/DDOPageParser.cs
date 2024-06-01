using System.Text.RegularExpressions;
using CopyWords.Parsers.Models;

namespace CopyWords.Parsers
{
    public interface IDDOPageParser : IPageParser
    {
        List<VariationUrl> ParseVariationUrls();

        string ParseWord();

        string ParseEndings();

        string ParsePronunciation();

        string ParseSound();

        string ParseDefinitions();

        List<string> ParseExamples();
    }

    public class DDOPageParser : PageParserBase, IDDOPageParser
    {
        /// <summary>
        /// Gets the words count for given search.
        /// </summary>
        /// <returns>The words count.</returns>
        public List<VariationUrl> ParseVariationUrls()
        {
            var div = FindElementById("opslagsordBox_expanded");

            var searchResultBoxDiv = div.SelectSingleNode("./div/div[contains(@class, 'searchResultBox')]");
            if (searchResultBoxDiv == null)
            {
                throw new PageParserException("Cannot find a div element with CSS class 'searchResultBox'");
            }

            List<VariationUrl> variationUrls = new List<VariationUrl>();

            var ahrefNodes = searchResultBoxDiv.SelectNodes("./div/a");

            foreach (var ahref in ahrefNodes)
            {
                if (ahref != null && ahref.Attributes["href"] != null)
                {
                    string word = ahref.ParentNode
                        .InnerText
                        .Trim();

                    // replace any sequence of spaces or tabs with a single space
                    char[] separators = new char[] { ' ', '\r', '\t', '\n' };

                    string[] temp = word.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    word = string.Join(" ", temp);

                    // decorade variant number with parenthesis
                    Match match = Regex.Match(word, "([0-9]+)");
                    if (match.Success)
                    {
                        string v = match.Groups[0].Value;
                        word = word.Replace(v, $"({v})");
                    }

                    word = word.Replace("&nbsp;", " ->");

                    string variationUrl = DecodeText(ahref.Attributes["href"].Value);

                    variationUrls.Add(new VariationUrl(word, variationUrl));
                }
            }

            return variationUrls;
        }

        /// <summary>
        /// Gets a string which contains found Danish word.
        /// </summary>
        public string ParseWord()
        {
            var div = FindElementByClassName("div", "definitionBoxTop");

            var wordSpan = div.SelectSingleNode("//*[contains(@class, 'match')]");
            if (wordSpan == null)
            {
                throw new PageParserException("Cannot find a span element with CSS class 'match'");
            }

            return DecodeText(wordSpan.InnerText);
        }

        /// <summary>
        /// Gets endings for found word.
        /// </summary>
        public string ParseEndings()
        {
            string endings = string.Empty;

            var div = FindElementById("id-boj");

            if (div != null)
            {
                var spanEndings = div.SelectSingleNode("./span[contains(@class, 'tekstmedium allow-glossing')]");
                if (spanEndings != null)
                {
                    endings = spanEndings.InnerText;
                }
            }

            return DecodeText(endings);
        }

        /// <summary>
        /// Gets pronunciation for found word.
        /// </summary>
        public string ParsePronunciation()
        {
            string pronunciation = string.Empty;

            var div = FindElementById("id-udt");

            if (div != null)
            {
                var span = div.SelectSingleNode("./span/span[contains(@class, 'lydskrift')]");
                if (span != null)
                {
                    pronunciation = DecodeText(span.InnerText);
                }
            }

            return pronunciation;
        }

        /// <summary>
        /// Gets path to sound file for found word (which would be an URL).
        /// </summary>
        public string ParseSound()
        {
            string soundUrl = string.Empty;

            var div = FindElementById("id-udt");

            if (div != null)
            {
                var ahref = div.SelectSingleNode("./span/span/audio/div/a");
                if (ahref != null && ahref.Attributes["href"] != null)
                {
                    soundUrl = ahref.Attributes["href"].Value;

                    if (!soundUrl.EndsWith(".mp3"))
                    {
                        throw new PageParserException(
                            string.Format("Sound URL must have '.mp3' at the end. Parsed value = '{0}'", soundUrl));
                    }
                }
            }

            return soundUrl;
        }

        /// <summary>
        /// Gets definitions for found word. It will concatenate different definitions into one string with line breaks.
        /// </summary>
        public string ParseDefinitions()
        {
            string definitions = string.Empty;

            var contentBetydningerDiv = FindElementById("content-betydninger");

            if (contentBetydningerDiv != null)
            {
                var definitionsDivs = contentBetydningerDiv.SelectNodes("./div/div/span/span[contains(@class, 'definition')]");

                if (definitionsDivs != null && definitionsDivs.Count > 0)
                {
                    for (int i = 0; i < definitionsDivs.Count; i++)
                    {
                        if (definitionsDivs.Count > 1)
                        {
                            definitions += string.Format("{0}{1}. {2}", Environment.NewLine, (i + 1).ToString(), DecodeText(definitionsDivs[i].InnerText));
                        }
                        else
                        {
                            definitions += string.Format("{0}", DecodeText(definitionsDivs[i].InnerText));
                        }
                    }
                }
            }

            return definitions.Trim();
        }

        /// <summary>
        /// Gets examples for found word. It will also add a full stop at the end of each example.
        /// </summary>
        public List<string> ParseExamples()
        {
            List<string> examples = new List<string>();

            var contentBetydningerDiv = FindElementById("content-betydninger");

            if (contentBetydningerDiv != null)
            {
                // can't run XPath with a search for any depth - we want to take examples only from "top level" meaning
                // var examplesDivs = contentBetydningerDiv.SelectNodes("descendant::span[@class='citat']");
                var examplesDivs = contentBetydningerDiv.SelectNodes("./div/div/div/span[@class='citat']");
                if (examplesDivs == null)
                {
                    examplesDivs = contentBetydningerDiv.SelectNodes("./div/div/div/div/span[@class='citat']");
                }

                if (examplesDivs != null)
                {
                    for (int i = 0; i < examplesDivs.Count; i++)
                    {
                        string example = DecodeText(examplesDivs[i].InnerText);
                        if ((example.EndsWith(".") || example.EndsWith("!") || example.EndsWith("?")) == false)
                        {
                            example += ".";
                        }

                        examples.Add(string.Format("{0}. {1}", (i + 1).ToString(), example));
                    }
                }
            }

            return examples;
        }
    }
}
