# FalloutTextureWatcher

Keeps an eye on your TGA source files & and converts them to DDS when they are modified.

## About

**Fallout 4 Texture Watcher** monitors your source texture directory for changes in TGA files. When a TGA file that ends in a prefix (like _d) is modified, Fallout 4 Texture Watcher will convert it to the proper DDS format and save that DDS in the Data folder respecting your directory structure.

Here's an example scenario:

- Your Source folder is set to `C:\Fallout4\Source\Textures`.
- Your Data folder is set to `C:\Program Files (x86)\Steam\steamapps\common\Fallout4\Data\Textures`.

You are working on a texture file located at `C:\Fallout4\Source\Textures\MyMod\test_d.tga`.

Every time you save your changes to `test_d.tga`, the file will be converted to DDS and automatically saved as `C:\Program Files (x86)\Steam\steamapps\common\Fallout4\Data\Textures\MyMod\test_d.dds` by Fallout 4 Texture Watcher.

**Advantages:**

- Greatly increases texture iteration/tweaking allowing you to save the TGA from Photoshop or your prefered editing tool and having the DDS instantly generated for you.

**Limitations:**

- For now it only handles `_d`, `_s` and `_n` textures.
- Fallout 4 Texture Watcher is not intended as a replacement for any other tools. For final texture authoring, it's recommended you use Elrich, included in the Fallout 4 Creation Kit tools, which can be installed via Bethesda's Launcher.

## Requirements

Fallout 4 Texture Watcher requires Windows & .NET Framework 4.5.

## Download

1.0 Release coming soon.

[You can download the source code from this link](https://github.com/yogensia/FalloutTextureWatcher/archive/master.zip).

## Changelog

- 1.0: Initial version.

## Feature Requests

If you use this tool and want features added to it let me know. While I can't promise update, feature requests are very welcome. If I see people using the tool I'll dedicate more time to improving it.

To request features you can [submit an issue on GitHub](https://github.com/yogensia/FalloutTextureWatcher/issues/new).

## Credits & Acknowledgments

Fallout 4 Texture Watcher is written in C# by Yogensia.

Fallout 4 Texture Watcher uses TexConv to convert TGA files to DDS.

## License

Fallout 4 Texture Watcher - Keeps an eye on your TGA source files and converts them to DDS when they are modified.

Copyright (c) 2018 Yogensia.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

**[Copy of the GNU General Public License](https://github.com/yogensia/FalloutTextureWatcher/blob/master/LICENSE)**.