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
using System.Threading.Tasks;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// Asks the viewer to play a transition while it switches to the next photo.
/// </summary>
public sealed class TransitionRequest(TransitionEffect effect, int durationMs, int direction)
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);


    /// <summary>
    /// Gets the effect to play.
    /// </summary>
    public TransitionEffect Effect { get; } = effect;


    /// <summary>
    /// Gets the effect duration in milliseconds.
    /// </summary>
    public int DurationMs { get; } = durationMs;


    /// <summary>
    /// Gets the navigation direction: <c>1</c> forward, <c>-1</c> backward.
    /// </summary>
    public int Direction { get; } = direction;


    /// <summary>
    /// Completes when the transition finished, was skipped, or was superseded by another photo.
    /// </summary>
    public Task Completion => _completion.Task;


    /// <summary>
    /// Marks the request as finished; safe to call more than once.
    /// </summary>
    internal void Complete() => _completion.TrySetResult();

}
