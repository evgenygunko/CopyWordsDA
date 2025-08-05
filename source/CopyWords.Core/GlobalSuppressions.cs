// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// .NET Code Analyzers
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.CancelAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.WordViewModel.CopyBackAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Reviewed>", Scope = "member", Target = "~P:CopyWords.Core.ViewModels.SettingsViewModel.CanUseFfmpeg")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Reviewed>", Scope = "member", Target = "~P:CopyWords.Core.ViewModels.SettingsViewModel.CanUseMp3gain")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Reviewed>", Scope = "member", Target = "~P:CopyWords.Core.ViewModels.SettingsViewModel.About")]
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.PickMp3gainPathAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.PickSettingsFilePathAsync~System.Threading.Tasks.Task{System.String}")]

[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.WordViewModel.CopyExamplesAsync~System.Threading.Tasks.Task")]

[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Need to see its value in debugger", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.PickSettingsFilePathAsync~System.Threading.Tasks.Task{System.String}")]
[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.WordViewModel.OpenCopyMenuAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.MainViewModel.UpdateUI(CopyWords.Core.Models.WordModel)")]
[assembly: SuppressMessage("Style", "IDE0032:Use auto property", Justification = "<Reviewed>", Scope = "member", Target = "~F:CopyWords.Core.ViewModels.MainViewModel._wordViewModel")]

// Roslynator
[assembly: SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception.", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.PickMp3gainPathAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.PickSettingsFilePathAsync~System.Threading.Tasks.Task{System.String}")]
[assembly: SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.SettingsViewModel.ExportSettingsAsync(System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Reviewed>", Scope = "member", Target = "~M:CopyWords.Core.ViewModels.MainViewModel.UpdateUI(CopyWords.Core.Models.WordModel)")]
