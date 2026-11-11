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
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ImageGlass.Common.Commands;

public sealed partial class AsyncCommand : IPhCommand
{
    private readonly Func<object?, Task> _executeFn;
    private readonly Func<object?, bool> _canExecuteFn;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;
    public bool IsAsync => true;


    public AsyncCommand(Func<object?, Task> execute, Func<object?, bool> canExecute)
    {
        _executeFn = execute;
        _canExecuteFn = canExecute;
    }

    public AsyncCommand(Func<string?, Task> execute, Func<object?, bool> canExecute)
    {
        _executeFn = obj => execute(obj?.ToString());
        _canExecuteFn = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && _canExecuteFn.Invoke(parameter);
    }

    public void Execute(object? parameter)
    {
        _ = ExecuteAsync(parameter);
    }

    public async Task ExecuteAsync(object? parameter)
    {
        if (_isExecuting) return;

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _executeFn.Invoke(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        if (CanExecuteChanged is not null)
        {
            Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }
    }
}

