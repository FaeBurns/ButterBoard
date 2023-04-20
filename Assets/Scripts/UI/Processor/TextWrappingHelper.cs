using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    /// <summary>
    /// Forces all child <see cref="TMP_Text"/> components to disable word wrapping. TextMeshProInputField forces its text display component to overflow wrapping. This component fixes that issue along with linking up the horizontal scroll positions
    /// </summary>
    public class TextWrappingHelper : ReferenceResolvedBehaviour
    {
        [BindMultiComponent(Child = true)]
        private TMP_Text[] textComponents = null!;

        public override void Start()
        {
            base.Start();

            foreach (TMP_Text tmpText in textComponents)
            {
                tmpText.enableWordWrapping = false;
            }
        }
    }
}