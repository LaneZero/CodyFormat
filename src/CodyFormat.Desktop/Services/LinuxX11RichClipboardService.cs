using System.Runtime.InteropServices;
using System.Text;

namespace CodyFormat.Desktop.Services;

/// <summary>
/// Linux/X11 rich clipboard provider used by "Copy for Word".
///
/// Avalonia's portable clipboard API remains the fallback, but office suites on Linux
/// do not all negotiate custom MIME targets in the same way. This provider owns the
/// X11 CLIPBOARD selection directly and advertises HTML, RTF and plain-text targets.
/// It works on native X11 and on Wayland desktops through XWayland when DISPLAY exists.
/// </summary>
internal static class LinuxX11RichClipboardService
{
    private const string X11 = "libX11.so.6";
    private const int SelectionClear = 29;
    private const int SelectionRequest = 30;
    private const int SelectionNotify = 31;
    private const int PropModeReplace = 0;
    private const ulong CurrentTime = 0;
    private const ulong None = 0;
    private const ulong XaAtom = 4;
    private const ulong XaString = 31;

    private static readonly object Sync = new();
    private static long _generation;

    public static bool TrySet(string plainText, string htmlDocument, string rtf, out string backend)
    {
        backend = string.Empty;
        if (!OperatingSystem.IsLinux() || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
            return false;

        var generation = Interlocked.Increment(ref _generation);
        var ready = new ManualResetEventSlim(false);
        Exception? startupError = null;
        var started = false;

        var payload = new ClipboardPayload(
            plainText ?? string.Empty,
            htmlDocument ?? string.Empty,
            rtf ?? string.Empty);

        var thread = new Thread(() =>
        {
            IntPtr display = IntPtr.Zero;
            ulong window = 0;
            try
            {
                display = XOpenDisplay(IntPtr.Zero);
                if (display == IntPtr.Zero)
                    throw new InvalidOperationException("XOpenDisplay failed.");

                var screen = XDefaultScreen(display);
                var root = XRootWindow(display, screen);
                window = XCreateSimpleWindow(display, root, 0, 0, 1, 1, 0, 0, 0);
                if (window == 0)
                    throw new InvalidOperationException("XCreateSimpleWindow failed.");

                var atoms = ClipboardAtoms.Create(display);
                XSetSelectionOwner(display, atoms.Clipboard, window, CurrentTime);
                XFlush(display);

                if (XGetSelectionOwner(display, atoms.Clipboard) != window)
                    throw new InvalidOperationException("Could not become X11 clipboard owner.");

                lock (Sync)
                {
                    started = true;
                    ready.Set();
                }

                while (generation == Volatile.Read(ref _generation))
                {
                    XNextEvent(display, out var xevent);
                    var type = xevent.Type;
                    if (type == SelectionClear)
                        break;
                    if (type != SelectionRequest)
                        continue;

                    HandleSelectionRequest(display, window, atoms, payload, ref xevent.SelectionRequest);
                }
            }
            catch (Exception ex)
            {
                lock (Sync)
                {
                    startupError = ex;
                    ready.Set();
                }
            }
            finally
            {
                if (display != IntPtr.Zero)
                {
                    if (window != 0) XDestroyWindow(display, window);
                    XCloseDisplay(display);
                }
            }
        })
        {
            IsBackground = true,
            Name = "CodyFormat X11 Clipboard"
        };

        thread.Start();
        if (!ready.Wait(TimeSpan.FromSeconds(2)))
        {
            backend = "Linux fallback (X11 clipboard initialization timed out)";
            return false;
        }

        if (!started || startupError is not null)
        {
            backend = startupError is null
                ? "Linux fallback"
                : $"Linux fallback ({startupError.Message})";
            return false;
        }

        backend = "Linux X11/XWayland rich clipboard";
        return true;
    }

    private static void HandleSelectionRequest(
        IntPtr display,
        ulong ownerWindow,
        ClipboardAtoms atoms,
        ClipboardPayload payload,
        ref XSelectionRequestEvent request)
    {
        var property = request.Property != None ? request.Property : request.Target;
        var success = false;

        if (request.Target == atoms.Targets)
        {
            var targets = new[]
            {
                atoms.Targets,
                atoms.Utf8String,
                atoms.Text,
                atoms.String,
                atoms.Html,
                atoms.HtmlUtf8,
                atoms.Rtf,
                atoms.RichText,
                atoms.ApplicationRtf,
                atoms.PlainUtf8,
                atoms.PlainText
            };
            success = ChangeAtomProperty(display, request.Requestor, property, targets);
        }
        else if (request.Target == atoms.Html || request.Target == atoms.HtmlUtf8)
        {
            success = ChangeBytesProperty(display, request.Requestor, property, request.Target,
                Encoding.UTF8.GetBytes(payload.HtmlDocument));
        }
        else if (request.Target == atoms.Rtf || request.Target == atoms.RichText || request.Target == atoms.ApplicationRtf)
        {
            success = ChangeBytesProperty(display, request.Requestor, property, request.Target,
                Encoding.ASCII.GetBytes(payload.Rtf));
        }
        else if (request.Target == atoms.Utf8String || request.Target == atoms.Text)
        {
            success = ChangeBytesProperty(display, request.Requestor, property, atoms.Utf8String,
                Encoding.UTF8.GetBytes(payload.PlainText));
        }
        else if (request.Target == atoms.PlainUtf8 || request.Target == atoms.PlainText)
        {
            success = ChangeBytesProperty(display, request.Requestor, property, request.Target,
                Encoding.UTF8.GetBytes(payload.PlainText));
        }
        else if (request.Target == atoms.String)
        {
            // XA_STRING is traditionally ISO-8859-1. Keep ASCII-compatible source intact and
            // replace unsupported characters rather than returning UTF-8 under the wrong target.
            var latin1 = Encoding.Latin1.GetBytes(payload.PlainText);
            success = ChangeBytesProperty(display, request.Requestor, property, XaString, latin1);
        }

        var notify = new XEvent
        {
            Selection = new XSelectionEvent
            {
                Type = SelectionNotify,
                Serial = 0,
                SendEvent = 1,
                Display = display,
                Requestor = request.Requestor,
                Selection = request.Selection,
                Target = request.Target,
                Property = success ? property : None,
                Time = request.Time
            }
        };

        XSendEvent(display, request.Requestor, false, 0, ref notify);
        XFlush(display);
    }

    private static bool ChangeBytesProperty(IntPtr display, ulong window, ulong property, ulong type, byte[] data)
    {
        if (property == None) return false;
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            XChangeProperty(display, window, property, type, 8, PropModeReplace,
                handle.AddrOfPinnedObject(), data.Length);
            return true;
        }
        finally
        {
            handle.Free();
        }
    }

