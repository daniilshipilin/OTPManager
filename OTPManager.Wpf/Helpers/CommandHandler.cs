namespace OTPManager.Wpf.Helpers;

using System;
using System.Windows.Input;

/// <summary>
/// Initializes a new instance of the <see cref="CommandHandler"/> class.
/// </summary>
public class CommandHandler(Action action, Func<bool> canExecute) : ICommand
{
    private readonly Action action = action;
    private readonly Func<bool> canExecute = canExecute;

    /// <summary>
    /// Wires CanExecuteChanged event.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Forcess checking if execute is allowed.
    /// </summary>
    public bool CanExecute(object? parameter)
        => this.canExecute.Invoke();

    public void Execute(object? parameter)
        => this.action();
}
