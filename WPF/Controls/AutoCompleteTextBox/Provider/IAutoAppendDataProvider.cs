namespace Scar.Common.WPF.Controls.Provider
{
    public interface IAutoAppendDataProvider
    {
        string GetAppendText(string textPattern, string firstMatch);
    }
}
