# CopyWords (Danish)

This is a helper tool for adding new Danish words into [Anki](http://ankisrs.net/), a powerful, intelligent flashcard system.

I have customized Anki's card template to include additional information about a word, such as transcription, sound file, and more. The CopyWords tool simplifies the process of adding new cards by automatically searching for a word in an online Danish dictionary and parsing different parts of it. You can then click on various "Copy text" buttons to copy the information to the clipboard and paste it into the Anki editor.

## How to Use

1. Type a word (or part of it) in the Search box and click the **Search** button.
2. If the word exists in the [Den Danske Ordbog](http://ordnet.dk/ddo/) online dictionary, it will be parsed and presented within the tool.
3. Use the **Copy** buttons to copy the relevant text into the clipboard and paste it into the Anki card editor.

![Copy word parts into Anki](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Copy_word_parts.png)

## Card Templates

Anki card templates can be customized but unfortunately, they cannot be exported directly. Templates are saved with a "deck," which is a set of cards the user wants to learn. Users can add new cards with new words to their collection, making learning and memorization more efficient.

My card templates:

- **Forward Card Template**: The word in the foreign language is shown, and you need to guess the translation.
  ![Forward card](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Card_template_forward.png)

- **Reverse Card Template**: The translation is shown, and you need to type the word in the foreign language.
  ![Reverse card](https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/img/Card_template_reverse.png)

  See examples:

  - [front.html](<(https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/card_templates/front.html)>)
  - [back.html](<(https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/card_templates/back.html)>)
  - [styling.css](<(https://raw.githubusercontent.com/evgenygunko/CopyWordsDA/master/card_templates/styling.css)>)

## Sound Functionality

- The **Play sound** button downloads an MP3 file (if available) and plays it.
- The **Save sound** button normalizes the sound file (adjusts its volume) and saves it into Anki's media collection folder.

## Translations

The app can call an Azure Function App (refer to this repository: [Azure Translations](https://github.com/evgenygunko/Translations/tree/master)) to translate headlines and definitions. This feature can be enabled or disabled in the **Settings**.

## Settings

The settings dialog can be accessed by clicking the **Settings** button in the toolbar.

- **Path to Anki Media Collection**: Specifies the path to Anki's media collection for the current user (this folder contains media files displayed or played on the cards).
- **Path to mp3gain**: Specifies the path to the [mp3gain](http://mp3gain.sourceforge.net/) utility, which is used for normalizing the sound volume in MP3 files.
