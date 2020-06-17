The FileHashes ReadMe file


Introduction
============

FileHashes calculates file hashes (checksums) for a local file. This file hash can be used to
determine if a file has changed.

Many websites list file hashes for files available to download. The idea is to compare the file after
it has been downloaded to the has listed on the website to see if the file is the same or if it has
been corrupted.

FileHashes can calculate the MD5, SHA1, SHA256 and SHA512 hashes.


How FileHashes Works
====================

Enter a file name in the top text box either by typing or pasting the complete path, by clicking the
File Name button and navigating to the desired file, or by right clicking the file and selecting
File Hashes from the context menu. Note that the last option is only available after integrating
FileHashes with Windows File Explorer, which is explained below.

After entering a file name, select which hash method you need. You can select more than one but be
aware that each method requires it's own calculation and therefore execution time will increase.
Then click Calculate on the menu. If one or more hash methods have already been selected, the
calculation will happen automatically.

If you wish to verify a file hash, paste the hash in the text box next to the Verify button and then
click verify. A message will be shown below the text box indicating if the hashes match. Note that
case is ignored during verification, there is no need to convert upper case to lower case beforehand.
If there are multiple hash methods selected, the verify process will choose the hash based on length.


File Size Matters
=================

Calculating hashes takes time. For small files the elapsed time is imperceptible. Larger files will
take more time. If the size of the file is greater that 500 megabytes, a message box will pop up
asking for confirmation before proceeding. Note that the title bar may change to "Not Responding"
before the calculation is completed. FileHashes will not attempt to calculate the file hash for
any file over 4000 megabytes. This should allow a downloaded Windows 10 ISO to be checked. Select
only one hash method for large files.


Integrate with File Explorer
============================

If you want to integrate File Hashes with File Explorer, select Integrate with File Explorer from the
File menu. A window will pop up that allows you to either add a menu item to the context menu that
appears when you right-click on a file. The Add to Explorer option adds a registry key at
HKEY_CURRENT_USER\Software\Classes\*\shell\FileHashes. The Remove from Explorer option deletes this key.


Uninstalling
============

To uninstall use the regular Windows add/remove programs feature.  If you are not planning to reinstall
you may want to delete the files and folders in %localappdata%\T_K\FileHashes*.


Notices and License
===================

FileHashes was written in C# by Tim Kennedy. Graphics files were created by Tim Kennedy.

Xceed WPF Toolkit by Xceed was used for message boxes.


MIT License
Copyright (c) 2019 - 2020 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject
to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.