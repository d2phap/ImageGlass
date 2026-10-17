# Contributing to ImageGlass

## Introduction

First, thank you for considering contributing to ImageGlass! It's people like you that make the open source community such a great community! 😊

ImageGlass 10 is a full rewrite in C# / .NET 11 with Avalonia and SkiaSharp, running on **Windows, macOS, and Linux**. That means there is more to help with than ever: new platforms to test, new formats to verify, and a lot of UI to polish.

Any type of contribution is welcome, not only code:

- **QA**: file bug reports. The more detail the better (OS and version, ImageGlass version, the image file that triggers it, screenshots or a screen recording).
- **Code**: take a look at the [open issues](https://github.com/d2phap/ImageGlass/issues). Even if you can't write code, commenting on an issue and confirming that it affects you helps with triage.
- **Translation**: ImageGlass should be usable by anyone, even if they don't speak English. See [Translations](#translations) below.
- **Docs**: improvements to the [documentation](https://imageglass.org/docs), this file, or the in-app text are always welcome.
- **Community**: presenting the project at meetups, writing blog posts, answering questions on [Discord](https://discord.gg/tWjbynH2X8) or in [Discussions](https://github.com/d2phap/ImageGlass/discussions).
- **Money**: ImageGlass is built and maintained by one person. You can support it through [GitHub Sponsors](https://github.com/sponsors/d2phap), [PayPal](https://www.paypal.me/d2phap), [donate](https://imageglass.org/donate), or by buying [ImageGlass Pro](https://imageglass.org/pricing).


## Repository layout

| Path | What it is |
| --- | --- |
| [source/](source/) | ImageGlass 10 (.NET 11, Avalonia, SkiaSharp). This is where active development happens. |
| [v9/](v9/) | Legacy ImageGlass 9 (WinForms). Archived; only critical fixes are considered. |

Branches:

- `develop`: latest commits. **All pull requests must target `develop`.**
- `prod`: the final stable release.


## Building and running

**Version 10** (in [source/](source/)):

- Visual Studio 2026 for the Windows build, or VS Code for the macOS and Linux builds
- .NET 11 SDK
- Open `source/ImageGlass.slnx`, or use the CLI:

```powershell
# build the Windows app (x64 is required; the default AnyCPU fails on native deps)
dotnet build ImageGlass.Win32 -p:Platform=x64

# run it with an image
dotnet run --project ImageGlass.Win32 -p:Platform=x64 -- "C:\path\to\photo.jpg"
```

Use `ImageGlass.Mac` or `ImageGlass.Linux` instead of `ImageGlass.Win32` on those platforms.

> If the app fails to start with a theme error, copy `source/__assets/__app/_themes` next to the built executable. A plain `dotnet build` output does not include the theme assets that the packaged builds ship with.

**Version 9** (in [v9/](v9/)): Visual Studio 2026 on Windows 11, plus VS Code for `WebUI`.


## Submitting code

Any code change should be submitted as a pull request **based on the `develop` branch**.

Before you open it:

- **Keep it focused.** One topic per pull request. Unrelated refactoring makes a change much harder to review.
- **Match the surrounding code.** The conventions for v10 (AOT and trim safety, thread safety, disposal of Skia objects, reusing the `Ph*` base controls and themed resources instead of hardcoded values, localizing user-facing strings through `LangId`) are documented in [source/CLAUDE.md](source/CLAUDE.md).
- **Localize new UI text.** Add a `LangId` key with its English default rather than a literal string.
- **Say how you verified it.** There is no automated test suite for v10, so the description should list the steps you ran, the image formats you tried, and the platforms you tested on. Screenshots or a short recording are very welcome for UI changes.
- **Mention the platforms you could not test.** That is useful information, not a problem.
- Confirm the [CLA](CLA.md) checkbox in the pull request template.

For performance or image-loading problems, the app has two opt-in tracers that produce logs worth attaching: `--ig-startup-trace` (startup timing) and `--ig-photo-trace` (photo load and render).


## Code review process

Reviews are done by one maintainer, so the bigger the pull request, the longer it will take to review and merge. Try to break down large pull requests into smaller chunks that are easier to review and merge.

It is also always helpful to have some context for your pull request. What was the purpose? Why does it matter to you?

Large or architectural changes are best discussed first in an [issue](https://github.com/d2phap/ImageGlass/issues) or a [discussion](https://github.com/d2phap/ImageGlass/discussions), before you write the code.


## Translations

Translations for both ImageGlass 10 and ImageGlass 9 are managed on [Crowdin](https://crowdin.com/project/imageglass). Join the project there, pick your language, and translate in the browser. No build tools needed, and no pull request to open.

ImageGlass 10 uses language packs (`*.iglang.json`), so you can also work on a pack locally and test it before it goes to Crowdin:

1. Open **Settings > Language** in ImageGlass 10 and export the built-in English pack.
2. Translate the values in the exported `.iglang.json` file.
3. Import it back through the same page to see it live in the app.

Published packs are listed at [imageglass.org/languages](https://imageglass.org/languages).


## Reporting bugs

Use the [issue templates](https://github.com/d2phap/ImageGlass/issues/new/choose) and include:

- ImageGlass version and edition (Classic or Pro, installer or Microsoft Store), and your OS version
- Exact steps to reproduce
- The image file that triggers it, when the problem is format-specific
- Screenshots, a recording, or the trace log described above

Please **do not** file security vulnerabilities as public issues. See [SECURITY.md](SECURITY.md).


## Questions

If you have a question, start a [discussion](https://github.com/d2phap/ImageGlass/discussions/new/choose) or ask on [Discord](https://discord.gg/tWjbynH2X8). Do a quick search first, in case someone already asked the same thing.

You can also write to phap@imageglass.org. Priority support is available with Pro Business subscriptions, see [imageglass.org/support](https://imageglass.org/support).

