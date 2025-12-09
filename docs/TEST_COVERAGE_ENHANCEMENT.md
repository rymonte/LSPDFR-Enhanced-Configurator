# Test Coverage Enhancement Initiative

## Overview

This document tracks the comprehensive test coverage enhancement effort for the LSPDFR Enhanced Configurator application. The goal is to achieve 80% line coverage through a phased approach focusing on unit tests, integration tests, and UI automation tests.

**Branch**: `tests/coverage-enhancements`
**Start Date**: 2025-12-09
**Target Completion**: 9 weeks
**Coverage Target**: 80% line coverage

---

## Current State (Baseline)

### Existing Test Infrastructure
- **Test Framework**: xUnit 2.9.2 with FluentAssertions 6.12.0
- **Mocking**: Moq 4.20.70
- **UI Testing**: Avalonia.Headless.XUnit available
- **CI/CD**: GitHub Actions workflow (test.yml)
- **Code Coverage**: Coverlet 6.0.2 configured

### Existing Test Coverage
**Strong Areas:**
- ✅ RanksXmlGeneratorTests - XML generation logic
- ✅ RanksParserTests - Data parsing validation
- ✅ RanksViewModelValidationTests - 89 validation tests
- ✅ Validation Rules - Comprehensive rule testing
- ✅ Integration - XML round-trip tests

**Coverage Gaps:**
- ❌ No UI automation tests
- ❌ Limited ViewModel interaction tests
- ❌ No E2E user journey tests
- ❌ No dialog interaction tests
- ❌ No backup/restore workflow tests
- ❌ No cross-tab state management tests

---

## Implementation Strategy

### Approach Decisions
1. **Priority Order**: Unit Tests → Integration → UI Automation
2. **UI Testing**: Hybrid (Avalonia.Headless + FlaUI for complex scenarios)
3. **Test Data**: Programmatic Builders (fluent API pattern)
4. **Coverage Target**: 80% line coverage

### Technology Stack
- **Unit/Integration Testing**: xUnit, Moq, FluentAssertions
- **UI Testing**: Avalonia.Headless for most scenarios
- **UI Testing (Complex)**: FlaUI for drag-drop and system dialogs
- **Code Coverage**: Coverlet with ReportGenerator
- **CI/CD**: GitHub Actions with PR coverage comments

---

## Phase 1: Unit Test Gap Filling

**Duration**: Weeks 1-3
**Test Count**: ~230 new tests
**Coverage Target**: 60-70%

### Phase 1.1: Test Data Builder Infrastructure (Week 1)

**Files to Create:**
- `LSPDFREnhancedConfigurator.Tests/Builders/RankHierarchyBuilder.cs`
- `LSPDFREnhancedConfigurator.Tests/Builders/StationAssignmentBuilder.cs`
- `LSPDFREnhancedConfigurator.Tests/Builders/VehicleBuilder.cs`
- `LSPDFREnhancedConfigurator.Tests/Builders/OutfitVariationBuilder.cs`
- `LSPDFREnhancedConfigurator.Tests/Builders/MockServiceBuilder.cs`

**Purpose**: Provide fluent API for creating test data consistently and maintainably.

**Example Usage**:
```csharp
var rank = new RankHierarchyBuilder()
    .WithName("Detective")
    .WithXP(500)
    .WithSalary(5000)
    .WithPayBands(3)  // Creates Detective I, II, III
    .Build();
```

### Phase 1.2: ViewModel Unit Tests (Weeks 1-2)

**Test Count**: ~140 tests

**Files to Create:**
- `Tests/UI/ViewModels/WelcomeWindowViewModelTests.cs` (15 tests)
- `Tests/UI/ViewModels/MainWindowViewModelTests.cs` (30 tests)
- `Tests/UI/ViewModels/RanksViewModelTests.cs` - extend existing (45 tests)
- `Tests/UI/ViewModels/StationAssignmentsViewModelTests.cs` (20 tests)
- `Tests/UI/ViewModels/VehiclesViewModelTests.cs` (25 tests)
- `Tests/UI/ViewModels/OutfitsViewModelTests.cs` (25 tests)
- `Tests/UI/ViewModels/SettingsViewModelTests.cs` (15 tests)
- `Tests/UI/ViewModels/AddVehiclesDialogViewModelTests.cs` (12 tests)
- `Tests/UI/ViewModels/AddOutfitsDialogViewModelTests.cs` (12 tests)
- `Tests/UI/ViewModels/CopyFromRankDialogViewModelTests.cs` (10 tests)
- `Tests/UI/ViewModels/RestoreBackupDialogViewModelTests.cs` (10 tests)

