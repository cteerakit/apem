using System.Runtime.InteropServices;

namespace Apem.Interop;

internal sealed class WindowMessageHook : IDisposable
{
    private delegate nint SubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nint dwRefData);

    private readonly nint _hwnd;
    private readonly SubclassProc _callback;
    private readonly GCHandle _gcHandle;
    private readonly Func<uint, nint, nint, nint?> _handler;

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(nint hWnd, SubclassProc pfnSubclass, nuint uIdSubclass, nint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(nint hWnd, SubclassProc pfnSubclass, nuint uIdSubclass);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    public WindowMessageHook(nint hwnd, Action<uint, nint, nint> handler)
        : this(hwnd, (msg, wParam, lParam) =>
        {
            handler(msg, wParam, lParam);
            return null;
        })
    {
    }

    public WindowMessageHook(nint hwnd, Func<uint, nint, nint, nint?> handler)
    {
        _hwnd = hwnd;
        _handler = handler;
        _callback = SubclassCallback;
        _gcHandle = GCHandle.Alloc(_callback);
        SetWindowSubclass(_hwnd, _callback, 1, 0);
    }

    private nint SubclassCallback(nint hWnd, uint uMsg, nint wParam, nint lParam, nuint uIdSubclass, nint dwRefData)
    {
        var handled = _handler(uMsg, wParam, lParam);
        if (handled is not null)
        {
            return handled.Value;
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    public void Dispose()
    {
        RemoveWindowSubclass(_hwnd, _callback, 1);
        if (_gcHandle.IsAllocated)
        {
            _gcHandle.Free();
        }
    }
}
