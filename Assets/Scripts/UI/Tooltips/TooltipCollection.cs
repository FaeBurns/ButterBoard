using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ButterBoard.UI.Processor;

namespace ButterBoard.UI.Tooltips
{
    public class TooltipCollection : ICollection<TextPositionedTooltip>
    {
        /// <summary>
        /// Maps line index to the tooltips found on that line.
        /// </summary>
        private readonly Dictionary<int, List<TextPositionedTooltip>> _tooltips = new Dictionary<int, List<TextPositionedTooltip>>();

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

            foreach (TextPositionedTooltip tooltipArea in _tooltips[line])
            {
                int endColumn = tooltipArea.StartColumn + tooltipArea.Length;
                if (tooltipArea.StartColumn <= column && column <= endColumn)
                    result.Add(tooltipArea.Tooltip);
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
        public void AddRange(IEnumerable<TextPositionedTooltip> tooltipAreas)
        {
            foreach (TextPositionedTooltip tooltipArea in tooltipAreas)
            {
                Add(tooltipArea);
            }
        }

        /// <summary>
        /// Adds a <see cref="Tooltip"/>.
        /// </summary>
        /// <param name="tooltip">The tooltip to record.</param>
        public void Add(TextPositionedTooltip tooltip)
        {
            if (!_tooltips.ContainsKey(tooltip.Line))
                _tooltips.Add(tooltip.Line, new List<TextPositionedTooltip>());

            _tooltips[tooltip.Line].Add(tooltip);
            Count++;
        }

        /// <inheritdoc/>
        public IEnumerator<TextPositionedTooltip> GetEnumerator()
        {
            foreach (List<TextPositionedTooltip> lineTooltips in _tooltips.Values)
            {
                foreach (TextPositionedTooltip tooltip in lineTooltips)
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
        public bool Contains(TextPositionedTooltip item)
        {
            if (!_tooltips.ContainsKey(item.Line))
                return false;

            List<TextPositionedTooltip> lineTooltips = _tooltips[item.Line];

            return lineTooltips.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(TextPositionedTooltip[] array, int arrayIndex)
        {
            int currentIndex = arrayIndex;

            // use enumerator
            foreach (TextPositionedTooltip tooltip in this)
            {
                if (currentIndex >= array.Length)
                    return;

                array[currentIndex] = tooltip;
            }
        }

        /// <inheritdoc/>
        public bool Remove(TextPositionedTooltip item)
        {
            if (!_tooltips.ContainsKey(item.Line))
                return false;

            List<TextPositionedTooltip> lineTooltips = _tooltips[item.Line];

            return lineTooltips.Remove(item);
        }
    }
}