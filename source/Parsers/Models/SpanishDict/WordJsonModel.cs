#nullable disable

namespace CopyWords.Parsers.Models.SpanishDict
{
#pragma warning disable CA1819 // Properties should not return arrays

    public class WordJsonModel
    {
        public string altLangUrl { get; set; }
        public Wordofthedaydata wordOfTheDayData { get; set; }
        public string authHost { get; set; }
        public string facebookAppId { get; set; }
        public string googleAppId { get; set; }
        public string siteUrlBase { get; set; }
        public int testGroup { get; set; }
        public Hostconfig hostConfig { get; set; }
        public bool maintenanceMode { get; set; }
        public string learningLang { get; set; }
        public string uiLang { get; set; }
        public bool isMobile { get; set; }
        public Experimentregistry experimentRegistry { get; set; }
        public long serverNowAtPageview { get; set; }
        public bool isApp { get; set; }
        public string action { get; set; }
        public string dictDefaultLang { get; set; }
        public string langFrom { get; set; }
        public string langTo { get; set; }
        public string queryText { get; set; }
        public bool hasBothLangs { get; set; }
        public Navitem[] navItems { get; set; }
        public Resultcardheaderprops resultCardHeaderProps { get; set; }
        public Sddictionaryresultsprops sdDictionaryResultsProps { get; set; }
        public Usagenoteandarticlesprops usageNoteAndArticlesProps { get; set; }
        public Searchboxprops searchBoxProps { get; set; }
        public Phras[] phrases { get; set; }
        public Sdwordrootprops sdWordRootProps { get; set; }
        public Sdexamplesprops sdExamplesProps { get; set; }
        public Verb verb { get; set; }
        public string verbLang { get; set; }
        public Verbtenses verbTenses { get; set; }
        public object authoringProps { get; set; }
        public object sdQuicktipProps { get; set; }
        public Conjugationprops conjugationProps { get; set; }
        public Lockedpremiumcontentprops lockedPremiumContentProps { get; set; }
        public string adList { get; set; }
        public Dictionarypossibleresult[] dictionaryPossibleResults { get; set; }
        public Sdinflectionprops sdInflectionProps { get; set; }
    }

    public class Wordofthedaydata
    {
        public string wordSource { get; set; }
        public string wordDisplay { get; set; }
        public string translationText { get; set; }
        public string audioUrl { get; set; }
    }

    public class Hostconfig
    {
        public string authHost { get; set; }
        public string classroomHost { get; set; }
        public string examplesHost { get; set; }
        public string facebookAppId { get; set; }
        public string googleAppId { get; set; }
        public string audioHost { get; set; }
        public string hegemoneAssetHost { get; set; }
        public string hegemoneApplicationHost { get; set; }
        public string leaderboardsHost { get; set; }
        public string neodarwinAssetHost { get; set; }
        public string playgroundHost { get; set; }
        public string scribeHost { get; set; }
        public string siteHost { get; set; }
        public string suggestHost { get; set; }
        public string traductorHost { get; set; }
    }

    public class Experimentregistry
    {
        public bool coreWebVitals { get; set; }
        public bool sdIdentity { get; set; }
        public bool sdCss { get; set; }
        public bool sdPrimis { get; set; }
        public int adhesionMarketingInt { get; set; }
        public bool sdParty { get; set; }
        public object reduxlessVocabQuiz { get; set; }
        public object voiceTranslation { get; set; }
        public Randomizedexperiments randomizedExperiments { get; set; }
    }

    public class Randomizedexperiments
    {
    }

    public class Resultcardheaderprops
    {
        public Headwordandquickdefsprops headwordAndQuickdefsProps { get; set; }
        public Addtolistprops addToListProps { get; set; }
    }

    public class Headwordandquickdefsprops
    {
        public Headword headword { get; set; }
        public Quickdef1 quickdef1 { get; set; }
        public object quickdef2 { get; set; }
    }

