using Microsoft.JSInterop;

namespace XFStyleCreatorBlazor.Helpers
{
    public class ClipboardService : IClipboardService
    {
        private readonly IJSRuntime _jsInterop;

        public ClipboardService(IJSRuntime jsInterop)
        {
            _jsInterop = jsInterop;
        }

        public async Task CopyToClipboard(string text)
        {
            await Task.Run(async () => await _jsInterop.InvokeVoidAsync("navigator.clipboard.writeText", text));
        }
    }
}
