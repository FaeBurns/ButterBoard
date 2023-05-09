using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.UI.Processor;

namespace ButterBoard.UI.Tooltips
{
    public class TooltipCollection : ICollection<Tooltip>
    {
        /// <summary>
        /// Maps line index to the tooltips found on that line.
        /// </summary>
        private readonly Dictionary<int, List<Tooltip>> _tooltips = new Dictionary<int, List<Tooltip>>();

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a collection of all <see cref="Tooltip">Tooltips</see> that fall around the specified location.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public IEnumerable<Tooltip> FindTooltipsAtPosition(int line, int column)
        {
            // no tooltips on line
            if (!_tooltips.ContainsKey(line))
                return Array.Empty<Tooltip>();

            List<Tooltip> result = new List<Tooltip>();

            foreach (Tooltip tooltipArea in _tooltips[line])
            {
                int endColumn = tooltipArea.startColumn + tooltipArea.length;
                if (tooltipArea.startColumn <= column && column <= endColumn)
                    result.Add(tooltipArea);
            }

            return result;
        }

        /// <summary>
        /// Clears all records of <see cref="Tooltip">Tooltips</see>.
        /// </summary>
        public void Clear()
        {
            _tooltips.Clear();
            Count = 0;
        }

        /// <summary>
        /// Records a collection of <see cref="Tooltip">Tooltips</see>.
        /// </summary>
        /// <param name="tooltipAreas">The tooltips to record.</param>
        public void AddRange(IEnumerable<Tooltip> tooltipAreas)
        {
            foreach (Tooltip tooltipArea in tooltipAreas)
            {
                Add(tooltipArea);
            }
        }

        /// <summary>
        /// Adds a <see cref="Tooltip"/>.
        /// </summary>
        /// <param name="tooltip">The tooltip to record.</param>
        public void Add(Tooltip tooltip)
        {
            if (!_tooltips.ContainsKey(tooltip.line))
                _tooltips.Add(tooltip.line, new List<Tooltip>());

            _tooltips[tooltip.line].Add(tooltip);
            Count++;
        }

        /// <inheritdoc/>
        public IEnumerator<Tooltip> GetEnumerator()
        {
            foreach (List<Tooltip> lineTooltips in _tooltips.Values)
            {
                foreach (Tooltip tooltip in lineTooltips)
                {
                    yield return tooltip;
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Contains(Tooltip item)
        {
            if (!_tooltips.ContainsKey(item.line))
                return false;

            List<Tooltip> lineTooltips = _tooltips[item.line];

            return lineTooltips.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(Tooltip[] array, int arrayIndex)
        {
            int currentIndex = arrayIndex;

            // use enumerator
            foreach (Tooltip tooltip in this)
            {
                if (currentIndex >= array.Length)
                    return;

                array[currentIndex] = tooltip;
            }
        }

        /// <inheritdoc/>
        public bool Remove(Tooltip item)
        {
            if (!_tooltips.ContainsKey(item.line))
                return false;

            List<Tooltip> lineTooltips = _tooltips[item.line];

            return lineTooltips.Remove(item);
        }
    }
}