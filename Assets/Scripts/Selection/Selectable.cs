#nullable enable

using System;
using UnityEngine;

namespace Selection
{
    /// <summary>
    /// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
    /// </summary>
    public class Selectable : MonoBehaviour
    {
        private bool isHighlighted;
        private bool isSelected;

        public event Action<bool>? HighlightChanged;
        public event Action<bool>? SelectChanged;

        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted == value)
                {
                    return;
                }

                isHighlighted = value;
                HighlightChanged?.Invoke(value);
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value)
                {
                    return;
                }

                isSelected = value;
                SelectChanged?.Invoke(value);
            }
        }

        public void RerunHighlightEvent()
        {
            HighlightChanged?.Invoke(isHighlighted);
        }
    }
}