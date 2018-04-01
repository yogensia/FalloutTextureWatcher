#region License Information (GPL v3)

/*

    Fallout 4 Texture Watcher - Keeps an eye on your TGA source files
    and converts them to DDS when they are modified.

    Copyright (c) 2018 Yogensia.

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FalloutTextureWatcher
{
    public partial class TrayMenu
    {
        private void TrayIconCmdExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TrayIconCmdAbout(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yogensia/FalloutTextureWatcher");
        }

        private void TrayIconCmdShowHide(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Title.StartsWith("Fallout 4 Texture Watcher v"))
                {
                    if (window.ShowInTaskbar)
                    {
                        window.ShowInTaskbar = false;
                        window.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        window.ShowInTaskbar = true;
                        window.Visibility = Visibility.Visible;
                        window.Activate();
                    }
                }
            }
        }
    }
}
