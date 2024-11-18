namespace OMA.Core;

public class OmaStatusResult<T>(T? value, OmaStatus status)
{
    private readonly T? _value = value;
    private readonly OmaStatus _status = status;

    public bool Some() => _value != null;

    public T Value()
    {
        if (_value == null)
        {
            throw new NullReferenceException();
        }

        return _value;
    }

    public OmaStatus Status() => _status;
}
