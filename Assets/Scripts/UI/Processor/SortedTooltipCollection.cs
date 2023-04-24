using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace ButterBoard.UI.Processor
{
    public class SortedTooltipCollection : ICollection<Tooltip>
    {
        /// <summary>
        /// Maps line index to the tooltips found on that line.
        /// </summary>
        private readonly Dictionary<int, List<Tooltip>> _tooltips = new Dictionary<int, List<Tooltip>>();

        /// <summary>
        /// Clears all records of <see cref="Tooltip">Tooltips</see>.
        /// </summary>
        public void ClearTooltips()
        {
            _tooltips.Clear();
            Count = 0;
        }

        /// <summary>
        /// Records a collection of <see cref="Tooltip">Tooltips</see>.
        /// </summary>
        /// <param name="tooltipAreas">The tooltips to record.</param>
        public void AddTooltips(IEnumerable<Tooltip> tooltipAreas)
        {
            // enumerate to array to avoid doing so twice
            Tooltip[] enumerable = tooltipAreas as Tooltip[] ?? tooltipAreas.ToArray();
            foreach (Tooltip tooltipArea in enumerable)
            {
                AddTooltip(tooltipArea);
            }
            Count += enumerable.Length;
        }

        /// <summary>
        /// Records a <see cref="Tooltip"/>.
        /// </summary>
        /// <param name="tooltip">The tooltip to record.</param>
        public void AddTooltip(Tooltip tooltip)
        {
            if (!_tooltips.ContainsKey(tooltip.Line))
                _tooltips.Add(tooltip.Line, new List<Tooltip>());

            _tooltips[tooltip.Line].Add(tooltip);
            Count++;
        }

        /// <summary>
        /// Gets a collection of all <see cref="Tooltip">Tooltips</see> that fall around the specified location.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public IEnumerable<Tooltip> FindTooltipsUnderCursor(int line, int column)
        {
            // no tooltips on line
            if (!_tooltips.ContainsKey(line))
                return Array.Empty<Tooltip>();

            List<Tooltip> result = new List<Tooltip>();

            foreach (Tooltip tooltipArea in _tooltips[line])
            {
                int endColumn = tooltipArea.StartColumn + tooltipArea.Length;
                if (tooltipArea.StartColumn <= column && column <= endColumn)
                    result.Add(tooltipArea);
            }

            return result;
        }

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Tooltip item)
        {
            AddTooltip(item);
        }

        public void Clear()
        {
            ClearTooltips();
        }

        public bool Contains(Tooltip item)
        {
            if (!_tooltips.ContainsKey(item.Line))
                return false;

            List<Tooltip> lineTooltips = _tooltips[item.Line];

            return lineTooltips.Contains(item);
        }

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

        public bool Remove(Tooltip item)
        {
            if (!_tooltips.ContainsKey(item.Line))
                return false;

            List<Tooltip> lineTooltips = _tooltips[item.Line];

            return lineTooltips.Remove(item);
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;
    }
}