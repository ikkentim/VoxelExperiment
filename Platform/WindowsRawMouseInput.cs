using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyGame.Platform;

public class WindowsRawMouseInput : IRawMouseInput
{
    private readonly Game _game;
    private WindowsInputWindow? _nativeWindow;
    private bool _isPaused;

    public WindowsRawMouseInput(Game game)
    {
        _game = game;
    }

    public bool IsCapturing => _nativeWindow != null && !_isPaused && _nativeWindow.IsCapturing;

    public void Start()
    {
        var control = System.Windows.Forms.Control.FromHandle(_game.Window.Handle);

        if (control != null)
        {
            _nativeWindow = new WindowsInputWindow(control);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        CenterMouse();
        GlobalGameContext.Current.Game.IsMouseVisible = false;
    }

    public void Stop()
    {
        _nativeWindow?.DestroyHandle();
        _nativeWindow = null;
        
        GlobalGameContext.Current.Game.IsMouseVisible = true;
    }

    private static void CenterMouse()
    {
        // keep mouse in bounds
        var position = Mouse.GetState().Position.ToVector2();
        var windowSize = GlobalGameContext.Current.WindowSize;
        var left = windowSize * 0.1f;
        var right = windowSize * 0.9f;

        if (position.X < left.X || position.Y < left.Y || position.X > right.X || position.Y > right.Y)
        {
            var center = GlobalGameContext.Current.WindowSize / 2;
            Mouse.SetPosition((int)center.X, (int)center.Y);
        }
    }

    public Vector2 GetInput()
    {
        if (_nativeWindow == null || _isPaused)
        {
            return Vector2.Zero;
        }

        _nativeWindow.GetRawDelta(out var x, out var y);

        CenterMouse();

        return new Vector2(x, y);
    }

    public void Pause()
    {
        _isPaused = true;
        GlobalGameContext.Current.Game.IsMouseVisible = true;
    }

    public void Resume()
    {
        _isPaused = false;
        GlobalGameContext.Current.Game.IsMouseVisible = false;
        CenterMouse();

        // reset delta accumulated during pause
        _nativeWindow?.GetRawDelta(out _, out _);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("IDE", "IDE0079")]
    private class WindowsInputWindow : NativeWindow
    {
        private const int RIDEV_REMOVE = 0x00000001;
        private const int RIDEV_INPUTSINK = 0x00000100;

        private const uint RID_INPUT = 0x10000003;
        private const uint RID_HEADER = 0x10000005;

        private const uint RIM_TYPEMOUSE = 0x00;
        private const uint RIM_TYPEKEYBOARD = 0x01;
        private const uint RIM_TYPEHID = 0x02;

        private const uint MOUSE_MOVE_RELATIVE = 0x00;
        private const uint MOUSE_MOVE_ABSOLUTE = 0x01;
        private const uint MOUSE_VIRTUAL_DESKTOP = 0x02;
        private const uint MOUSE_ATTRIBUTES_CHANGED = 0x04;

        private const int HID_USAGE_PAGE_GENERIC = 0x01;
        private const int HID_USAGE_GENERIC_MOUSE = 0x02;

        private const int WM_INPUT = 0x00FF;

        private int _rawXValue;
        private int _rawYValue;

        public WindowsInputWindow(System.Windows.Forms.Control parent)
        {
            parent.HandleCreated += OnHandleCreated;
            parent.HandleDestroyed += OnHandleDestroyed;

            if (parent.IsHandleCreated)
            {
                AssignHandle(parent.Handle);
            }
        }

        public bool IsCapturing { get; private set; }
        
        public void GetRawDelta(out int x, out int y)
        {
            x = _rawXValue;
            y = _rawYValue;
            _rawXValue = 0;
            _rawYValue = 0;
        }

        private void OnHandleCreated(object? sender, EventArgs e)
        {
            if (sender is Form form)
                AssignHandle(form.Handle);
        }

        private void OnHandleDestroyed(object? sender, EventArgs e)
        {
            ReleaseHandle();
        }

        protected override void OnHandleChange()
        {
            base.OnHandleChange();
            if (Handle != IntPtr.Zero)
                RegisterMouseRawInput(Handle);
        }

        public override void ReleaseHandle()
        {
            UnRegisterMouseRawInput();
            base.ReleaseHandle();
        }

        private void RegisterMouseRawInput(IntPtr handle)
        {
            RawInputDevice rid;
            rid.UsagePage = HID_USAGE_PAGE_GENERIC;
            rid.Usage = HID_USAGE_GENERIC_MOUSE;
            rid.Flags = RIDEV_INPUTSINK;
            rid.HWndTarget = handle;
            var result = RegisterRawInputDevices(ref rid, 1, (uint)Marshal.SizeOf(typeof(RawInputDevice)));
            IsCapturing = result;
        }

        private void UnRegisterMouseRawInput()
        {
            RawInputDevice rid;
            rid.UsagePage = HID_USAGE_PAGE_GENERIC;
            rid.Usage = HID_USAGE_GENERIC_MOUSE;
            rid.Flags = RIDEV_REMOVE;
            rid.HWndTarget = IntPtr.Zero;
            RegisterRawInputDevices(ref rid, 1, (uint)Marshal.SizeOf(typeof(RawInputDevice)));
            IsCapturing = false;
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WM_INPUT)
            {
                ProcInput(ref message);
            }

            base.WndProc(ref message);
        }

        private unsafe void ProcInput(ref Message message)
        {
            uint dataSize = 0;
            _ = GetRawInputData(message.LParam, RID_INPUT, IntPtr.Zero, ref dataSize, sizeof(RawInputHeader));

            if (dataSize != sizeof(RawInput))
                return;

            RawInput ri;
            _ = GetRawInputData(message.LParam, RID_INPUT, new IntPtr(&ri), ref dataSize, sizeof(RawInputHeader));

            if (ri.header.Type == RIM_TYPEMOUSE)
            {
                var mi = &ri.data.mouse;

                if ((mi->Flags & MOUSE_MOVE_ABSOLUTE) == 0)
                {
                    unchecked
                    {
                        _rawXValue += mi->LastX;
                        _rawYValue += mi->LastY;
                    }
                }
            }
        }


        [DllImport("User32.dll")]
        private static extern int GetRawInputData(IntPtr hRawInput, uint command,
            IntPtr pData, ref uint size, int sizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(ref RawInputDevice pRawInputDevice, uint numberDevices, uint size);

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct RawMouse
        {
            public ushort Flags;
            public ushort ButtonFlags;
            public ushort ButtonData;

            public uint RawButtons;
            public int LastX;
            public int LastY;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct RawInputDevice
        {
            public ushort UsagePage;
            public ushort Usage;
            public int Flags;
            public IntPtr HWndTarget;
        }

        [StructLayout(LayoutKind.Explicit)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct RawInputData
        {
            [FieldOffset(0)] public RawMouse mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct RawInputHeader
        {
            public int Type;
            public int Size;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct RawInput
        {
            public RawInputHeader header;
            public RawInputData data;
        }
    }
}