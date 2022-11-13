namespace OTPManager.Wpf.Helpers;

using System;
using System.Windows.Input;

public class CommandHandler : ICommand
{
    private readonly Action action;
    private readonly Func<bool> canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler"/> class.
    /// </summary>
    public CommandHandler(Action action, Func<bool> canExecute)
    {
        this.action = action;
        this.canExecute = canExecute;
    }

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