    private static bool ChangeAtomProperty(IntPtr display, ulong window, ulong property, ulong[] atoms)
    {
        if (property == None) return false;
        var handle = GCHandle.Alloc(atoms, GCHandleType.Pinned);
        try
        {
            XChangeProperty(display, window, property, XaAtom, 32, PropModeReplace,
                handle.AddrOfPinnedObject(), atoms.Length);
            return true;
        }
        finally
        {
            handle.Free();
        }
    }

    private sealed record ClipboardPayload(string PlainText, string HtmlDocument, string Rtf);

    private readonly record struct ClipboardAtoms(
        ulong Clipboard,
        ulong Targets,
        ulong Utf8String,
        ulong Text,
        ulong String,
        ulong Html,
        ulong HtmlUtf8,
        ulong Rtf,
        ulong RichText,
        ulong ApplicationRtf,
        ulong PlainUtf8,
        ulong PlainText)
    {
        public static ClipboardAtoms Create(IntPtr display) => new(
            Intern(display, "CLIPBOARD"),
            Intern(display, "TARGETS"),
            Intern(display, "UTF8_STRING"),
            Intern(display, "TEXT"),
            XaString,
            Intern(display, "text/html"),
            Intern(display, "text/html;charset=utf-8"),
            Intern(display, "text/rtf"),
            Intern(display, "text/richtext"),
            Intern(display, "application/rtf"),
            Intern(display, "text/plain;charset=utf-8"),
            Intern(display, "text/plain"));

        private static ulong Intern(IntPtr display, string name) => XInternAtom(display, name, false);
    }

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    private struct XEvent
    {
        [FieldOffset(0)] public int Type;
        [FieldOffset(0)] public XSelectionRequestEvent SelectionRequest;
        [FieldOffset(0)] public XSelectionEvent Selection;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XSelectionRequestEvent
    {
        public int Type;
        private int _padding0;
        public ulong Serial;
        public int SendEvent;
        private int _padding1;
        public IntPtr Display;
        public ulong Owner;
        public ulong Requestor;
        public ulong Selection;
        public ulong Target;
        public ulong Property;
        public ulong Time;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XSelectionEvent
    {
        public int Type;
        private int _padding0;
        public ulong Serial;
        public int SendEvent;
        private int _padding1;
        public IntPtr Display;
        public ulong Requestor;
        public ulong Selection;
        public ulong Target;
        public ulong Property;
        public ulong Time;
    }

    [DllImport(X11)] private static extern IntPtr XOpenDisplay(IntPtr displayName);
    [DllImport(X11)] private static extern int XCloseDisplay(IntPtr display);
    [DllImport(X11)] private static extern int XDefaultScreen(IntPtr display);
    [DllImport(X11)] private static extern ulong XRootWindow(IntPtr display, int screenNumber);
    [DllImport(X11)] private static extern ulong XCreateSimpleWindow(IntPtr display, ulong parent, int x, int y, uint width, uint height, uint borderWidth, ulong border, ulong background);
    [DllImport(X11)] private static extern int XDestroyWindow(IntPtr display, ulong window);
    [DllImport(X11)] private static extern ulong XInternAtom(IntPtr display, [MarshalAs(UnmanagedType.LPStr)] string atomName, [MarshalAs(UnmanagedType.Bool)] bool onlyIfExists);
    [DllImport(X11)] private static extern int XSetSelectionOwner(IntPtr display, ulong selection, ulong owner, ulong time);
    [DllImport(X11)] private static extern ulong XGetSelectionOwner(IntPtr display, ulong selection);
    [DllImport(X11)] private static extern int XNextEvent(IntPtr display, out XEvent xevent);
    [DllImport(X11)] private static extern int XFlush(IntPtr display);
    [DllImport(X11)] private static extern int XSendEvent(IntPtr display, ulong window, [MarshalAs(UnmanagedType.Bool)] bool propagate, long eventMask, ref XEvent sendEvent);
    [DllImport(X11)] private static extern int XChangeProperty(IntPtr display, ulong window, ulong property, ulong type, int format, int mode, IntPtr data, int elementCount);
}
