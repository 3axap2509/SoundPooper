using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundPooper.Infrastructure.Extensions;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.Services;

public class SoundService : ISoundService
{
    // private const string MicrophoneKey = "NVIDIA";

    // private const string OutputDeviceKey = "CABLE Input";
    //todo: add both options available:
    //todo: 1) in-out devices with microphone-wrapping (for example VB-Cable)
    //todo: 2) out-only device (for example SteelSeries Sonar)
    
    private const string OutputDeviceKey = "SteelSeries Sonar - Aux";
    private static readonly WaveFormat WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);


    private WasapiCapture? _capture;
    private BufferedWaveProvider? _bufferedWaveProvider;
    private MixingSampleProvider? _mainMixer;
    private MixingSampleProvider? _soundMixer;
    private VolumeSampleProvider? _soundVolumeProvider;
    private WasapiOut? _virtualOutput;

    private readonly List<AudioFileReader> _activeFileReaders = [];
    private readonly List<ISampleProvider> _activeSoundProviders = [];
    private string _lastPlayedSoundPath = string.Empty;

    public void Initialize()
    {
        //todo: microphone wrapping
        // var inputDevice = new MMDeviceEnumerator()
        //     .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
        //     .First(d => d.FriendlyName.Contains(MicrophoneKey));
        //
        // _capture = new WasapiCapture(inputDevice, true); // Exclusive mode
        // _capture.WaveFormat = waveFormat;
        //
        // _bufferedWaveProvider = new BufferedWaveProvider(_capture.WaveFormat)
        // {
        //     DiscardOnBufferOverflow = true,
        //     BufferDuration = TimeSpan.FromMilliseconds(100) // было 5 секунд!
        // };
        //
        // _capture.DataAvailable += (s, e) =>
        //     _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        // _capture.StartRecording();

        _soundMixer = new MixingSampleProvider(WaveFormat)
        {
            ReadFully = true
        };
        _soundVolumeProvider = new VolumeSampleProvider(_soundMixer)
        {
            Volume = 0.3f
        };
        _mainMixer = new MixingSampleProvider(WaveFormat)
        {
            ReadFully = true
        };

        // _mainMixer.AddMixerInput(_bufferedWaveProvider.ToSampleProvider());
        _mainMixer.AddMixerInput(_soundVolumeProvider);

        var outputDevice = new MMDeviceEnumerator()
            .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
            .First(d => d.FriendlyName.Contains(OutputDeviceKey));

        _virtualOutput = new WasapiOut(
            outputDevice,
            AudioClientShareMode.Shared,
            useEventSync: false,
            latency: 10
        );

        _virtualOutput.Init(_mainMixer);
        _virtualOutput.Play();
    }

    public void PlaySound(string soundPath)
    {
        if (string.IsNullOrEmpty(soundPath)) return;

        var reader = new AudioFileReader(soundPath);
        var resamplingProvider = new WdlResamplingSampleProvider(reader, WaveFormat.SampleRate);
        var selfRemovableProvider = new SampleProviderWithCallback(
            resamplingProvider,
            self =>
            {
                _soundMixer!.RemoveMixerInput(self);
                _activeSoundProviders.Remove(self);
                _activeFileReaders.Remove(reader);
                reader.Dispose();
            }
        );

        _soundMixer!.AddMixerInput(selfRemovableProvider);
        _activeSoundProviders.Add(selfRemovableProvider);
        _activeFileReaders.Add(reader);
        _lastPlayedSoundPath = soundPath;

        if (_virtualOutput!.PlaybackState != PlaybackState.Playing)
            _virtualOutput.Play();
    }

    public void RepeatLastPlayedSound() => PlaySound(_lastPlayedSoundPath);

    public void StopPlaying()
    {
        _activeSoundProviders.ForEach(sound => _soundMixer!.RemoveMixerInput(sound));
        _activeSoundProviders.Clear();

        _activeFileReaders.ForEach(reader => reader.Dispose());
        _activeFileReaders.Clear();
    }

    public void SetSoundVolume(float value) => _soundVolumeProvider!.Volume = Math.Clamp(value, 0f, 1f);
}