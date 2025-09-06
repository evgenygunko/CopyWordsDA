# Copy Mode and Anki Integration

CopyWords was originally created to make it easier to add new Danish or Spanish words to the [Anki](http://ankisrs.net/) flashcard system.
In **Copy Mode**, the app automatically extracts word details from online dictionaries so you can quickly copy them into your Anki cards.

---

## How to Use Copy Mode

1. Select the source language.
2. Type a word (or part of it) in the **Search** box and click **Search**.
3. If the word exists in the online dictionary, its details will be parsed and displayed.
4. Use the **Copy** buttons to copy the relevant text to the clipboard.
5. Paste the text into the Anki card editor.

**Note:**
The screenshot is taken on Windows, as it is easier to copy different parts of a word there.
On Android, you can copy the **front** and **back** fields.

![Copy word parts into Anki](./img/Copy_word_parts.png)

---

## Sound Functionality

- **Play sound**: Downloads an MP3 file (if available) and plays it.
- **Save sound**: Normalizes the sound file (adjusting its volume) and saves it to Anki's media collection folder.

> Normalizing requires [mp3Gain](http://mp3gain.sourceforge.net/), which is optional.

---

## Settings

The settings dialog can be opened by clicking the **Settings** button in the app bar.

![Settings](./img/CopyWords_settings_Windows.png)

- **Export**: Save settings as a JSON file.
- **Import**: Load settings from a JSON file.
- **Path to Anki Media Collection** _(only relevant for Copy Mode)_
- **Path to FFMpeg**: Location of the [ffmpeg](https://www.ffmpeg.org/) utility, used to convert audio files.
- **Path to MP3Gain**: Location of the [mp3gain](http://mp3gain.sourceforge.net/) utility, used to normalize MP3 volume. _(Optional)_
- **Show copy buttons**: Switch between dictionary mode and copy mode.

---

## Related

- [Main README (Dictionary Mode)](./README.md)
- [Card Templates](./README_CARD_TEMPLATES.md)
