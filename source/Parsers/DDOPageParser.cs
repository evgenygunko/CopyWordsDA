using System.Text.RegularExpressions;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Models.DDO;
using HtmlAgilityPack;

namespace CopyWords.Parsers
{
    public interface IDDOPageParser : IPageParser
    {
        string ParseHeadword();

        string ParsePartOfSpeech();

        string ParseEndings();

        string ParsePronunciation();

        string ParseSound();

        List<DDODefinition> ParseDefinitions();

        List<Variant> ParseVariants();
    }

    public class DDOPageParser : PageParserBase, IDDOPageParser
    {
        internal const string DDOBaseUrl = "https://ordnet.dk/ddo/ordbog";

        #region Public Methods

        /// <summary>
        /// Gets a string which contains found Danish word.
        /// </summary>
        public string ParseHeadword()
        {
            string headWord = string.Empty;

            var div = FindElementByClassName("div", "definitionBoxTop");

            var spanHeadword = div.SelectSingleNode("//*[contains(@class, 'match')]");
            if (spanHeadword == null)
            {
                throw new PageParserException("Cannot find a span element with CSS class 'match'");
            }

            // Check if it has several meanings
            if (spanHeadword.InnerHtml.Contains("<span class=\"diskret\">"))
            {
                headWord += spanHeadword.InnerHtml.Replace("<span class=\"diskret\">", "")
                        .Replace("</span>", "");
            }
            else
            {
                headWord = spanHeadword.InnerHtml;
            }

            // Remove numbers added for additional meanings
            string pattern = @"<span class=""super"">\d<\/span>";
            headWord = Regex.Replace(headWord, pattern, string.Empty);

            headWord = DecodeText(headWord);
            return headWord;
        }

        public string ParsePartOfSpeech()
        {
            var div = FindElementByClassName("div", "definitionBoxTop");

            var wordSpan = div.SelectSingleNode("//*[contains(@class, 'tekstmedium allow-glossing')]");

            // Not all searches have part of the speech, e.g. "på højtryk" does not have one
            if (wordSpan != null)
            {
                return DecodeText(wordSpan.InnerText);
            }

            return string.Empty;
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
                    // Check if it has several meanings
                    if (spanEndings.InnerHtml.Contains("<span class=\"diskret\">"))
                    {
                        string[] meanings = spanEndings.InnerHtml.Split("<span class=\"dividerDouble\">&#160;</span>");
                        foreach (string meaning in meanings)
                        {
                            endings += meaning.Replace("<span class=\"diskret\">", "")
                                .Replace("</span>", "");
                            endings += "||";
                        }

                        endings = endings.TrimEnd("||".ToCharArray());
                    }
                    else
                    {
                        endings = spanEndings.InnerHtml.Replace("<span class=\"dividerDouble\">&#160;</span>", "||");
                    }
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
        public List<DDODefinition> ParseDefinitions()
        {
            List<DDODefinition> definitions = new();

            var div = FindElementById("content-betydninger");
            if (div == null)
            {
                // It is probably "Faste udtryk", try another way...
                div = FindElementByClassName("div", "artikel");
            }

            if (div != null)
            {
                var definitionsDivs = div.SelectNodes("./div/div/span/span[contains(@class, 'definition')]");

                if (definitionsDivs == null)
                {
                    // Sometimes the word is marked as "Faste udtryk" and has one more level of div blocks.
                    definitionsDivs = div.SelectNodes("./div/div/div/span/span[contains(@class, 'definition')]");
                }

                if (definitionsDivs != null && definitionsDivs.Count > 0)
                {
                    foreach (var definitionDiv in definitionsDivs)
                    {
                        string meaning = DecodeText(definitionDiv.InnerText);
                        string? tag = ParseDefinitionTag(definitionDiv);

                        // Parse examples only for this definition
                        IEnumerable<string> examples = ParseExamplesForDefinition(definitionDiv);
                        IEnumerable<Example> examplesList = examples.Select(x => new Example(Original: x, Translation: null));

                        definitions.Add(new DDODefinition(meaning, tag, examplesList));
                    }
                }
            }

            return definitions;
        }

        #endregion

        #region Private Methods

        private string? ParseDefinitionTag(HtmlNode divDefinition)
        {
            string? tag = null;

            var definitionIndentDiv = divDefinition.SelectNodes("ancestor::div[@class='definitionIndent']").FirstOrDefault();
            if (definitionIndentDiv != null)
            {
                var firstTag = definitionIndentDiv.SelectNodes("./div/span/span[@class='stempelNoBorder']")?.FirstOrDefault();
                if (firstTag != null)
                {
                    tag = DecodeText(firstTag.InnerText);
                }
            }

            return tag;
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

            if (examples.Count == 0)
            {
                examples.Add("-");
            }

            return examples;
        }

        /// <summary>
        /// Gets urls for the words variants.
        /// </summary>
        /// <returns>The words count.</returns>
        public List<Variant> ParseVariants()
        {
            var div = FindElementById("opslagsordBox_expanded");

            var searchResultBoxDiv = div.SelectSingleNode("./div/div[contains(@class, 'searchResultBox')]");
            if (searchResultBoxDiv == null)
            {
                throw new PageParserException("Cannot find a div element with CSS class 'searchResultBox'");
            }

            var variants = new List<Variant>();

            var ahrefNodes = searchResultBoxDiv.SelectNodes("./div/a");

            // Not all searches have word variants, e.g. "på højtryk" does not have one
            if (ahrefNodes != null)
            {
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

                        variants.Add(new Variant(word, variationUrl));
                    }
                }
            }

            return variants;
        }

        #endregion
    }
}
