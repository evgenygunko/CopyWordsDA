using System.Collections.ObjectModel;
using Autofac.Extras.Moq;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class CopySelectedToClipboardServiceTests
    {
        #region Tests for CompileFrontAsync

        [TestMethod]
        public async Task CompileFrontAsync_ForCocheWhenOneExampleSelected_ReturnsOneFrontMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("un coche");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForCocheWhenTwoExamplesSelected_ReturnsOneFrontMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[1].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("un coche");
            }
        }

        [TestMethod]
        public void CompileFrontAsync_ForAfeitarWhenMoreThanOneWordVariantSelected_ThrowsException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForAfeitar();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[1].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                sut.Invoking(x => x.CompileFrontAsync(wordVariantVMs))
                    .Should().ThrowAsync<PrepareWordForCopyingException>()
                    .WithMessage("Cannot copy front word, examples from several variants selected");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForMasculineNoun_AddsUn()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("un coche");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForFeminineNoun_AddsUna()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCasa();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("una casa");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForBien_AddsAdverbToTheEnd()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForBien();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("bien <span style=\"color: rgba(0, 0, 0, 0.4)\">ADVERB</span>");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForGuayWhenAdjectiveSelected_AddsAdjectiveToTheEnd()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGuay();
                wordVariantVMs[1].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[1].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[1].IsChecked = true;
                wordVariantVMs[1].ContextViewModels[0].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("guay <span style=\"color: rgba(0, 0, 0, 0.4)\">ADJECTIVE</span>");
            }
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForLuce_AddsPhraseToTheEnd()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForLuce();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[2].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(wordVariantVMs);

                front.Should().Be("luce <span style=\"color: rgba(0, 0, 0, 0.4)\">PHRASE</span>");
            }
        }

        #endregion

        #region Tests for CompileExamplesAsync

        [TestMethod]
        public async Task CompileExamplesAsync_ForCocheWhenOneExampleSelected_ReturnsExampleWithoutNumbering()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileExamplesAsync(wordVariantVMs);

                result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span>");
            }
        }

        [TestMethod]
        public async Task CompileExamplesAsync_ForCocheWhenTwoExampleSelected_ReturnsExamplesWithoutNumbering()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileExamplesAsync(wordVariantVMs);

                result.Should().Be(
                    "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span><br>" +
                    "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Todos estos coches tienen bolsas de aire.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">All these automobiles have airbags.</span>");
            }
        }

        #endregion

        #region Tests for CompileBackAsync

        [TestMethod]
        public async Task CompileBackAsync_ForCocheWhenOneExampleSelected_ReturnsOneBackMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("car (vehicle)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_ForCocheWhenTwoExamplesSelected_ReturnsOneBackMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForCoche();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[1].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be(
                    "1.&nbsp;car (vehicle)<br>" +
                    "2.&nbsp;coach (vehicle led by horses)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_ForAfeitarWhenMoreThanOneWordVariantSelected_ReturnsTwoBackMeanings()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForAfeitar();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[1].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be(
                    "1.&nbsp;to shave (to remove hair)<br>" +
                    "2.&nbsp;to shave (to shave oneself)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_ForBienWhenTwoContextsSelected_ReturnsTwoBackMeanings()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForBien();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[1].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be(
                    "1.&nbsp;well (in good health)<br>" +
                    "2.&nbsp;well (properly)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_ForGuayWhenTwoExamplesFromOneTranslationSelected_ReturnsOneBackMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGuay();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[1].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("cool (colloquial) (used to express approval) (Spain)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_ForGuayWhenTwoTranslationsSelected_ReturnsAddsContextOnlyToFirstBackMeaning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGuay();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[1].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be(
                    "1.&nbsp;cool (colloquial) (used to express approval) (Spain)<br>" +
                    "2.&nbsp;great (colloquial)");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsPresentAndSelected_CallsSaveImageFileService()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<ISaveImageFileService>().Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

                var wordVariantVMs = CreateVMForSaltamontes();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].IsImageChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ImageUrl.Should().NotBeNullOrEmpty();

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("grasshopper (animal)<br><img src=\"saltamontes.jpg\">");
                mock.Mock<ISaveImageFileService>().Verify(x => x.SaveImageFileAsync(wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ImageUrl, "saltamontes"));
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsPresentButNotSelected_DoesNotCallSaveImageFileService()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<ISaveImageFileService>().Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

                var wordVariantVMs = CreateVMForSaltamontes();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].IsImageChecked = false;
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ImageUrl.Should().NotBeNullOrEmpty();

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("grasshopper (animal)");
                mock.Mock<ISaveImageFileService>().Verify(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsNull_DoesNotCallSaveImageFileService()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<ISaveImageFileService>().Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

                var wordVariantVMs = CreateVMForLuce();
                wordVariantVMs[0].ContextViewModels[0].TranslationViewModels[0].ExampleViewModels[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("he looks (masculine) (third person singular)");
                mock.Mock<ISaveImageFileService>().Verify(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }

        #endregion

        #region Private Methods

        private static ObservableCollection<WordVariantViewModel> CreateVMForCoche()
        {
            var wordVariantVM = new WordVariantViewModel(new WordVariant("coche", "MASCULINE NOUN",
                new List<Context>
                {
                    new Context("(vehicle)", 1,
                        new List<Translation>
                        {
                            new Translation("car", "a", null,
                                new List<Example>() { new Example("Mi coche no prende porque tiene una falla en el motor.", "My car won't start because of a problem with the engine.") }),
                            new Translation("automobile", "b", null,
                                new List<Example>() { new Example("Todos estos coches tienen bolsas de aire.", "All these automobiles have airbags.") }),
                        }),

                    new Context("(vehicle led by horses)", 2,
                        new List<Translation>
                        {
                            new Translation("carriage", "a", null,
                                new List<Example>() { new Example("Los monarcas llegaron en un coche elegante.", "The monarchs arrived in an elegant carriage.") }),
                            new Translation("coach", "b", null,
                                new List<Example>() { new Example("Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.", "Horse-drawn coaches were used much more before the invention of the automobile.") }),
                        })
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForCasa()
        {
            var wordVariantVM = new WordVariantViewModel(new WordVariant("casa", "feminine noun",
                new List<Context>
                {
                    new Context("(dwelling)", 1,
                        new List<Translation>
                        {
                            new Translation("house", "a", null,
                                new List<Example>() { new Example("Vivimos en una casa con un gran jardín.", "We live in a house with a big garden.") }),
                        }),
                    // ...
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForAfeitar()
        {
            var wordVariantVM1 = new WordVariantViewModel(new WordVariant("afeitar", "TRANSITIVE VERB",
                new List<Context>
                {
                    new Context("(to remove hair)", 1,
                        new List<Translation>
                        {
                            new Translation("to shave", "a", null,
                                new List<Example>() { new Example("Para el verano, papá decidió afeitar al perro.", "For the summer, dad decided to shave the dog.") }),
                        }),
                }
            ));

            var wordVariantVM2 = new WordVariantViewModel(new WordVariant("afeitarse", "REFLEXIVE VERB",
                new List<Context>
                {
                    new Context("(to shave oneself)", 1,
                        new List<Translation>
                        {
                            new Translation("to shave", "a", null,
                                new List<Example>() { new Example("¿Con qué frecuencia te afeitas la barba?", "How often do you shave your beard?") }),
                        }),
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM1,
                wordVariantVM2
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForBien()
        {
            var wordVariantVM = new WordVariantViewModel(new WordVariant("bien", "ADVERB",
                new List<Context>
                {
                    new Context("(in good health)", 1,
                        new List<Translation>
                        {
                            new Translation("well", "a", null,
                                new List<Example>() { new Example("Últimamente no me he sentido bien.", "I haven't felt well lately.") }),
                        }),
                    new Context("(properly)", 2,
                        new List<Translation>
                        {
                            new Translation("well", "a", null,
                                new List<Example>() { new Example("Si la carne molida no se cocina bien, las bacterias no mueren.", "If the ground meat is not cooked well, the bacteria don't die.") }),
                        }),
                }
                // ...
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForGuay()
        {
            var wordVariantVM1 = new WordVariantViewModel(new WordVariant("guay", "INTERJECTION",
                new List<Context>
                {
                    new Context("(colloquial) (used to express approval) (Spain)", 1,
                        new List<Translation>
                        {
                            new Translation("cool (colloquial)", "a", null,
                                new List<Example>() {
                                    new Example("¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!", "Do you want to watch the movie on my computer? - Cool, man!"),
                                    new Example("¡Gané un viaje a Francia! - ¡Guay!", "I won a trip to France! - Cool!")
                                }),
                            new Translation("great (colloquial)", "b", null,
                                new List<Example>() { new Example("Puedes tomarte el día libre mañana. - ¡Guay!", "You can take the day off tomorrow. - Great!") }),
                        }),
                    // ...
                }
            ));

            var wordVariantVM2 = new WordVariantViewModel(new WordVariant("guay", "ADJECTIVE",
                new List<Context>
                {
                    new Context("(colloquial) (extremely good) (Spain)", 1,
                        new List<Translation>
                        {
                            new Translation("cool (colloquial)", "a", null,
                                new List<Example>() {
                                    new Example("La fiesta de anoche estuvo muy guay.", "Last night's party was really cool."),
                                    new Example("Tus amigos son guays, Roberto. ¿Dónde los conociste?", "Your friends are cool, Roberto. Where did you meet them?")
                                }),
                            new Translation("super (colloquial)", "b", null,
                                new List<Example>() { new Example("¡Que monopatín tan guay!", "That's a super skateboard!") }),
                        }),
                    // ...
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM1,
                wordVariantVM2
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForLuce()
        {
            var wordVariantVM1 = new WordVariantViewModel(new WordVariant("luce", "PHRASE",
                new List<Context>
                {
                    new Context("(third person singular)", 1,
                        new List<Translation>
                        {
                            new Translation("he looks (masculine)", "a", null,
                                new List<Example>() { new Example("Luce más fuerte. ¿Ha estado yendo al gimnasio?", "He looks stronger. Has he been going to the gym?"), }),
                            new Translation("she looks (feminine)", "b", null,
                                new List<Example>() { new Example("Luce muy bien con el pelo corto.", "She looks great with short hair.") }),
                            new Translation("it looks", "c", null,
                                new List<Example>() { new Example("¿Llevaste tu uniforme a la tintorería? Luce impecable el día de hoy.", "Did you take your uniform to the cleaners? It looks immaculate today.") }),
                        }),
                    new Context("(formal) (second person singular)", 2,
                        new List<Translation>
                        {
                            new Translation("you look", "a", null,
                                new List<Example>() { new Example("Luce muy elegante, Sra. Vargas. ¿Tiene planes para hoy?", "You look very elegant, Mrs. Vargas. Do you have plans for today?"), }),
                        })
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM1
            };

            return wordVariantVMs;
        }

        private static ObservableCollection<WordVariantViewModel> CreateVMForSaltamontes()
        {
            var wordVariantVM1 = new WordVariantViewModel(new WordVariant("saltamontes", "MASCULINE NOUN",
                new List<Context>
                {
                    new Context("(animal)", 1,
                        new List<Translation>
                        {
                            new Translation("grasshopper", "a", "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg",
                                new List<Example>() {
                                    new Example("Los saltamontes pueden saltar muy alto.", "Grasshoppers can jump really high.")
                                })
                        }),
                    // ...
                }
            ));

            var wordVariantVMs = new ObservableCollection<WordVariantViewModel>()
            {
                wordVariantVM1
            };

            return wordVariantVMs;
        }

        #endregion
    }
}
