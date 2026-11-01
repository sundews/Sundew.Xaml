// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Controls;

using System.Windows.Input;

/// <summary>
/// One of the things a window can be told to do, handed out as a command so that a caption drawn as content can
/// bind its buttons to it without a line of code behind.
/// </summary>
/// <remarks>
/// Nothing about it is particular to one framework: all three take <see cref="ICommand"/> on a button, so each
/// window library compiles its own copy from this one source, the way the XAML surface types are shared.
/// </remarks>
internal sealed class WindowCommand : ICommand
{
    private readonly Action execute;
    private readonly Func<bool> canExecute;

    public WindowCommand(Action execute, Func<bool> canExecute)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return this.canExecute();
    }

    public void Execute(object? parameter)
    {
        if (this.canExecute())
        {
            this.execute();
        }
    }

    /// <summary>
    /// Says that the answer to <see cref="CanExecute"/> may have changed, which is what leaves a button bound to
    /// this command enabled or not.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
