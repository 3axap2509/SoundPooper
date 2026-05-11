namespace SoundPooper.Infrastructure.Services;

public interface ISoundService
{
    void Initialize();
    void PlaySound(string soundPath);
    void StopPlaying();
    void RepeatLastPlayedSound();
    void SetSoundVolume(float value);
}