    public class Headword
    {
        public string displayText { get; set; }
        public string textToPronounce { get; set; }
        public string audioUrl { get; set; }
        public Pronunciation[] pronunciations { get; set; }
        public string wordLang { get; set; }
        public string type { get; set; }
    }

    public class Pronunciation
    {
        public int id { get; set; }
        public string ipa { get; set; }
        public string abc { get; set; }
        public string spa { get; set; }
        public string region { get; set; }
        public int hasVideo { get; set; }
        public int? speakerId { get; set; }
        public int? version { get; set; }
    }

    public class Quickdef1
    {
        public string displayText { get; set; }
        public string textToPronounce { get; set; }
        public string audioUrl { get; set; }
        public Pronunciation1[] pronunciations { get; set; }
        public string wordLang { get; set; }
        public string type { get; set; }
    }

    public class Pronunciation1
    {
        public int id { get; set; }
        public string ipa { get; set; }
        public string abc { get; set; }
        public string spa { get; set; }
        public string region { get; set; }
        public int hasVideo { get; set; }
        public int? speakerId { get; set; }
        public int? version { get; set; }
        public string source { get; set; }
    }

    public class Addtolistprops
    {
        public Word word { get; set; }
        public int userCount { get; set; }
        public bool shouldShow { get; set; }
        public bool shouldBeDisabled { get; set; }
        public bool initialIsModalOpen { get; set; }
    }

    public class Word
    {
        public int id { get; set; }
        public string source { get; set; }
        public string lang { get; set; }
    }

    public class Sddictionaryresultsprops
    {
        public Entry entry { get; set; }
        public Pronunciationdictionarylink pronunciationDictionaryLink { get; set; }
        public string hegemoneAssetHost { get; set; }
        public string entryLang { get; set; }
        public string defaultLang { get; set; }
    }

    public class Entry
    {
        public string chambers { get; set; }
        public string collins { get; set; }
        public Neodict[] neodict { get; set; }
        public Neoharrap[] neoharrap { get; set; }
        public string velazquez { get; set; }
    }

    public class Neodict
    {
        public string subheadword { get; set; }
        public Posgroup[] posGroups { get; set; }
    }

    public class Posgroup
    {
        public Pos pos { get; set; }
        public string entryLang { get; set; }
        public object gender { get; set; }
        public Sens[] senses { get; set; }
        public Posdisplay posDisplay { get; set; }
    }

    public class Pos
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Posdisplay
    {
        public string name { get; set; }
        public Tooltip tooltip { get; set; }
    }

    public class Tooltip
    {
        public string def { get; set; }
        public string href { get; set; }
    }

    public class Sens
    {
        public string contextEn { get; set; }
        public string contextEs { get; set; }
        public object gender { get; set; }
        public int id { get; set; }
        public Partofspeech partOfSpeech { get; set; }
        public Region[] regions { get; set; }
        public Registerlabel[] registerLabels { get; set; }
        public Translation[] translations { get; set; }
        public string subheadword { get; set; }
        public int idx { get; set; }
        public string context { get; set; }
        public Regionsdisplay[] regionsDisplay { get; set; }
        public Registerlabelsdisplay[] registerLabelsDisplay { get; set; }
        public Translationsdisplay[] translationsDisplay { get; set; }
    }

    public class Partofspeech
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Region
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Registerlabel
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Translation
    {
        public string contextEn { get; set; }
        public string contextEs { get; set; }
        public Example[] examples { get; set; }
        public object gender { get; set; }
        public int id { get; set; }
        public string imagePath { get; set; }
        public bool isOppositeLanguageHeadword { get; set; }
        public bool isQuickTranslation { get; set; }
        public Region[] regions { get; set; }
        public Registerlabel1[] registerLabels { get; set; }
        public string translation { get; set; }
    }

    public class Example
    {
        public string textEn { get; set; }
        public string textEs { get; set; }
    }

    public class Registerlabel1
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Regionsdisplay
    {
        public string name { get; set; }
        public Tooltip1 tooltip { get; set; }
    }

    public class Tooltip1
    {
        public string def { get; set; }
    }

