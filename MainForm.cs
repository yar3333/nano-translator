namespace NanoTranslator;

public partial class MainForm : Form
{
    private readonly NotifyIcon trayIcon = new();

    private Keys lastPressedKey = Keys.A;

    private bool isControlPressed = false;

    public MainForm()
    {
        InitializeComponent();

        var trayContextMenu = new ContextMenuStrip();
        trayContextMenu.Items.Add(new ToolStripLabel("Press Ctrl+C+C to translate clipboard"));
        trayContextMenu.Items.Add("Exit", null, (sender, args) => Close());

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        trayIcon.Icon = new Icon(assembly.GetManifestResourceStream("NanoTranslator.icon.ico")!);
        trayIcon.ContextMenuStrip = trayContextMenu;

        trayIcon.Visible = true;
        trayIcon.Text = Text;

        InterceptKeys.OnKeyDown += key =>
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                isControlPressed = true;
            }

            if (isControlPressed && key == Keys.C && lastPressedKey == Keys.C)
            {
                trayIcon.Icon = new Icon(assembly.GetManifestResourceStream("NanoTranslator.icon_alt.ico")!);

                Task.Run(() =>
                {
                    translateClipboard();

                    Invoke(() =>
                    {
                        trayIcon.Icon = new Icon(assembly.GetManifestResourceStream("NanoTranslator.icon.ico")!);
                    });
                });
            }

            lastPressedKey = key;
        };

        InterceptKeys.OnKeyUp += key =>
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                isControlPressed = false;
            }
        };
    }

    private void translateClipboard()
    {
        // access to clipboard must be in STA thread
        var staThread = new Thread(() =>
        {
            try
            {
                var text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var fromLang = detectLang(text);
                    var toLang = fromLang == "en" ? "ru" : "en";
                    var resText = GoogleTranslator.TranslateAsync(fromLang, toLang, text).Result;
                    if (resText != "") Clipboard.SetText(resText, TextDataFormat.UnicodeText);
                }
            }
            catch {}
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }

    private string detectLang(string text)
    {
        const string enAbc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string ruAbc = "àáâãäå¸æçèéêëìíîïğñòóôõö÷øùúûüışÿÀÁÂÃÄÅ¨ÆÇÈÉÊËÌÍÎÏĞÑÒÓÔÕÖ×ØÙÚÛÜİŞß";

        var en = 0;
        var ru = 0;
        foreach (var c in text)
        {
            if (enAbc.Contains(c)) en++;
            else if (ruAbc.Contains(c)) ru++;
        }

        return ru >= en ? "ru" : "en";
    }
}
