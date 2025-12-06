using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for removing all ranks from the hierarchy.
    /// </summary>
    public class RemoveAllRanksCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly List<RankHierarchy> _previousRanks;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public RemoveAllRanksCommand(
            List<RankHierarchy> ranks,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Backup current ranks before clearing
            _previousRanks = _ranks.ToList();

            var count = _previousRanks.Count;
            Description = $"Remove all {count} rank{(count != 1 ? "s" : "")}";
        }

        public void Execute()
        {
            _ranks.Clear();

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var rank in _previousRanks)
            {
                _ranks.Add(rank);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
