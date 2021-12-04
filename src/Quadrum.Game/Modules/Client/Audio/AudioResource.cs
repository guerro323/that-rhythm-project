using System.Threading.Tasks;

namespace Quadrum.Game.Modules.Client.Audio;

public class AudioResource
{
    private Task<byte[]> _task;
    private byte[] _bytes;

    public readonly string Key;
    
    public byte[] Bytes
    {
        get
        {
            if (_bytes == null && _task != null)
            {
                _bytes = _task.Result;
                _task = null;
            }

            return _bytes ?? Array.Empty<byte>();
        }
        set
        {
            if (_bytes != null)
                _task = null;

            _bytes = value;
        }
    }

    public bool IsCompleted => _bytes != null;

    public AudioResource(string key, Task<byte[]> task)
    {
        _task = task;
    }

    public AudioResource(string key, byte[] bytes)
    {
        _bytes = bytes;
    }
}