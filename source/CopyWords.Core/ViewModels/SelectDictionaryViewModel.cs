﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class SelectDictionaryViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        public SelectDictionaryViewModel(
            ISettingsService settingsService)
        {
            _settingsService = settingsService;

            Parsers = new ObservableCollection<Models.Parsers>();
            Parsers.Add(new Models.Parsers("Den Danske Ordbog", "flag_of_denmark.png", SourceLanguage.Danish));
            Parsers.Add(new Models.Parsers("Spanish Dict", "flag_of_spain.png", SourceLanguage.Spanish));

            SelectedParser = Parsers.FirstOrDefault(x => x.SourceLanguage.ToString() == _settingsService.LoadSettings().SelectedParser);
            if (SelectedParser is null)
            {
                SelectedParser = Parsers[0];
            }
        }

        public ObservableCollection<Models.Parsers> Parsers { get; }

        [ObservableProperty]
        private Models.Parsers selectedParser;

        partial void OnSelectedParserChanged(Models.Parsers value)
        {
            SaveSelectedParserInSettings(value);
        }

        internal void SaveSelectedParserInSettings(Models.Parsers value)
        {
            AppSettings appSettings = _settingsService.LoadSettings();
            appSettings.SelectedParser = value.SourceLanguage.ToString();
            _settingsService.SaveSettings(appSettings);

            Debug.WriteLine($"Selected parser has changed to '{value.Name}'");
        }
    }
}