**Coverage Areas:**
- Property change tracking and validation
- Command execution and CanExecute logic
- Undo/Redo integration
- TreeView interactions
- Dialog workflows
- Cross-tab synchronization

### Phase 1.3: Service Unit Tests (Week 2)

**Test Count**: ~65 tests

**Files to Create:**
- `Tests/Services/DataLoadingServiceTests.cs` (20 tests)
- `Tests/Services/ValidationServiceExtendedTests.cs` (15 tests)
- `Tests/Services/ValidationDismissalServiceTests.cs` (12 tests)
- `Tests/Services/BackupPathHelperTests.cs` (10 tests)
- `Tests/Services/SelectionStateServiceTests.cs` (8 tests)

**Coverage Areas:**
- Data loading and parsing logic
- Reference linking between entities
- Validation caching and performance
- Dismissal persistence
- Backup file management

### Phase 1.4: Command Pattern Tests (Week 3)

**Test Count**: ~23 tests

**Files to Create:**
- `Tests/Commands/UndoRedoManagerTests.cs` (15 tests)
- `Tests/Commands/PropertyChangeCommandTests.cs` (8 tests)

**Coverage Areas:**
- Command execution and stacking
- Undo/Redo operations
- Stack size limits and overflow
- Composite commands
- Command descriptions

---

## Phase 2: Integration Tests

**Duration**: Weeks 4-5
**Test Count**: ~45 new tests
**Coverage Target**: 70-75%

### Files to Create:
- `Tests/Integration/IntegrationTestFixture.cs` - Shared infrastructure
- `Tests/Integration/CrossViewModelIntegrationTests.cs` (15 tests)
- `Tests/Integration/FileSystemIntegrationTests.cs` (12 tests)
- `Tests/Integration/ValidationWorkflowIntegrationTests.cs` (10 tests)
- `Tests/Integration/CommandWorkflowIntegrationTests.cs` (8 tests)

### Coverage Areas:
- Cross-ViewModel state synchronization
- File I/O operations (backup, restore, XML generation)
- End-to-end validation workflows
- Complex command workflows across tabs
- Profile switching with data reload

---

## Phase 3: UI Automation Tests

**Duration**: Weeks 6-8
**Test Count**: ~100 new tests
**Coverage Target**: 80%+

### Files to Create:
- `Tests/UI/HeadlessTestBase.cs` - Base infrastructure
- `Tests/UI/WelcomeWindowUITests.cs` (10 tests)
- `Tests/UI/MainWindowUITests.cs` (15 tests)
- `Tests/UI/RanksTabUITests.cs` (15 tests)
- `Tests/UI/StationAssignmentsTabUITests.cs` (10 tests)
- `Tests/UI/VehiclesTabUITests.cs` (12 tests)
- `Tests/UI/OutfitsTabUITests.cs` (12 tests)
- `Tests/UI/SettingsTabUITests.cs` (8 tests)
- `Tests/UI/DialogUITests.cs` (10 tests)
- `Tests/UI/UserJourneyTests.cs` (8 tests - hybrid with FlaUI)

### Coverage Areas:
- User interaction flows (clicks, text entry, navigation)
- Tab switching and state preservation
- Dialog interactions
- Validation message display
- TreeView operations
- End-to-end user journeys

### When to Use FlaUI vs Avalonia.Headless:
- **Avalonia.Headless**: Most UI tests, ViewModel-driven interactions
- **FlaUI**: Drag-and-drop, system dialogs (file picker), complex gestures

---

## Phase 4: CI/CD Enhancement

**Duration**: Week 9
**Goal**: Integrate coverage reporting and test optimization

### Files to Create/Modify:
- `.github/workflows/test.yml` - Enhanced with coverage reporting
- `Tests/TestCategories.cs` - Test categorization traits
- `Tests/xunit.runner.json` - Parallel execution config
- `coverlet.runsettings` - Coverage exclusions and settings
- `Tests/Performance/PerformanceTests.cs` - Benchmark tests

### CI/CD Enhancements:
- Code coverage with Coverlet
- Coverage report generation with ReportGenerator
- PR comments with coverage summary
- 80% coverage threshold enforcement
- Test categorization (Unit/Integration/UI/Performance)
- Parallel test execution
- Fast unit test runs for quick feedback

---

## Active Todo List

This section tracks current work items and is updated as tasks are completed.

### Current Phase: Phase 1.2 - ViewModel Unit Tests

#### ⏳ In Progress
- None currently

#### ⏸️ Pending
1. **Phase 1.2: Implement ViewModel unit tests (~140 tests)**
   - 11 ViewModel test files covering property changes, commands, validation

3. **Phase 1.3: Implement Service unit tests (~65 tests)**
   - 5 Service test files covering data loading, validation, backup management

