# Building libwebp as a windows native library (.dll)

1. Get the latest [source code release from libwebp repo](https://github.com/webmproject/libwebp/releases).
2. Extract the archive to a new folder.
3. Launch `Command Prompt` with `Native Tools Command Prompt` which is come from C++ BuildTools (Get it from ["Tools for Visual Studio" section here](https://visualstudio.microsoft.com/downloads/)).
4. Change the `Current Directory` to the directory, which you created at Step 2, using this command:
```batch
cd /d "F:\Path\To\Directory"
```
5. Start compiling the source with nmake:
```batch
nmake /f Makefile.vc CFG=release-dynamic RTLIBCFG=dynamic OBJDIR=output
```

## Notes
* Depend on which `Native Tools Command Prompt` (x86 or x64), the compiled binaries will match the architecture with the `Command Prompt`'s environment.
* You **may** need to manually add some directory to `PATH` environment so that the build tool can find the external tools.
