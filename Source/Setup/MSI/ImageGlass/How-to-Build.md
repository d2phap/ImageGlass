## Configurations to build new MSI installer


### FOR x64 (default):
- Change the `Solution Configuration` dropdown to `Release`
- `ImageGlass Property Pages` dialogue: 
  + Update the `Output file name` textbox to the ImageGlass version
- `Deployment Project Properties` panel:
  + `TargetPlatform`: change to  `x64`
  + `Version`: change to ImageGlass version, only support `##.##.####` format
- `Properties` panel of `Application Folder`:
  + `DefaultLocation`: change to `[ProgramFiles64Folder]\[ProductName]`


### FOR x86:
- Change the `Solution Configuration` dropdown to `Release_x86`
- `ImageGlass Property Pages` dialogue: 
  + Update the `Output file name` textbox to the ImageGlass version
- `Deployment Project Properties` panel:
  + `TargetPlatform`: change to  `x86`
  + `Version`: change to ImageGlass version, only support `##.##.####` format
- `Properties` panel of `Application Folder`:
  + `DefaultLocation`: change to `[ProgramFilesFolder]\[ProductName]`

