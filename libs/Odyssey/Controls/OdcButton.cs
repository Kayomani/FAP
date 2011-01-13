using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Odyssey.Controls.Interfaces;
using System.Windows.Controls.Primitives;

namespace Odyssey.Controls
{
    /// <summary>
    /// Derived Button that implements IKeyTipControl.
    /// </summary>
    public class OdcButton:Button,IKeyTipControl
    {
        #region IKeyTipControl Members

        void IKeyTipControl.ExecuteKeyTip()
        {
            OnClick();
        }

        #endregion
    }

    /// <summary>
    /// Derived ToggleButon that implements IKeyTipControl.
    /// </summary>
    public class OdcToggleButton:ToggleButton,IKeyTipControl
    {
        #region IKeyTipControl Members

        void IKeyTipControl.ExecuteKeyTip()
        {
            OnClick();
        }

        #endregion
    }
}
