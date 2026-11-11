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
using System;
using System.Threading.Tasks;

namespace ImageGlass.Common.Commands;

public static class PhCommands
{
    public static IPhCommand Create(Action? execute)
    {
        return new SyncCommand(WrapAction(execute), CanExecuteTrue);
    }

    public static IPhCommand Create(Action<object?>? execute)
    {
        return new SyncCommand(execute ?? DefaultExecute, CanExecuteTrue);
    }

    public static IPhCommand Create(Action<string?>? execute)
    {
        return new SyncCommand(execute ?? DefaultExecute, CanExecuteTrue);
    }

    public static IPhCommand Create(Action? execute, Func<bool>? canExecute)
    {
        return new SyncCommand(WrapAction(execute), WrapAction(canExecute));
    }

    public static IPhCommand Create(Action<object?>? execute, Func<object?, bool>? canExecute)
    {
        return new SyncCommand(execute ?? DefaultExecute, canExecute ?? CanExecuteTrue);
    }

    public static IPhCommand Create(Func<Task>? execute)
    {
        return new AsyncCommand(WrapAction(execute), CanExecuteTrue);
    }

    public static IPhCommand Create(Func<object?, Task>? execute)
    {
        return new AsyncCommand(execute ?? DefaultExecuteAsync, CanExecuteTrue);
    }

    public static IPhCommand Create(Func<string?, Task>? execute)
    {
        return new AsyncCommand(execute ?? DefaultExecuteAsync, CanExecuteTrue);
    }

    public static IPhCommand Create(Func<Task>? execute, Func<bool>? canExecute)
    {
        return new AsyncCommand(WrapAction(execute), WrapAction(canExecute));
    }

    public static IPhCommand Create(Func<object?, Task>? execute, Func<object?, bool>? canExecute)
    {
        return new AsyncCommand(execute ?? DefaultExecuteAsync, canExecute ?? CanExecuteTrue);
    }



    private static void DefaultExecute(object? _) { }

    private static Task DefaultExecuteAsync(object? _) => Task.CompletedTask;

    private static bool CanExecuteTrue(object? _) => true;

    private static Func<object?, Task> WrapAction(Func<Task>? action)
    {
        if (action is null)
            return DefaultExecuteAsync;

        return _ => action();
    }

    private static Action<object?> WrapAction(Action? action)
    {
        if (action is null)
            return DefaultExecute;

        return _ => action();
    }

    private static Func<object?, bool> WrapAction(Func<bool>? action)
    {
        if (action is null)
            return CanExecuteTrue;

        return _ => action();
    }
}
