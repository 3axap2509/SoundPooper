using NAudio.Wave;

namespace SoundPooper.Infrastructure.Extensions;

public class SampleProviderWithCallback : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly Action<SampleProviderWithCallback>? _onFinishedCallback;
    private bool _isFinished;

    public SampleProviderWithCallback(ISampleProvider source, Action<SampleProviderWithCallback> onFinishedCallback)
    {
        _source = source;
        _onFinishedCallback = onFinishedCallback;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        if (_isFinished) return 0;

        var samples = _source.Read(buffer, offset, count);
        if (samples != 0) return samples;

        _isFinished = true;
        _onFinishedCallback?.Invoke(this);
        return samples;
    }
}