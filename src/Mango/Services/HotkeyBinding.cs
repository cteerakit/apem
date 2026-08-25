namespace Mango.Services;

public sealed class HotkeyBinding
{
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }
    public int VirtualKey { get; set; }

    public HotkeyBinding Clone() => new()
    {
        Ctrl = Ctrl,
        Alt = Alt,
        Shift = Shift,
        Win = Win,
        VirtualKey = VirtualKey,
    };

    public void CopyFrom(HotkeyBinding other)
    {
        Ctrl = other.Ctrl;
        Alt = other.Alt;
        Shift = other.Shift;
        Win = other.Win;
        VirtualKey = other.VirtualKey;
    }

    public bool EqualsBinding(HotkeyBinding? other) =>
        other is not null &&
        Ctrl == other.Ctrl &&
        Alt == other.Alt &&
        Shift == other.Shift &&
        Win == other.Win &&
        VirtualKey == other.VirtualKey;

    public static HotkeyBinding DefaultToggleOverlay() => new()
    {
        Alt = true,
        VirtualKey = 0x78, // F9
    };

    public static HotkeyBinding DefaultToggleInteractive() => new()
    {
        Alt = true,
        VirtualKey = 0xC0, // Oem3 / `
    };
}

public static class HotkeyFormatting
{
    public static string ToDisplayString(HotkeyBinding binding)
    {
        if (binding.VirtualKey == 0)
        {
            return "Not set";
        }

        var parts = new List<string>();
        if (binding.Ctrl)
        {
            parts.Add("Ctrl");
        }

        if (binding.Alt)
        {
            parts.Add("Alt");
        }

        if (binding.Shift)
        {
            parts.Add("Shift");
        }

        if (binding.Win)
        {
            parts.Add("Win");
        }

        parts.Add(VirtualKeyName(binding.VirtualKey));
        return string.Join("+", parts);
    }

    public static string VirtualKeyName(int virtualKey) => virtualKey switch
    {
        0x08 => "Backspace",
        0x09 => "Tab",
        0x0D => "Enter",
        0x1B => "Esc",
        0x20 => "Space",
        0x21 => "PageUp",
        0x22 => "PageDown",
        0x23 => "End",
        0x24 => "Home",
        0x25 => "Left",
        0x26 => "Up",
        0x27 => "Right",
        0x28 => "Down",
        0x2D => "Insert",
        0x2E => "Delete",
        0x30 => "0",
        0x31 => "1",
        0x32 => "2",
        0x33 => "3",
        0x34 => "4",
        0x35 => "5",
        0x36 => "6",
        0x37 => "7",
        0x38 => "8",
        0x39 => "9",
        >= 0x41 and <= 0x5A => ((char)virtualKey).ToString(),
        0x70 => "F1",
        0x71 => "F2",
        0x72 => "F3",
        0x73 => "F4",
        0x74 => "F5",
        0x75 => "F6",
        0x76 => "F7",
        0x77 => "F8",
        0x78 => "F9",
        0x79 => "F10",
        0x7A => "F11",
        0x7B => "F12",
        0xBA => ";",
        0xBB => "=",
        0xBC => ",",
        0xBD => "-",
        0xBE => ".",
        0xBF => "/",
        0xC0 => "`",
        0xDB => "[",
        0xDC => "\\",
        0xDD => "]",
        0xDE => "'",
        _ => $"VK_{virtualKey:X2}",
    };

    public static bool IsModifierKey(int virtualKey) =>
        virtualKey is 0x10 or 0x11 or 0x12 or 0x5B or 0x5C // Shift, Ctrl, Alt, LWin, RWin
            or 0xA0 or 0xA1 or 0xA2 or 0xA3 or 0xA4 or 0xA5; // L/R variants
}
