﻿using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ExampleViewModel : ObservableObject
    {
        public ExampleViewModel(Example example)
        {
            Original = example?.Original;
            Translation = example?.Translation;
        }

        [ObservableProperty]
        private string original;

        [ObservableProperty]
        private string translation;

        [ObservableProperty]
        private bool isChecked;
    }
}
