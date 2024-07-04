using Android.Content;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using static Android.Content.ClipboardManager;
using ClipboardManager = Android.Content.ClipboardManager;

namespace iosKeyboardTest.Android
{

    public class ClipboardListener : Java.Lang.Object, IOnPrimaryClipChangedListener
    {
        private string _lastText;
        private Context _context;
        ClipboardManager _clipboardManager;
        public ClipboardManager ClipboardManager
        {
            get
            {
                if(_clipboardManager == null)
                {
                    _clipboardManager = (ClipboardManager)_context.GetSystemService(Context.ClipboardService);
                }
                return _clipboardManager;
            }
        }
        public event EventHandler<object> OnClipboardChanged;
        public ClipboardListener(Context context)
        {
            _context = context;
        }
        public void Start()
        {
            ClipboardManager.AddPrimaryClipChangedListener(this);
            Task.Run(async () =>
            {
                await RunListener();
            });
            Dispatcher.UIThread.Post(async () =>
            {
                await RunListener();
            });
        }
        async Task RunListener()
        {
            while(true)
            {
                await Task.Delay(100);
                if(ClipboardManager.Text != _lastText)
                {
                    _lastText = ClipboardManager.Text;
                    OnClipboardChanged?.Invoke(this, _lastText);
                }
            }
        }
        public void Stop()
        {
            ClipboardManager.RemovePrimaryClipChangedListener(this);
        }
        public void OnPrimaryClipChanged()
        {
            OnClipboardChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