4. **Phase 1.4: Implement Command pattern tests (~23 tests)**
   - 2 Command test files covering undo/redo manager and property changes

5. **Phase 2: Implement Integration tests (~45 tests)**
   - Integration fixture and cross-ViewModel/file system/validation tests

6. **Phase 3: Implement UI Automation tests (~100 tests)**
   - Headless test base and UI interaction tests for all windows/tabs

7. **Phase 4: Configure CI/CD with coverage reporting**
   - Enhanced GitHub Actions workflow with coverage reporting and thresholds

#### ✅ Completed
- Created feature branch `tests/coverage-enhancements`
- Documented comprehensive test plan
- Set up todo tracking system
- **Phase 1.1 COMPLETE**: Created all 5 test data builders:
  - `RankHierarchyBuilder.cs` - Fluent builder for ranks with pay bands (245 lines)
  - `StationAssignmentBuilder.cs` - Builder for stations with zones/vehicles/outfits (140 lines)
  - `VehicleBuilder.cs` - Builder for vehicles with agency associations (119 lines)
  - `OutfitVariationBuilder.cs` - Builder for outfit variations (144 lines)
  - `MockServiceBuilder.cs` - Pre-configured service mocks (236 lines)
- Refactored existing `TestHelpers.cs` to use new builders for consistency
- Removed test directory ignore from `.gitignore`
- Added docs folder exception to `.gitignore` for documentation tracking
- All 89 existing tests pass with builder integration

---

## Progress Tracking (Overview)

### Phase 1: Unit Tests
- [x] Phase 1.1: Test Data Builders (5 files) ✅ **COMPLETE**
- [ ] Phase 1.2: ViewModel Tests (11 files, ~140 tests)
- [ ] Phase 1.3: Service Tests (5 files, ~65 tests)
- [ ] Phase 1.4: Command Tests (2 files, ~23 tests)
- [ ] **Phase 1 Complete**: 60-70% coverage achieved

### Phase 2: Integration Tests
- [ ] Integration Test Fixture (1 file)
- [ ] Cross-ViewModel Tests (15 tests)
- [ ] File System Tests (12 tests)
- [ ] Validation Workflow Tests (10 tests)
- [ ] Command Workflow Tests (8 tests)
- [ ] **Phase 2 Complete**: 70-75% coverage achieved

### Phase 3: UI Automation
- [ ] UI Test Base Infrastructure (1 file)
- [ ] Welcome Window UI Tests (10 tests)
- [ ] Main Window UI Tests (15 tests)
- [ ] Tab UI Tests (57 tests across 5 files)
- [ ] Dialog UI Tests (10 tests)
- [ ] User Journey Tests (8 tests)
- [ ] **Phase 3 Complete**: 80%+ coverage achieved

### Phase 4: CI/CD
- [ ] Enhanced GitHub Actions workflow
- [ ] Coverage reporting configuration
- [ ] Test categorization
- [ ] Performance benchmarks
- [ ] **Phase 4 Complete**: CI/CD integrated

---

## Success Metrics

### Coverage Metrics
- **Phase 1**: 60-70% line coverage
- **Phase 2**: 70-75% line coverage
- **Phase 3**: 80%+ line coverage (TARGET)
- **CI/CD**: All tests passing with coverage reports on PRs

### Quality Metrics
- No false positives in test runs
- Unit tests: < 30 seconds execution
- Full test suite: < 5 minutes execution
- Clear, actionable test failure messages
- Maintainable test code (builder pattern, DRY)

### Performance Targets
- Validation of 100 ranks: < 5 seconds
- XML generation: < 2 seconds
- TreeView refresh (100 items): < 500ms
- Profile switch: < 3 seconds
- Undo/Redo operation: < 100ms

---

## Technical Patterns

### Test Data Builder Pattern

Fluent API for creating consistent test data:

```csharp
var rank = new RankHierarchyBuilder()
    .WithName("Detective")
    .WithXP(500)
    .WithSalary(5000)
    .WithStation(new StationAssignmentBuilder()
        .WithName("Mission Row")
        .WithZones("Downtown", "Little Seoul")
        .Build())
    .WithPayBands(3)
    .Build();
```

### Mocking Strategy

Use `MockServiceBuilder` for consistent service mocks:

```csharp
var mockDataService = new MockServiceBuilder()
    .WithDefaultAgencies()
    .WithDefaultStations()
    .WithDefaultVehicles()
    .BuildMock();
```

### Test Organization

Tests follow AAA pattern (Arrange, Act, Assert):

