﻿using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveSoundFileServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for CopyFileToAnkiFolderAsync

        [TestMethod]
        public async Task CopyFileToAnkiFolderAsync_WhenAnkiSoundsFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = "abc";

            string sourceFile = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.FileExists(sourceFile)).Returns(true);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(false);

            var sut = _fixture.Create<SaveSoundFileService>();

            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            result.Should().BeFalse();

            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify(x => x.DisplayAlert("Path to Anki folder is incorrect", "Cannot find path to Anki folder 'abc'. Please update it in Settings.", "OK"));
        }

        [TestMethod]
        public async Task CopyFileToAnkiFolderAsync_WhenDestinationFileExists_DisplaysAlert()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            string sourceFile = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.SetupSequence(x => x.FileExists(It.IsAny<string>()))
                .Returns(true) // source file
                .Returns(true); // destination file
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);

            var sut = _fixture.Create<SaveSoundFileService>();

            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            result.Should().BeFalse();

            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify(x => x.DisplayAlert("File already exists", It.IsAny<string>(), "Yes", "No"));
        }

        #endregion
    }
}