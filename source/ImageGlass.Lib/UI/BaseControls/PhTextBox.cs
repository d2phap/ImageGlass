/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageGlass.UI;

public partial class PhTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);


    #region Private Regex
    private static Regex Regex_IntValueOnly => new Regex(
            pattern: $"^[{Const.SIGN_POSITIVE}{Const.SIGN_NEGATIVE}]?[0-9]+$",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex Regex_FloatValueOnly => new Regex(
            pattern: $"^([0-9]+([{Const.DECIMAL_SEPARATOR}][0-9]*)?|[{Const.DECIMAL_SEPARATOR}][0-9]+)$",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex Regex_UnsignedFloatValueOnly => new Regex(
            pattern: $"^[{Const.SIGN_POSITIVE}]?([0-9]+([{Const.DECIMAL_SEPARATOR}][0-9]*)?|[{Const.DECIMAL_SEPARATOR}][0-9]+)$",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase);


    [GeneratedRegex("^[0-9]+$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Regex_UnsignedIntValueOnly();


    // a single extension: optional leading dot + alphanumerics (e.g. ".psd" or "psd")
    [GeneratedRegex(@"^\s*\.?[A-Za-z0-9]+\s*$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Regex_FileExtensionValueOnly();


    // ".*" (all extensions), or one/more dot + alphanumeric extensions separated by ";" (e.g. ".jpg;.png")
    [GeneratedRegex(@"^\s*\.(\*|[A-Za-z0-9]+)(\s*;\s*\.(\*|[A-Za-z0-9]+))*\s*$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Regex_FileExtensionsValueOnly();


    #endregion // Private Regex



    #region Public properties

    /// <summary>
    /// Gets, sets the rules for the input control.
    /// </summary>
    public TextBoxAcceptValue AcceptValue
    {
        get => GetValue(AcceptValueProperty);
        set => SetValue(AcceptValueProperty, value);
    }
    public static readonly StyledProperty<TextBoxAcceptValue> AcceptValueProperty =
        AvaloniaProperty.Register<PhTextBox, TextBoxAcceptValue>(nameof(AcceptValue), TextBoxAcceptValue.Any);


    /// <summary>
    /// Gets, sets the value indicates that empty value is not allowed.
    /// </summary>
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set
        {
            SetValue(IsRequiredProperty, value);
            ValidateAndShowError();
        }
    }
    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<PhTextBox, bool>(nameof(IsRequired));


    /// <summary>
    /// Gets, sets the regex pattern for the validation
    /// if <see cref="AcceptValue"/> is <see cref="TextBoxAcceptValue.RegexPattern"/>.
    /// </summary>
    public string? RegexPattern
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ValidateAndShowError();
        }
    }


    /// <summary>
    /// Gets, sets the value indicates that
    /// the textbox should be validated when pressing Enter key
    /// (and when <see cref="TextBox.AcceptsReturn"/> is <c>false</c>).
    /// </summary>
    public bool ValidateByPressingEnter { get; set; } = true;


    #endregion // Public properties



    #region Override methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TextChanged += PhTextBox_TextChanged;
        Core.LanguageChanged += Core_LanguageChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        TextChanged -= PhTextBox_TextChanged;
        Core.LanguageChanged -= Core_LanguageChanged;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (AcceptsReturn) return;

        // submit the textbox
        if (ValidateByPressingEnter && e.Key == Key.Enter)
        {
            var isValid = ValidateAndShowError();
            if (!isValid) _ = AnimateValidationErrorAsync();
        }
    }

    #endregion // Override methods



    #region Control Methods

    private void PhTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateAndShowError();
    }


    private void Core_LanguageChanged(object? sender, EventArgs e)
    {
        ValidateAndShowError();
    }


    /// <summary>
    /// Validates the text input.
    /// </summary>
    protected (bool IsValid, ValidationException? Error) Validate()
    {
        ValidationException? error = null;
        var isValid = true;
        var textValue = Text ?? string.Empty;
        var isEmpty = string.IsNullOrEmpty(textValue);

        if (isEmpty)
        {
            isValid = !IsRequired;
            if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_Required]);
        }
        else
        {
            // validate according to the accept value
            switch (AcceptValue)
            {
                // use custom regex
                case TextBoxAcceptValue.RegexPattern:
                    if (!string.IsNullOrEmpty(RegexPattern))
                    {
                        isValid = Regex.IsMatch(textValue, RegexPattern);
                        if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_RegexPattern]);
                    }
                    break;

                // Int value only
                case TextBoxAcceptValue.IntValueOnly:
                    isValid = Regex_IntValueOnly.IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_IntValueOnly]);
                    break;

                // Unsigned Int value only
                case TextBoxAcceptValue.UnsignedIntValueOnly:
                    isValid = Regex_UnsignedIntValueOnly().IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_UnsignedIntValueOnly]);
                    break;

                // Float value only
                case TextBoxAcceptValue.FloatValueOnly:
                    isValid = Regex_FloatValueOnly.IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_FloatValueOnly]);
                    break;

                // Unsigned Float value only
                case TextBoxAcceptValue.UnsignedFloatValueOnly:
                    isValid = Regex_UnsignedFloatValueOnly.IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_UnsignedFloatValueOnly]);
                    break;

                // a single file extension (e.g. ".psd")
                case TextBoxAcceptValue.FileExtensionValueOnly:
                    isValid = Regex_FileExtensionValueOnly().IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_FileExtensionValueOnly]);
                    break;

                // one or more file extensions separated by ";" (e.g. ".jpg;.png" or ".*")
                case TextBoxAcceptValue.FileExtensionsValueOnly:
                    isValid = Regex_FileExtensionsValueOnly().IsMatch(textValue);
                    if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_FileExtensionsValueOnly]);
                    break;

                // Valid filename only
                case TextBoxAcceptValue.FileNameValueOnly:
                    var badChars = Path.GetInvalidFileNameChars();

                    foreach (var c in badChars.AsSpan())
                    {
                        if (textValue.Contains(c))
                        {
                            isValid = false;
                            if (!isValid) error = new ValidationException(Core.Lang[LangId._Validation_FileNameValueOnly]);
                            break;
                        }
                    }
                    break;

                // Valid file path only
                case TextBoxAcceptValue.FilePathValueOnly:
                    foreach (var c in Path.GetInvalidPathChars().AsSpan())
                    {
                        if (textValue.Contains(c))
                        {
                            isValid = false;
                            error = new ValidationException(Core.Lang[LangId._Validation_FilePathValueOnly]);
                            break;
                        }
                    }
                    break;

                // Any value
                default:
                    break;
            }
        }

        return (IsValid: isValid, Error: error);
    }


    /// <summary>
    /// Validates the text input and shows error.
    /// </summary>
    public bool ValidateAndShowError()
    {
        DataValidationErrors.ClearErrors(this);

        var result = Validate();
        if (!result.IsValid)
        {
            DataValidationErrors.SetError(this, result.Error);
        }

        return result.IsValid;
    }


    /// <summary>
    /// Animates the validation error.
    /// </summary>
    public async Task AnimateValidationErrorAsync()
    {
        var distance = 5;
        var duration = 30;

        await AnimateMarginAsync(this,
                new Thickness(Margin.Left + distance, Margin.Top, Margin.Right, Margin.Bottom),
                duration);
        await AnimateMarginAsync(this,
                new Thickness(Margin.Left - distance, Margin.Top, Margin.Right, Margin.Bottom),
                duration);
        await AnimateMarginAsync(this, Margin, duration);
    }


    /// <summary>
    /// Animates control margin.
    /// </summary>
    private static async Task AnimateMarginAsync(Control control, Thickness toMargin, int durationMs)
    {
        var fromMargin = control.Margin;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(durationMs),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(Layoutable.MarginProperty, fromMargin) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(Layoutable.MarginProperty, toMargin) }
                }
            }
        };

        await animation.RunAsync(control);
    }


    #endregion // Control Methods


}



public enum TextBoxAcceptValue
{
    Any,
    RegexPattern,
    IntValueOnly,
    UnsignedIntValueOnly,
    FloatValueOnly,
    UnsignedFloatValueOnly,
    FileNameValueOnly,
    FilePathValueOnly,
    FileExtensionValueOnly,
    FileExtensionsValueOnly,
}