```csharp
[Fact]
[Trait("Category", "Unit")]
public void AddRank_AddsNewRankToList()
{
    // Arrange
    var viewModel = CreateRanksViewModel();
    var initialCount = viewModel.RankTreeItems.Count;

    // Act
    viewModel.AddRankCommand.Execute(null);

    // Assert
    viewModel.RankTreeItems.Should().HaveCount(initialCount + 1);
    viewModel.RankTreeItems.Last().Rank.Name.Should().Be("New Rank");
}
```

---

## Risk Mitigation

### Identified Risks

1. **Avalonia.Headless Limitations**
   - **Risk**: Some UI interactions may not be testable
   - **Mitigation**: Use FlaUI for complex scenarios, test ViewModels directly as fallback

2. **Test Data Complexity**
   - **Risk**: Large XML files and complex state graphs
   - **Mitigation**: Programmatic builders with fluent API (type-safe, maintainable)

3. **Test Execution Time**
   - **Risk**: 375+ tests could slow down CI/CD
   - **Mitigation**: Parallel execution, test categorization, fast unit test runs

4. **Flaky Tests**
   - **Risk**: Timing issues in async operations
   - **Mitigation**: Proper async/await, explicit waits, retry logic

5. **Maintenance Burden**
   - **Risk**: Large test suite requires upkeep
   - **Mitigation**: Shared utilities, builder pattern, clear conventions

---

## PR Strategy

### Option 1: Phased PRs (Recommended)
- **PR 1**: Phase 1 (Unit tests) - ~230 tests, reviewable size
- **PR 2**: Phase 2 (Integration) - ~45 tests, builds on Phase 1
- **PR 3**: Phase 3 (UI Automation) - ~100 tests, final testing layer
- **PR 4**: Phase 4 (CI/CD) - Infrastructure improvements

**Benefits**: Easier to review, incremental value delivery, faster feedback

### Option 2: Single Large PR
- All phases in one PR
- **Benefits**: Atomic change, easier to track
- **Drawbacks**: Difficult to review, all-or-nothing merge

---

## Implementation Log

This section serves as a chronological record of all work completed during the test coverage enhancement initiative.

### 2025-12-09: Project Kickoff

#### Morning: Planning and Setup
- Explored existing test infrastructure (xUnit, Moq, FluentAssertions, Avalonia.Headless)
- Analyzed current coverage: 89 existing tests (strong in validation, XML, parsing)
- Identified coverage gaps: No UI automation, limited ViewModel interactions, no E2E tests
- User clarification on priorities: Unit Tests First → Integration → UI Automation
- User selected hybrid UI approach: Avalonia.Headless + FlaUI for complex scenarios
- User confirmed programmatic test data builders and 80% coverage target

#### Afternoon: Documentation and Branch Setup
- Created comprehensive 9-week implementation plan (375 tests across 4 phases)
- Created feature branch `tests/coverage-enhancements`
- Set up todo tracking system with 7 phase items
- Created `docs/TEST_COVERAGE_ENHANCEMENT.md` as living documentation
- Enhanced documentation to include active todo tracking and implementation log

#### Evening: Phase 1.1 - Test Data Builder Infrastructure ✅
- Created `RankHierarchyBuilder.cs` with fluent API for rank hierarchies and pay bands
- Created `StationAssignmentBuilder.cs` for station assignments with zones/vehicles/outfits
- Created `VehicleBuilder.cs` with factory methods for common vehicle types
- Created `OutfitVariationBuilder.cs` for outfit variations and complete outfits
- Created `MockServiceBuilder.cs` for pre-configured DataLoadingService mocks
- Refactored `TestHelpers.cs` to use new builders for consistency
- Fixed `.gitignore` to track test directory and docs folder
- Verified all 89 existing tests pass with builder integration
- **Phase 1.1 Complete**: 5 builder files created (884 total lines)

#### Next Steps
- Begin Phase 1.2: ViewModel Unit Tests
- Start with `WelcomeWindowViewModelTests.cs` (15 tests)
- Continue with `MainWindowViewModelTests.cs` (30 tests)
- Target: ~140 new tests across 11 ViewModel test files

---

## References

- **Test Plan**: `.claude/plans/snazzy-hatching-puppy.md`
- **Test Requirements**: Original test plan document provided by user
- **GitHub Actions**: `.github/workflows/test.yml`
- **Coverage Config**: `coverlet.runsettings` (to be created)

---

## Notes

- All tests follow xUnit conventions
- Use FluentAssertions for readable assertions
- Mock dependencies with Moq
- Keep tests focused and independent
- Use descriptive test names that explain the scenario
- Add traits for test categorization
- Maintain builder pattern for consistency

---

_Last Updated: 2025-12-09 - Enhanced with active todo tracking and implementation log_
