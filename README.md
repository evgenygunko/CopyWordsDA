# CopyWords (Danish)

This is a helper tool for adding new Danish words into [Anki](http://ankisrs.net/).

I have customized Anki's card template to contain additional information about a word, such as transcription, sound file, etc. The CopyWords tool makes it easier to fill a new card by automatically finding a word in an online Danish dictionary and parsing different parts. Then you can click on various "Copy text" buttons to copy text into the clipboard and paste it into the Anki editor.

First, type a word or its part in the "Search" box and click the "Search" button. If this word exists in the [Den Danske Ordbog](http://ordnet.dk/ddo/) online dictionary, it will be parsed and presented in the tool.
Use the "copy" buttons to copy relevant text into the clipboard and paste it into the Anki card editor.

![Copy word parts into Anki](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Copy_word_parts.png)

#### Card Templates

A card template can be created in Anki but unfortunately cannot be exported. It is saved with a "deck," which is a set of cards that a user wants to learn.
A user adds new cards with new words into the collection and then will learn and remember them.

My card templates:

- Forward card template (when a word in a foreign language is shown and you need to guess the translation) ![Forward card](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Card_template_forward.png)
- Reverse card template (when the translation is shown and you need to type the word in the foreign language) ![Reverse card](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Card_template_reverse.png)

## Sound

The "Play sound" button will download an mp3 file (if it exists) and play it.
The "Save sound" button will normalize the sound file (change its volume) and save it into Anki's media collection folder.

## Settings

The settings dialog can be opened by clicking on the Settings button in the toolbar.

- "Path to Anki media collection" specifies a path to Anki's media collection for the current user (it contains media files which are displayed or played on the cards).
- "Path to mp3gain" specifies a path to the [mp3gain](http://mp3gain.sourceforge.net/) utility, which is used for normalizing sound volume in mp3 files.
