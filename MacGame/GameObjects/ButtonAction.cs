using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame.GameObjects
{
    public class ButtonAction
    {
        public string ActionName { get; set; }
        public string Args { get; set; }

        public ButtonAction(string actionName, string args)
        {
            ActionName = actionName;
            Args = args;
        }
    }
}