    public class Registerlabelsdisplay
    {
        public string name { get; set; }
        public Tooltip2 tooltip { get; set; }
    }

    public class Tooltip2
    {
        public string def { get; set; }
    }

    public class Translationsdisplay
    {
        public string translation { get; set; }
        public object gender { get; set; }
        public bool isQuickTranslation { get; set; }
        public string imagePath { get; set; }
        public Translationsdisplay1 translationsDisplay { get; set; }
        public string letters { get; set; }
        public string context { get; set; }
        public object[] regionsDisplay { get; set; }
        public Registerlabelsdisplay1[] registerLabelsDisplay { get; set; }
        public string[][] examplesDisplay { get; set; }
    }

    public class Translationsdisplay1
    {
        public string[] texts { get; set; }
        public object[] tooltips { get; set; }
    }

    public class Registerlabelsdisplay1
    {
        public string name { get; set; }
        public Tooltip3 tooltip { get; set; }
    }

    public class Tooltip3
    {
        public string def { get; set; }
    }

    public class Neoharrap
    {
        public string subheadword { get; set; }
        public Posgroup1[] posGroups { get; set; }
    }

    public class Posgroup1
    {
        public Pos1 pos { get; set; }
        public string entryLang { get; set; }
        public object gender { get; set; }
        public Sens1[] senses { get; set; }
        public Posdisplay1 posDisplay { get; set; }
    }

    public class Pos1
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Posdisplay1
    {
        public string name { get; set; }
        public Tooltip1 tooltip { get; set; }
    }

    public class Sens1
    {
        public string contextEs { get; set; }
        public Region[] regions { get; set; }
        public Translation1[] translations { get; set; }
        public string contextEn { get; set; }
        public Partofspeech1 partOfSpeech { get; set; }
        public string subheadword { get; set; }
        public int idx { get; set; }
        public string context { get; set; }
        public object[] regionsDisplay { get; set; }
        public object[] registerLabelsDisplay { get; set; }
        public Translationsdisplay2[] translationsDisplay { get; set; }
    }

    public class Partofspeech1
    {
        public string abbrEn { get; set; }
        public string abbrEs { get; set; }
        public string nameEn { get; set; }
        public string nameEs { get; set; }
    }

    public class Translation1
    {
        public Region[] regions { get; set; }
        public string translation { get; set; }
        public Example1[] examples { get; set; }
    }

    public class Example1
    {
        public string textEs { get; set; }
        public string textEn { get; set; }
    }

    public class Translationsdisplay2
    {
        public string translation { get; set; }
        public Translationsdisplay3 translationsDisplay { get; set; }
        public string letters { get; set; }
        public object[] regionsDisplay { get; set; }
        public object[] registerLabelsDisplay { get; set; }
        public string[][] examplesDisplay { get; set; }
    }

    public class Translationsdisplay3
    {
        public string[] texts { get; set; }
        public object[] tooltips { get; set; }
    }

    public class Pronunciationdictionarylink
    {
        public string uiLang { get; set; }
        public string word { get; set; }
        public string regionChoice { get; set; }
        public string spellingChoice { get; set; }
        public string[] pronunciationSpellings { get; set; }
        public string wordLang { get; set; }
        public string dictDefaultLang { get; set; }
    }

    public class Usagenoteandarticlesprops
    {
        public object[] linkedArticles { get; set; }
    }

    public class Searchboxprops
    {
        public string initialSearchText { get; set; }
        public string action { get; set; }
        public string category { get; set; }
        public string traductorHost { get; set; }
        public int traductorTimeoutMs { get; set; }
        public Searchquicklinksection[] searchQuickLinkSections { get; set; }
        public bool isMTPage { get; set; }
        public bool stickySearchBarEnabled { get; set; }
    }

    public class Searchquicklinksection
    {
        public string title { get; set; }
        public string action { get; set; }
        public string[] words { get; set; }
    }

    public class Sdwordrootprops
    {
        public string wordLang { get; set; }
        public int wordId { get; set; }
    }

