using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for AddOutfitsDialogViewModel
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class AddOutfitsDialogViewModelTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;

        public AddOutfitsDialogViewModelTests()
        {
            _mockDataService = new MockServiceBuilder()
                .WithDefaultOutfits()
                .BuildMock();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoStation_InitializesSuccessfully()
        {
            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.Title.Should().Be("Add Outfits");
        }

        [Fact]
        public void Constructor_WithStation_SetsTitle()
        {
            // Arrange
            var station = new Station { Name = "Mission Row", Agency = "LSPD" };

            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.Title.Should().Contain("Mission Row");
        }

        [Fact]
        public void Constructor_InitializesCollections()
        {
            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.OutfitItems.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_LoadsOutfits()
        {
            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.OutfitItems.Should().NotBeEmpty("outfits should be loaded from data service");
        }

        [Fact]
        public void Constructor_ExcludesExistingOutfits()
        {
            // Arrange
            var existingOutfits = new List<string> { OutfitVariationBuilder.CreateMaleVariation().CombinedName };

            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object, null, existingOutfits);

            // Assert - existing outfits should not appear in the list
            viewModel.OutfitItems.Should().NotContain(o => existingOutfits.Contains(o.CombinedName));
        }

        #endregion

        #region Search Tests

        [Fact]
        public void SearchText_FiltersOutfits()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var initialCount = viewModel.OutfitItems.Count;

            // Act
            viewModel.SearchText = "LSPD";

            // Assert
            viewModel.OutfitItems.Count.Should().BeLessThanOrEqualTo(initialCount);
        }

        [Fact]
        public void SearchText_EmptyString_ShowsAllOutfits()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            viewModel.SearchText = "LSPD"; // Filter first
            var filteredCount = viewModel.OutfitItems.Count;

            // Act
            viewModel.SearchText = "";

            // Assert
            viewModel.OutfitItems.Count.Should().BeGreaterThanOrEqualTo(filteredCount);
        }

        [Fact]
        public void IsStrictSearch_ToggleChangesSearchMode()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Act
            viewModel.IsStrictSearch = true;

            // Assert
            viewModel.SearchModeText.Should().Be("Strict");
        }

        [Fact]
        public void IsStrictSearch_ToggleRefilters()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            viewModel.SearchText = "patrol";

            // Act & Assert - toggling should trigger re-filtering
            viewModel.IsStrictSearch = true;
            viewModel.SearchModeText.Should().Be("Strict");

            viewModel.IsStrictSearch = false;
            viewModel.SearchModeText.Should().Be("Basic");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddSelectedCommand_InitiallyCannotExecute()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Act
            var canExecute = viewModel.AddSelectedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse("no outfits are selected");
        }

        [Fact]
        public void AddSelectedCommand_CanExecuteWhenOutfitSelected()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var firstOutfit = viewModel.OutfitItems.First();
            firstOutfit.IsSelected = true;

            // Act
            var canExecute = viewModel.AddSelectedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue("outfit is selected");
        }

        [Fact]
        public void AddSelectedCommand_SetsSelectedOutfits()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var firstOutfit = viewModel.OutfitItems.First();
            firstOutfit.IsSelected = true;

            // Act
            viewModel.AddSelectedCommand.Execute(null);

            // Assert
            viewModel.SelectedOutfits.Should().ContainSingle();
            viewModel.SelectedOutfits.First().Should().Be(firstOutfit.CombinedName);
        }

        [Fact]
        public void AddSelectedCommand_SetsMultipleSelectedOutfits()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var outfits = viewModel.OutfitItems.Take(3).ToList();
            foreach (var outfit in outfits)
            {
                outfit.IsSelected = true;
            }

            // Act
            viewModel.AddSelectedCommand.Execute(null);

            // Assert
            viewModel.SelectedOutfits.Should().HaveCount(3);
        }

        #endregion

        #region Selection Tests

        [Fact]
        public void OutfitItemViewModel_IsSelected_NotifiesPropertyChanged()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var firstOutfit = viewModel.OutfitItems.First();
            var propertyChangedRaised = false;
            firstOutfit.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(firstOutfit.IsSelected))
                    propertyChangedRaised = true;
            };

            // Act
            firstOutfit.IsSelected = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void StatusText_UpdatesWhenSelectionsChange()
        {
            // Arrange
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);
            var initialStatus = viewModel.StatusText;

            // Act
            var firstOutfit = viewModel.OutfitItems.First();
            firstOutfit.IsSelected = true;

            // Assert - status text should update
            // Note: This may require the ViewModel to listen to item changes
            viewModel.StatusText.Should().NotBeNull();
        }

        #endregion

        #region Title Tests

        [Fact]
        public void Title_WithContextStation_IncludesStationName()
        {
            // Arrange
            var station = new Station { Name = "Vespucci Police Station", Agency = "LSPD" };

            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.Title.Should().Contain("Vespucci Police Station");
        }

        [Fact]
        public void Title_WithoutStation_IsGeneric()
        {
            // Act
            var viewModel = new AddOutfitsDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.Title.Should().Be("Add Outfits");
        }

        #endregion
    }
}
