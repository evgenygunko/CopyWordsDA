using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models;
using HtmlAgilityPack;

namespace CopyWords.Parsers
{
    public interface IDDOPageParser : IPageParser
    {
        string ParseHeadword();

        string ParseEndings();

        string ParsePronunciation();

        string ParseSound();

        List<Definition> ParseDefinitions();
    }

    public class DDOPageParser : PageParserBase, IDDOPageParser
    {
        /// <summary>
        /// Gets a string which contains found Danish word.
        /// </summary>
        public string ParseHeadword()
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
        public List<Definition> ParseDefinitions()
        {
            List<Definition> definitions = new();

            var contentBetydningerDiv = FindElementById("content-betydninger");

            if (contentBetydningerDiv != null)
            {
                var definitionsDivs = contentBetydningerDiv.SelectNodes("./div/div/span/span[contains(@class, 'definition')]");

                if (definitionsDivs != null && definitionsDivs.Count > 0)
                {
                    foreach (var div in definitionsDivs)
                    {
                        string meaning = DecodeText(div.InnerText);

                        // todo: parse examples only for this definition
                        IEnumerable<string> examples = ParseExamplesForDefinition(div);

                        definitions.Add(new Definition(Meaning: meaning, Examples: examples));
                    }
                }
            }

            return definitions;
        }

        /// <summary>
        /// Gets examples for given definition. It will also add a full stop at the end of each example.
        /// </summary>
        private List<string> ParseExamplesForDefinition(HtmlNode divDefinition)
        {
            List<string> examples = new List<string>();

            var definitionIndentDiv = divDefinition.SelectNodes("ancestor::div[@class='definitionIndent']").FirstOrDefault();

            if (definitionIndentDiv != null)
            {
                // can't run XPath with a search for any depth - we want to take examples only from "top level" meaning
                // var examplesDivs = contentBetydningerDiv.SelectNodes("descendant::span[@class='citat']");
                var examplesDivs = definitionIndentDiv.SelectNodes("./div/div/span[@class='citat']");
                if (examplesDivs == null)
                {
                    examplesDivs = definitionIndentDiv.SelectNodes("./div/div/div/span[@class='citat']");
                }

                if (examplesDivs != null)
                {
                    foreach (var div in examplesDivs)
                    {
                        string example = DecodeText(div.InnerText);
                        if ((example.EndsWith(".") || example.EndsWith("!") || example.EndsWith("?")) == false)
                        {
                            example += ".";
                        }

                        examples.Add(example);
                    }
                }
            }

            return examples;
        }
    }
}
