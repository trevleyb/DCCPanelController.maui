using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Services.ProfileService {
    /// <summary>
    /// Tools to validate (read-only) and repair (opt-in) the profile catalog,
    /// keeping the catalog entries and the on-disk JSON files in sync.
    /// </summary>
    public static class ProfileRepair {
        public sealed record CatalogValidationReport(
            IReadOnlyList<string> MissingCatalogFiles,
            IReadOnlyList<string> OrphanDiskFiles,
            int CatalogCount,
            int DiskCandidateCount) {
            public bool HasDifferences => MissingCatalogFiles.Count > 0 || OrphanDiskFiles.Count > 0;

            /// <summary>
            /// Produces a readable summary for UI/log display.
            /// </summary>
            public string SummaryString() {
                if (!HasDifferences)
                    return$"✅ Catalog is valid ({CatalogCount} entries, {DiskCandidateCount} disk files).";

                var summary = $"Catalog check summary:\n" +
                              $"- Catalog entries: {CatalogCount}\n" +
                              $"- Disk JSON candidates: {DiskCandidateCount}\n";

                if (MissingCatalogFiles.Count > 0) {
                    summary += $"\n⚠️ Missing ({MissingCatalogFiles.Count}) catalog references not found on disk:\n";
                    foreach (var f in MissingCatalogFiles.Take(10))
                        summary += $"   • {f}\n";

                    if (MissingCatalogFiles.Count > 10)
                        summary += $"   … and {MissingCatalogFiles.Count - 10} more\n";
                } else summary += "\n✅ No missing catalog references.\n";

                if (OrphanDiskFiles.Count > 0) {
                    summary += $"\n📄 Orphan JSON files on disk ({OrphanDiskFiles.Count}) not in catalog:\n";
                    foreach (var f in OrphanDiskFiles.Take(10))
                        summary += $"   • {f}\n";

                    if (OrphanDiskFiles.Count > 10)
                        summary += $"   … and {OrphanDiskFiles.Count - 10} more\n";
                } else summary += "\n✅ No orphan JSON files found.\n";

                summary += "\nYou can safely run ProfileRepair.RepairCatalogAsync() to synchronize.";

                return summary.TrimEnd();
            }
        }

        /// <summary>
        /// Scans the catalog and storage folder and returns differences WITHOUT changing anything.
        /// Missing = referenced by catalog but file is not present.
        /// Orphans = JSON-ish files on disk that are not in the catalog AND load as valid Profile.
        /// </summary>
        public static async Task<CatalogValidationReport> ValidateCatalogAsync(ProfileCatalog catalog) {
            ArgumentNullException.ThrowIfNull(catalog);

            var catalogPath = JsonRepository.GetStorageFilePath(ProfileCatalog.FileNameOnDisk);
            var storageFolder = Path.GetDirectoryName(catalogPath) ?? Environment.CurrentDirectory;

            // --- Normalize catalog filenames to always include .json for comparison ---
            var catalogFiles = catalog.Profiles
                                      .Select(p => {
                                          var file = p.FileName;
                                          if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                                              file += ".json";

                                          return file;
                                      })
                                      .Distinct(StringComparer.OrdinalIgnoreCase)
                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Candidate files on disk: *.json or legacy "DCCPanelController.<guid>"
            var diskCandidates = Directory.EnumerateFiles(storageFolder)
                                          .Select(Path.GetFileName)!
                                          .Where(f => f != null &&
                                              // include JSON or legacy "DCCPanelController." files
                                              (f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                               f.StartsWith("DCCPanelController.", StringComparison.OrdinalIgnoreCase))
                                            &&
                                              // exclude the catalog/index file itself (with or without .json)
                                              !f.Equals(ProfileCatalog.FileNameOnDisk, StringComparison.OrdinalIgnoreCase) &&
                                              !f.Equals(ProfileCatalog.FileNameOnDisk + ".json", StringComparison.OrdinalIgnoreCase)
                                          )
                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                          .ToList();

            // 1) Missing files: referenced in catalog but not on disk
            var missing = catalogFiles
                          .Where(name => !File.Exists(Path.Combine(storageFolder, name)))
                          .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                          .ToList();

            // 2) Orphan JSON files: exist on disk, not referenced in catalog, and loadable as a valid Profile
            var orphans = new List<string>();
            foreach (var file in diskCandidates) {
                if (file is null ||
                    string.Equals(file, ProfileCatalog.FileNameOnDisk + ".json", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(file, ProfileCatalog.FileNameOnDisk, StringComparison.OrdinalIgnoreCase)) continue;

                if (catalogFiles.Contains(file)) continue;

                if (await JsonTagValidator.HasTypeTagInFileAsync<Profile>(file)) {
                    var loaded = await JsonRepository.LoadAsync(file);
                    if (loaded is { }) orphans.Add(file);
                }
            }

            return new CatalogValidationReport(missing, orphans, catalog.Profiles.Count, diskCandidates.Count);
        }

        /// <summary>
        /// Applies a previously computed validation report.
        /// - Removes catalog entries that are missing on disk.
        /// - Adds loadable orphan files on disk into the catalog.
        /// Does NOT change the active/default selection beyond what ProfileCatalog already enforces.
        /// </summary>
        public static async Task<(int removed, int added)> RepairCatalogAsync(ProfileCatalog catalog, CatalogValidationReport? report) {
            ArgumentNullException.ThrowIfNull(catalog);
            if (report is null || !report.HasDifferences) return(0, 0);

            var removed = 0;
            var added = 0;

            // Remove missing catalog entries
            foreach (var file in report.MissingCatalogFiles) {
                // try both .json and non-.json variants for backwards compatibility
                var shortName = file.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? file[..^5]
                    : file;

                if (catalog.Profiles.Any(p =>
                        p.FileName.Equals(file, StringComparison.OrdinalIgnoreCase) ||
                        p.FileName.Equals(shortName, StringComparison.OrdinalIgnoreCase))) {
                    catalog.Delete(shortName);
                    removed++;
                }
            }

            // Add orphan files to catalog
            foreach (var file in report.OrphanDiskFiles) {
                var p = await JsonRepository.LoadAsync(file);
                if (p is null) continue;

                // Ensure consistent .json naming
                if (string.IsNullOrWhiteSpace(p.Filename))
                    p.Filename = file;
                else if (!p.Filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    p.Filename += ".json";

                if (string.IsNullOrWhiteSpace(p.ProfileName))
                    p.ProfileName = catalog.GetUniqueProfileName("Profile");

                await JsonRepository.SaveAsync(p);
                catalog.Upsert(p);
                added++;
            }

            return(removed, added);
        }
    }
}