using NAudio.CoreAudioApi;
using SoundPooper.Infrastructure.Services;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SoundPooper.Services;

public class SoundService : ISoundService
{
    private BufferedWaveProvider? _micBuffer;
    private MixingSampleProvider? _mainMixer;
    private MixingSampleProvider? _soundMixer;
    private VolumeSampleProvider? _soundVolume;

    private WasapiCapture? _micCapture;
    private WasapiOut? _virtualMicOut;
    private ISampleProvider? _micSampleProvider;
    private readonly List<ISampleProvider> _mixerInputs = new();

    private string _lastPlayedSoundPath = string.Empty;

    private Action _actionToExecute = EmptyAction;

    private static Action EmptyAction => () => { };


    public void Initialize()
    {
        var inputDevicesList = new MMDeviceEnumerator()
            .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        var outputDevicesList = new MMDeviceEnumerator()
            .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        // 1. Microphone
        var micDevice = inputDevicesList.First(d => d.FriendlyName.Contains("NVIDIA"));
        _micCapture = new WasapiCapture(micDevice) { WaveFormat = waveFormat };
        _micBuffer = new BufferedWaveProvider(_micCapture.WaveFormat);
        _micCapture.DataAvailable += (s, e) =>
            _micBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
        _micCapture.StartRecording();
        _micSampleProvider = _micBuffer.ToSampleProvider();

        // 2. Sound mixer
        _soundMixer = new MixingSampleProvider(waveFormat) { ReadFully = true };
        _soundVolume = new VolumeSampleProvider(_soundMixer) { Volume = 1f };

        // 3. Main mixer
        _mainMixer = new MixingSampleProvider(waveFormat) { ReadFully = true };
        _mainMixer.AddMixerInput(_soundVolume);
        _mainMixer.AddMixerInput(_micSampleProvider);

        // 3. To output device (VB-Cable)
        var virtualMicDevice = outputDevicesList
            .First(
                mic => mic.FriendlyName.Equals(
                    "CABLE Input (VB-Audio Virtual Cable)",
                    StringComparison.InvariantCulture
                )
            );


        // ищем виртуальный кабель
        _virtualMicOut = new WasapiOut(
            virtualMicDevice,
            AudioClientShareMode.Shared,
            true,
            20
        );
        _virtualMicOut.Init(_mainMixer);
        _virtualMicOut.Play();
    }


    public void SetPlaySoundAction(string soundPath) =>
        _actionToExecute = () =>
        {
            using var reader = new AudioFileReader(soundPath);
            var resampled = new WdlResamplingSampleProvider(reader, _soundMixer!.WaveFormat.SampleRate);
            ISampleProvider stereo = resampled.WaveFormat.Channels == 1
                ? new MonoToStereoSampleProvider(resampled)
                : resampled;
            _soundMixer.AddMixerInput(stereo);
            _mixerInputs!.Add(stereo);
            _lastPlayedSoundPath = soundPath;

            //if (_virtualMicOut!.PlaybackState == PlaybackState.Playing) return;
            _virtualMicOut.Play();
        };

    public void SetStopPlayingAction() =>
        _actionToExecute = () =>
        {
            _mixerInputs!.ForEach(input => _soundMixer!.RemoveMixerInput(input));
            _mixerInputs.Clear();
        };

    public void SetLastPlayedSoundToRepeat() =>
        SetPlaySoundAction(_lastPlayedSoundPath);

    public void SetVoidAction() => _actionToExecute = EmptyAction;

    public void SetSoundVolume(float value)
    {
        _soundVolume!.Volume = Math.Clamp(value, 0, 1);
    }


    public void ExecuteCurrentAction() => _actionToExecute?.Invoke();
}