namespace SoundPooper.Infrastructure.Services;

public interface IActionService
{
    void ExecuteCurrentAction();
    void SetActionToExecute(Action action);
    void SetActionToExecute<T>(Action<T> action, T parameter);
}