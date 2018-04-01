﻿#region License Information (GPL v3)

/*

    Fallout 4 Texture Watcher - Keeps an eye on your TGA source files
    and converts them to DDS when they are modified.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

#endregion License Information (GPL v3)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FalloutTextureWatcher
{
    public class ControlWriter : TextWriter
    {
        private readonly TextBlock TextBox;
        private readonly ScrollViewer Scroll;

        public ControlWriter(TextBlock _TextBox, ScrollViewer _Scroll)
        {
            TextBox = _TextBox;
            Scroll = _Scroll;
        }

        public override void Write(char value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextBox.Text += value;
                Scroll.ScrollToEnd();
            });
        }

        public override void Write(string value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextBox.Text += value;
                Scroll.ScrollToEnd();
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
