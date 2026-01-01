namespace SoundPooper.Infrastructure.Services;

public interface IKeyboardService
{
    public void InitializeHooks();
    public void RemoveHooks();
}