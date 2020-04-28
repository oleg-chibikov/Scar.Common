namespace Scar.Common.WPF.Controls
{
    public interface IAutoAppendDataProvider
    {
        string GetAppendText(string textPattern, string firstMatch);
    }
}
