namespace XFStyleCreatorBlazor.Helpers
{
    public interface IClipboardService
    {
        Task CopyToClipboard(string text);
    }
}
