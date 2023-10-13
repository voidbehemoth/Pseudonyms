using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Game.Interface;
using UnityEngine.UI;

namespace Pseudonyms.extensions
{
    public static class PickNamesPanelExtensions
    {
        private static readonly ConditionalWeakTable<PickNamesPanel, Button> buttons = new ConditionalWeakTable<PickNamesPanel, Button>();

        public static Button GetRerollButton(this PickNamesPanel pickNamesPanel)
        {
            return buttons.GetOrCreateValue(pickNamesPanel);
        }
    }
}
