#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.Views;
using Fap.Foundation;

namespace FAP.Application.ViewModels
{
    public class QueryViewModel : IQuery
    {
        IQuery brower;
        public QueryViewModel(IQuery q)
        {
            brower = q;
        }

        public bool SelectFolder(out string result)
        {
            return brower.SelectFolder(out result);
        }

        public bool SelectFile(out string result)
        {
            return brower.SelectFile(out result);
        }
    }
}
