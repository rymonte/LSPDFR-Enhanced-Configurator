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
    /// Tests for AddVehiclesDialogViewModel
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class AddVehiclesDialogViewModelTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;

        public AddVehiclesDialogViewModelTests()
        {
            _mockDataService = new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultVehicles()
                .BuildMock();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNoStation_InitializesSuccessfully()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.Title.Should().Be("Add Vehicles");
        }

        [Fact]
        public void Constructor_WithStation_SetsTitle()
        {
            // Arrange
            var station = new Station { Name = "Mission Row", Agency = "LSPD" };

            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.Title.Should().Contain("Mission Row");
        }

        [Fact]
        public void Constructor_InitializesCollections()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.AgencyFilters.Should().NotBeNull();
            viewModel.VehicleItems.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_LoadsVehicles()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.VehicleItems.Should().NotBeEmpty("vehicles should be loaded from data service");
        }

        [Fact]
        public void Constructor_LoadsAgencyFilters()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.AgencyFilters.Should().NotBeEmpty("agency filters should be loaded");
        }

        [Fact]
        public void Constructor_WithExistingVehicles_ExcludesThem()
        {
            // Arrange
            var existingVehicles = new List<Vehicle>
            {
                new Vehicle("police", "Police Cruiser", "LSPD")
            };

            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object, null, existingVehicles);

            // Assert
            viewModel.VehicleItems.Should().NotContain(v => v.Vehicle.Model == "police",
                "existing vehicles should be excluded");
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SearchText_DefaultsToEmpty()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.SearchText.Should().BeEmpty();
        }

        [Fact]
        public void SearchText_CanBeSet()
        {
            // Arrange
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Act
            viewModel.SearchText = "police";

            // Assert
            viewModel.SearchText.Should().Be("police");
        }

        [Fact]
        public void AgencySearchText_DefaultsToEmpty()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.AgencySearchText.Should().BeEmpty();
        }

        [Fact]
        public void AgencySearchText_CanBeSet()
        {
            // Arrange
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Act
            viewModel.AgencySearchText = "LSPD";

            // Assert
            viewModel.AgencySearchText.Should().Be("LSPD");
        }

        [Fact]
        public void StatusText_DefaultsToZeroSelected()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert - Format is "0 of N vehicles selected" where N is total available
            viewModel.StatusText.Should().MatchRegex(@"0 of \d+ vehicles selected");
        }

        [Fact]
        public void SelectedVehicles_DefaultsToEmptyList()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.SelectedVehicles.Should().NotBeNull();
            viewModel.SelectedVehicles.Should().BeEmpty();
        }

        [Fact]
        public void ShowStationAgencyFilter_IsFalseWhenNoStation()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.ShowStationAgencyFilter.Should().BeFalse();
        }

        [Fact]
        public void ShowStationAgencyFilter_IsTrueWhenStationHasAgency()
        {
            // Arrange
            var station = new Station { Name = "Mission Row", Agency = "LSPD" };

            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.ShowStationAgencyFilter.Should().BeTrue();
        }

        [Fact]
        public void StationAgencyFilterButtonText_ContainsAgencyName()
        {
            // Arrange
            var station = new Station { Name = "Mission Row", Agency = "LSPD" };

            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.StationAgencyFilterButtonText.Should().Contain("LSPD");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddSelectedCommand_IsCreated()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.AddSelectedCommand.Should().NotBeNull();
        }

        [Fact]
        public void FilterByStationAgencyCommand_IsCreated()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.FilterByStationAgencyCommand.Should().NotBeNull();
        }

        #endregion

        #region Title Tests

        [Fact]
        public void Title_WithoutStation_IsGeneric()
        {
            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object);

            // Assert
            viewModel.Title.Should().Be("Add Vehicles");
        }

        [Fact]
        public void Title_WithStation_IncludesStationName()
        {
            // Arrange
            var station = new Station { Name = "Vespucci Police Station", Agency = "LSPD" };

            // Act
            var viewModel = new AddVehiclesDialogViewModel(_mockDataService.Object, station);

            // Assert
            viewModel.Title.Should().Contain("Vespucci Police Station");
        }

        #endregion
    }
}
