# CopyWords

CopyWords is a helper tool designed for adding new Danish or Spanish words to the [Anki](http://ankisrs.net/) flashcard system.

The Anki card template has been customized to include additional details about a word, such as its transcription, sound file, and more. CopyWords simplifies the process of adding new cards by automatically searching for words in online dictionaries and parsing their components. Users can then use the "Copy text" buttons to copy specific information to the clipboard and paste it into the Anki editor.

The app is built using [Microsoft .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui) technology and is compatible with both Windows and macOS.


## How to Use

1. Select the source language. For Danish we will use the [Den Danske Ordbog](http://ordnet.dk/ddo/) and for Spanish we will use [SpanishDict](https://www.spanishdict.com/) online dictionary.
2. Type a word (or part of it) in the Search box and click the **Search** button.
3. If the word exists in the online dictionary, it will be parsed and presented within the tool.
4. Use the **Copy** buttons to copy the relevant text into the clipboard and paste it into the Anki card editor.

![Copy word parts into Anki](./img/Copy_word_parts.png)

## Sound Functionality

- The **Play sound** button downloads an MP3 file (if available) and plays it.
- The **Save sound** button normalizes the sound file (adjusting its volume) and saves it to Anki's media collection folder.Normalizing the sound requires a tool called mp3Gain and is optional.

## Translations

The app can call an Azure Function App (refer to this repository: [Azure Translations](https://github.com/evgenygunko/Translations/tree/master)) to translate headlines and definitions. This feature can be enabled or disabled in the **Settings**.

## Settings

The settings dialog can be accessed by clicking the **Settings** button in the app bar.

![Settings](./img/CopyWords_settings.png)

- **Export**: Exports current settings as a json file.
- **Import**: Imports settings from a json file.
- **Path to Anki Media Collection**: Specifies the path to Anki's media collection folder for the current user. This folder contains the media files displayed or played on Anki cards.
- **Path to FFMpeg**: Specifies the path to the [ffmpeg](https://www.ffmpeg.org/) utility, which is used to convert audio files from video formats to MP3.
- **Path to MP3Gain**: Specifies the path to the [mp3gain](http://mp3gain.sourceforge.net/) utility, which is used to normalize the sound volume of MP3 files. Using this tool is optional.
- **Translator API URL**: Specifies the URL for the [Translator app](https://github.com/evgenygunko/Translations) which can return translations in additional langugages. Using this tool is optional.

## Card Templates

Anki card templates can be customized but unfortunately, they cannot be exported directly. Templates are saved with a "deck," which is a set of cards the user wants to learn. Users can add new cards with new words to their collection, making learning and memorization more efficient.

1. Open Anki, click Tools -> Manage Note Types -> Add. Add a name for your new note type.
2. Select your note type and click "Fields". Add the following fields:
   - Front
   - Back
   - PartOfSpeech
   - Forms
   - Example
   - Sound
   - MyHint
   - Transcription
   - Synonymer

![Fields](./img/Note_type_fields.png)

3. Click cards and add Forward and Reverse cards:

    **Forward Card Template**
    The word in the foreign language is shown, and you need to guess the translation.
  - [front.html](./card_templates/Forward_card_front_template.html)
    ![Forward card front](./img/Forward_card_front_template.png)
  - [back.html](./card_templates/Forward_card_back_template.html)
    ![Forward card back](./img/Forward_card_back_template.png)
  - [styling.css](./card_templates/Forward_card_styling.css)
    ![Forward card styling](./img/Forward_card_styling.png)

    **Reverse Card Template**
    The translation is shown, and you need to type the word in the foreign language.
  - [front.html](./card_templates/Reverse_card_front_template.html)
    ![Reverse card front](./img/Reverse_card_front_template.png)
  - [back.html](./card_templates/Reverse_card_back_template.html)
    ![Reverse card back](./img/Reverse_card_back_template.png)
  - [styling.css](./card_templates/Reverse_card_styling.css)
    ![Reverse card styling](./img/Reverse_card_styling.png)
