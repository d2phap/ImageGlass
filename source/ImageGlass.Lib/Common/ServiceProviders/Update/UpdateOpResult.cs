/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using System;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Outcome of a download or install step, carrying the real failure text when it fails.
/// </summary>
public sealed class UpdateOpResult
{
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The real error message, never a hand-written placeholder.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Diagnostic detail for the dialog's expander (stack trace, HRESULT, deployment text).
    /// </summary>
    public string? ErrorDetails { get; init; }

    /// <summary>
    /// True when the step did nothing because it was not applicable, not because it failed.
    /// </summary>
    public bool IsSkipped { get; init; }

    public static UpdateOpResult Ok() => new() { IsSuccess = true };

    public static UpdateOpResult Skip() => new() { IsSkipped = true };

    public static UpdateOpResult Fail(string message, string? details = null)
        => new() { ErrorMessage = message, ErrorDetails = details };

    public static UpdateOpResult Fail(Exception ex)
        => new() { ErrorMessage = ex.Message, ErrorDetails = BHelper.GetExceptionDetails(ex) };
}

