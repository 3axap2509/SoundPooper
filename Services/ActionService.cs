using SoundPooper.Infrastructure.Services;

namespace SoundPooper.Services;

public class ActionService : IActionService
{
    private Action? _actionToExecute;
    public void ExecuteCurrentAction() => _actionToExecute?.Invoke();
    public void SetActionToExecute(Action action) => _actionToExecute = action;
    public void SetActionToExecute<T>(Action<T> action, T parameter) => _actionToExecute = () => action(parameter);
}