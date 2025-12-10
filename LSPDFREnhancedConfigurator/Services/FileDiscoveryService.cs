using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Discovers LSPDFR XML configuration files recursively
    /// </summary>
    public class FileDiscoveryService
    {
        private readonly string _rootPath;

        public FileDiscoveryService(string rootPath)
        {
            _rootPath = rootPath;
        }

        /// <summary>
        /// Find all Agency XML files in lspdfr/data and lspdfr/data/custom
        /// </summary>
        public List<string> FindAgencyFiles()
        {
            return FindFilesRecursive("agency*.xml");
        }

        /// <summary>
        /// Find all Station XML files in lspdfr/data and lspdfr/data/custom
        /// </summary>
        public List<string> FindStationFiles()
        {
            return FindFilesRecursive("stations*.xml");
        }

        /// <summary>
        /// Find all Outfit XML files in lspdfr/data and lspdfr/data/custom
        /// </summary>
        public List<string> FindOutfitFiles()
        {
            return FindFilesRecursive("outfits*.xml");
        }

        /// <summary>
        /// Find all duty_selection XML files
        /// </summary>
        public List<string> FindDutySelectionFiles()
        {
            return FindFilesRecursive("duty_selection*.xml");
        }

        /// <summary>
        /// Find the Ranks.xml file in lspdfr/data
        /// </summary>
        public string? FindRanksFile()
        {
            var lspdfEnhancedPath = Path.Combine(_rootPath, "lspdfr", "data", "Ranks.xml");
            if (File.Exists(lspdfEnhancedPath))
            {
                return lspdfEnhancedPath;
            }

            var standardPath = Path.Combine(_rootPath, "LSPD First Response", "Ranks.xml");
            if (File.Exists(standardPath))
            {
                return standardPath;
            }

            return null;
        }

        /// <summary>
        /// Recursively find files matching a pattern in lspdfr/data and subdirectories
        /// </summary>
        private List<string> FindFilesRecursive(string pattern)
        {
            var foundFiles = new List<string>();

            var lspdfDataPath = Path.Combine(_rootPath, "lspdfr", "data");
            if (Directory.Exists(lspdfDataPath))
            {
                try
                {
                    var files = Directory.GetFiles(lspdfDataPath, pattern, SearchOption.AllDirectories);
                    foundFiles.AddRange(files);
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    Console.WriteLine($"Error searching {lspdfDataPath}: {ex.Message}");
                }
            }

            // Also check LSPD First Response folder for compatibility
            var lspdfPath = Path.Combine(_rootPath, "LSPD First Response");
            if (Directory.Exists(lspdfPath))
            {
                try
                {
                    var files = Directory.GetFiles(lspdfPath, pattern, SearchOption.AllDirectories);
                    foundFiles.AddRange(files);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching {lspdfPath}: {ex.Message}");
                }
            }

            return foundFiles.Distinct().ToList();
        }

        /// <summary>
        /// Validate that we're in a GTA V root folder
        /// </summary>
        public bool IsValidGTAVRoot()
        {
            // Check for GTA5.exe or common GTA V files
            var gta5Exe = Path.Combine(_rootPath, "GTA5.exe");
            var lspdfFolder = Path.Combine(_rootPath, "lspdfr");
            var lspdfAltFolder = Path.Combine(_rootPath, "LSPD First Response");

            return File.Exists(gta5Exe) || Directory.Exists(lspdfFolder) || Directory.Exists(lspdfAltFolder);
        }
    }
}
