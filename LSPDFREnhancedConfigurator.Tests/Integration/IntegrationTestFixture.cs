using System;
using System.IO;
using System.Xml.Linq;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.Tests.Integration
{
    /// <summary>
    /// Shared test fixture for integration tests that require a temporary GTA V directory structure
    /// </summary>
    public class IntegrationTestFixture : IDisposable
    {
        public string TempGtaDirectory { get; }
        public string TempBackupDirectory { get; }
        public string ProfileName { get; } = "TestProfile";
        public DataLoadingService DataService { get; }
        public FileDiscoveryService FileDiscoveryService { get; }

        public IntegrationTestFixture()
        {
            // Create temp directory structure
            TempGtaDirectory = Path.Combine(
                Path.GetTempPath(),
                $"LSPDFRTest_{Guid.NewGuid()}");

            TempBackupDirectory = Path.Combine(TempGtaDirectory,
                "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", ProfileName, "Backups");

            CreateDirectoryStructure();
            CreateTestDataFiles();

            // Initialize services with temp directory
            FileDiscoveryService = new FileDiscoveryService(TempGtaDirectory);
            DataService = new DataLoadingService(FileDiscoveryService);

            // Load all test data
            DataService.LoadAll();
        }

        private void CreateDirectoryStructure()
        {
            // Create base GTA directory structure - use lowercase "lspdfr" to match FileDiscoveryService
            Directory.CreateDirectory(Path.Combine(TempGtaDirectory, "lspdfr", "data"));
            Directory.CreateDirectory(Path.Combine(TempGtaDirectory, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", ProfileName));
            Directory.CreateDirectory(TempBackupDirectory);
        }

        private void CreateTestDataFiles()
        {
            // Create sample agency.xml (singular, lowercase - matches real LSPDFR file naming)
            var agenciesXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Agencies>
    <Agency>
        <Name>Los Santos Police Department</Name>
        <ShortName>LSPD</ShortName>
        <ScriptName>lspd</ScriptName>
        <Vehicles>
            <Vehicle model=""police"">2011 Ford Crown Victoria</Vehicle>
            <Vehicle model=""police2"">2012 Ford Interceptor</Vehicle>
            <Vehicle model=""police3"">Ford Explorer</Vehicle>
        </Vehicles>
    </Agency>
    <Agency>
        <Name>Los Santos Sheriff's Department</Name>
        <ShortName>LSSD</ShortName>
        <ScriptName>lssd</ScriptName>
        <Vehicles>
            <Vehicle model=""sheriff"">2013 Dodge Charger</Vehicle>
            <Vehicle model=""sheriff2"">Chevrolet Tahoe</Vehicle>
        </Vehicles>
    </Agency>
</Agencies>";

            File.WriteAllText(
                Path.Combine(TempGtaDirectory, "lspdfr", "data", "agency.xml"),
                agenciesXml);

            // Create sample stations.xml (lowercase to match stations*.xml pattern)
            var stationsXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Stations>
    <Station>
        <Name>Mission Row Police Station</Name>
        <Agency>lspd</Agency>
        <ScriptName>MissionRow</ScriptName>
        <Position>460.3052f, -990.7862f, 30.68962f</Position>
        <Heading>90.0f</Heading>
    </Station>
    <Station>
        <Name>Vespucci Police Station</Name>
        <Agency>lspd</Agency>
        <ScriptName>Vespucci</ScriptName>
        <Position>-1094.946f, -832.875f, 19.0f</Position>
        <Heading>0.0f</Heading>
    </Station>
    <Station>
        <Name>Sandy Shores Sheriff's Office</Name>
        <Agency>lssd</Agency>
        <ScriptName>SandyShores</ScriptName>
        <Position>1851.3f, 3689.2f, 34.2f</Position>
        <Heading>120.0f</Heading>
    </Station>
</Stations>";

            File.WriteAllText(
                Path.Combine(TempGtaDirectory, "lspdfr", "data", "stations.xml"),
                stationsXml);

            // Create sample ranks.xml
            var ranksXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
    <Rank name=""Officer"" requiredPoints=""0"" salary=""1000"">
        <Stations>
            <Station name=""Mission Row Police Station"" styleID=""0"">
                <Zones>
                    <Zone>Downtown</Zone>
                </Zones>
            </Station>
        </Stations>
    </Rank>
    <Rank name=""Senior Officer"" requiredPoints=""500"" salary=""2000"">
        <Stations>
            <Station name=""Mission Row Police Station"" styleID=""0"">
                <Zones>
                    <Zone>Downtown</Zone>
                    <Zone>Little Seoul</Zone>
                </Zones>
            </Station>
            <Station name=""Vespucci Police Station"" styleID=""0"">
                <Zones>
                    <Zone>Vespucci Beach</Zone>
                </Zones>
            </Station>
        </Stations>
    </Rank>
</Ranks>";

            File.WriteAllText(
                GetRanksPath(),
                ranksXml);
        }

        /// <summary>
        /// Gets the path to the Ranks.xml file for the test profile
        /// </summary>
        public string GetRanksPath()
        {
            return Path.Combine(TempGtaDirectory, "plugins", "LSPDFR", "LSPDFR Enhanced",
                "Profiles", ProfileName, "Ranks.xml");
        }

        /// <summary>
        /// Gets the backup directory path for the test profile
        /// </summary>
        public string GetBackupDirectory()
        {
            return TempBackupDirectory;
        }

        /// <summary>
        /// Creates a sample backup file with the given timestamp in the specified backup directory
        /// </summary>
        /// <param name="timestamp">Timestamp for the backup file</param>
        /// <param name="backupRootDirectory">Root backup directory (if null, uses TempBackupDirectory)</param>
        public string CreateTestBackup(DateTime timestamp, string? backupRootDirectory = null)
        {
            // Use the same format as BackupPathHelper: Ranks_YYYYMMDD-HHMM.xml
            var backupFileName = $"Ranks_{timestamp:yyyyMMdd-HHmm}.xml";

            // Determine the backup directory to use
            var targetBackupDir = backupRootDirectory != null
                ? Path.Combine(backupRootDirectory, ProfileName)
                : TempBackupDirectory;

            // Ensure the directory exists
            Directory.CreateDirectory(targetBackupDir);

            var backupPath = Path.Combine(targetBackupDir, backupFileName);

            var sampleXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Ranks>
    <Rank name=""Backup Test Rank"" requiredPoints=""0"" salary=""1000"" />
</Ranks>";

            File.WriteAllText(backupPath, sampleXml);
            File.SetLastWriteTime(backupPath, timestamp);

            return backupPath;
        }

        /// <summary>
        /// Writes custom XML content to the Ranks.xml file
        /// </summary>
        public void WriteRanksXml(string xmlContent)
        {
            File.WriteAllText(GetRanksPath(), xmlContent);
        }

        /// <summary>
        /// Reads the current Ranks.xml content
        /// </summary>
        public string ReadRanksXml()
        {
            return File.ReadAllText(GetRanksPath());
        }

        /// <summary>
        /// Parses the current Ranks.xml as an XDocument
        /// </summary>
        public XDocument ParseRanksXml()
        {
            return XDocument.Load(GetRanksPath());
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(TempGtaDirectory))
                {
                    Directory.Delete(TempGtaDirectory, recursive: true);
                }
            }
            catch
            {
                // Best effort cleanup - ignore errors
            }
        }
    }
}
