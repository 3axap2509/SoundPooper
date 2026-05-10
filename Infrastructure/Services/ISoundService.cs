namespace SoundPooper.Infrastructure.Services;

public interface ISoundService
{
    void Initialize();
    void ExecuteCurrentAction();
    void SetPlaySoundAction(string soundPath);
    void SetStopPlayingAction();
    void SetLastPlayedSoundToRepeat();
    void SetVoidAction();
    void SetSoundVolume(float value);
}