    public class Sdexamplesprops
    {
        public object examplesSalt { get; set; }
        public string query { get; set; }
        public string originalQuery { get; set; }
        public string forcedLanguage { get; set; }
        public string pageCategory { get; set; }
    }

    public class Verb
    {
        public int id { get; set; }
        public string infinitive { get; set; }
        public string matchedConjugation { get; set; }
        public int isReflexive { get; set; }
        public bool isReflexiveVariation { get; set; }
        public int verbType { get; set; }
        public int supportsConjugationDrills { get; set; }
        public string reason { get; set; }
        public string infinitiveTranslation { get; set; }
        public Pastparticiple pastParticiple { get; set; }
        public Gerund gerund { get; set; }
        public Paradigms paradigms { get; set; }
    }

    public class Pastparticiple
    {
        public string word { get; set; }
        public string translation { get; set; }
    }

    public class Gerund
    {
        public string word { get; set; }
        public string translation { get; set; }
    }

    public class Paradigms
    {
        public Presentindicative[] presentIndicative { get; set; }
        public Preteritindicative[] preteritIndicative { get; set; }
        public Imperfectindicative[] imperfectIndicative { get; set; }
        public Conditionalindicative[] conditionalIndicative { get; set; }
        public Futureindicative[] futureIndicative { get; set; }
    }

    public class Presentindicative
    {
        public string word { get; set; }
        public string translation { get; set; }
        public string pronoun { get; set; }
        public string audioQueryString { get; set; }
    }

    public class Preteritindicative
    {
        public string word { get; set; }
        public string translation { get; set; }
        public string pronoun { get; set; }
        public string audioQueryString { get; set; }
    }

    public class Imperfectindicative
    {
        public string word { get; set; }
        public string translation { get; set; }
        public string pronoun { get; set; }
        public string audioQueryString { get; set; }
    }

    public class Conditionalindicative
    {
        public string word { get; set; }
        public string translation { get; set; }
        public string pronoun { get; set; }
        public string audioQueryString { get; set; }
    }

    public class Futureindicative
    {
        public string word { get; set; }
        public string translation { get; set; }
        public string pronoun { get; set; }
        public string audioQueryString { get; set; }
    }

    public class Verbtenses
    {
        public Indicative Indicative { get; set; }
    }

    public class Indicative
    {
        public Label label { get; set; }
        public Mood[] moods { get; set; }
    }

    public class Label
    {
        public string name { get; set; }
        public string href { get; set; }
    }

    public class Mood
    {
        public string name { get; set; }
        public string title { get; set; }
        public string href { get; set; }
    }

    public class Conjugationprops
    {
        public bool showVosConjugations { get; set; }
        public bool showVosotrosConjugations { get; set; }
    }

    public class Lockedpremiumcontentprops
    {
        public string displayType { get; set; }
        public string analyticsType { get; set; }
        public string title { get; set; }
    }

    public class Sdinflectionprops
    {
        public string wordLang { get; set; }
        public int wordId { get; set; }
    }

    public class Navitem
    {
        public string label { get; set; }
        public bool active { get; set; }
        public string href { get; set; }
        public string path { get; set; }
        public string token { get; set; }
        public bool isVerbInBothLangs { get; set; }
    }

    public class Phras
    {
        public string source { get; set; }
        public string sourceAudioQueryString { get; set; }
        public string quickdef { get; set; }
        public string quickdefAudioQueryString { get; set; }
        public string quickdef1 { get; set; }
        public string quickdef1AudioQueryString { get; set; }
        public string quickdef2AudioQueryString { get; set; }
        public string quickdef2 { get; set; }
    }

    public class Dictionarypossibleresult
    {
        public int wordId { get; set; }
        public string wordSource { get; set; }
        public string resultHeuristic { get; set; }
        public string result { get; set; }
        public string translation1 { get; set; }
        public Audiourls audioUrls { get; set; }
    }

    public class Audiourls
    {
        public string result { get; set; }
        public string translation1 { get; set; }
    }

#pragma warning restore CA1819 // Properties should not return arrays
}
