using NAudio.Wave;

namespace SoundPooper.Infrastructure.Extensions;

public class SampleProviderWithCallback(
    ISampleProvider source,
    Action<SampleProviderWithCallback> onFinishedCallback
) : ISampleProvider
{
    private readonly Action<SampleProviderWithCallback>? _onFinishedCallback = onFinishedCallback;
    private bool _isFinished;
    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        if (_isFinished) return 0;

        var samples = source.Read(buffer, offset, count);
        if (samples != 0) return samples;

        _isFinished = true;
        _onFinishedCallback?.Invoke(this);
        return samples;
    }
}