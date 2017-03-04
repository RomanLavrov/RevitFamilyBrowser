# RevitFamilyBrowser

Browser for .rfa Revit family files.

This project is C# .NET Revit add-in. Works with Revit 2015, 2016 and Revit 2017. 
It allows to see all types inside Revit family files (.rfa) from selected folder on disk
in dockable panel inside Revit without need to load them one by one. 
By double click, a selected family type can be loaded and an instance placed inside project. 
The `History` tab collects all family types from document to the dockable panel and allows the user to place new instances of them in the project. 
The `History` tab works with different documents simultaneously.
To start working with this add-in, copy the dll and zip file to the Revit `AddIns` folder:

    C:\Users\user\AppData\Roaming\Autodesk\Revit\Addins\2016 (or 2017 - if you use Revit 2017).
    
Alternatively, you can build the add-in from source code in VisualStudio &ndash; all necessary files will be automatically copied to the appropriate locations during the build process.

![Panel](https://github.com/RomanLavrov/RevitFamilyBrowser/blob/master/RevitFamilyBrowser/images/Panel.png)
