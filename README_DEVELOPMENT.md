# CopyWords - Development Guide

This guide provides information for developers who want to contribute to CopyWords or run it locally.

---

## Running the Project Locally

To run CopyWords on your local machine, follow these steps:

### Prerequisites

1. Clone the repository
2. **Install .NET MAUI workloads**:

   ```bash
   dotnet workload install maui
   ```

3. **Build the AnkiDroid API (Android only)**:

   CopyWords integrates with AnkiDroid on Android devices. You may need to build the AnkiDroid API module yourself:
   - Build the AnkiDroid "api" module AAR yourself from https://github.com/ankidroid/Anki-Android
   - Drop the resulting `*.aar` file into `source\CopyWords.MAUI\Platforms\Android\Jars\ankidroid-api.aar`

---

## Anki Integration

CopyWords integrates with Anki in two different ways, depending on the platform:

### AnkiDroid API (Android)

On Android, CopyWords uses the **AnkiDroid API** to communicate directly with the AnkiDroid app installed on the device.

- **Documentation**: https://github.com/ankidroid/Anki-Android/wiki/AnkiDroid-API
- **Source**: https://github.com/ankidroid/Anki-Android

The AnkiDroid API provides native Android integration and allows CopyWords to:

- Read deck and model (note type) lists from AnkiDroid
- Add notes directly to AnkiDroid decks
- Find duplicate notes
- Store media files (images and audio) in AnkiDroid's media collection

**Implementation**: See [AnkiContentApiWrapper.cs](source/CopyWords.MAUI/Services/AnkiContentApiWrapper.cs)

**Requirements**:

- AnkiDroid must be installed on the Android device
- App requires `com.ichi2.anki.permission.READ_WRITE_DATABASE` permission
- You need to build the AnkiDroid API AAR file (see Prerequisites above)

### AnkiConnect (Windows/Mac)

On desktop platforms (Windows and Mac), CopyWords uses **AnkiConnect**, a plugin for Anki Desktop that provides an HTTP API.

- **Documentation**: https://git.sr.ht/~foosoft/anki-connect
- **Plugin**: https://ankiweb.net/shared/info/2055492159

AnkiConnect runs as a local web server (default: `http://127.0.0.1:8765`) and provides the following capabilities:

- Retrieve deck and note type names
- Add and update notes
- Open the note editor GUI in Anki
- Access Anki's media directory for storing audio and images

**Implementation**: See [AnkiConnectService.cs](source/CopyWords.Core/Services/AnkiConnectService.cs)

**Requirements**:

- Anki Desktop must be running
- AnkiConnect plugin must be installed in Anki Desktop
- No additional configuration needed (uses default endpoint)

---

## Translations Service

CopyWords requires a companion web service to provide word definitions and translations. This service translates dictionary headlines and definitions using OpenAI language models.

- **Repository**: https://github.com/evgenygunko/Translations
- **Purpose**: Translates word definitions, headlines, and examples from Danish/Spanish to the user's target language

### Configuration

Configure the service URL and authentication in `appsettings.json`:

```json
{
  "TranslatorAppUrl": "https://your-translator-service.com",
  "TranslatorAppRequestCode": "your-api-key"
}
```

**Note**: The Translations service is required for the "Copy Mode" feature. Without it, CopyWords will still function in "Dictionary Mode" but won't be able to provide translated content for Anki flashcards.

---

## Syncfusion Controls

CopyWords uses Syncfusion controls, primarily the **Autocomplete** control for enhanced search functionality.

### License Requirement

If you want to use Syncfusion components in your own application, you will need a license. The free **Community License** should be sufficient for most non-commercial use cases.

Learn more and apply for a free license at: https://www.syncfusion.com/products/communitylicense

---

## Helper Services

CopyWords integrates with the following optional services:

### LaunchDarkly (Optional)

**LaunchDarkly** is a feature flag management platform that allows dynamic control of application features without redeployment.

- Website: https://launchdarkly.com/
- The app will work without LaunchDarkly configuration
- Leave `LaunchDarklyMobileKey` and `LaunchDarklyMemberId` empty in `appsettings.json` if not using

### Sentry (Optional)

**Sentry** is an error tracking and performance monitoring service that helps identify and diagnose issues in production.

- Website: https://sentry.io/
- The app will work without Sentry configuration
- Leave `SentryDsn` empty in `appsettings.json` if not using

---

## More Information

- [Main README](./README.md)
- [Copy Mode & Anki Integration](./README_COPY_MODE.md)
- [Card Templates for Anki](./README_CARD_TEMPLATES.md)
