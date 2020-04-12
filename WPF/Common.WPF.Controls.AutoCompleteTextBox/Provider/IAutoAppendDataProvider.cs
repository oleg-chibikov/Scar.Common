namespace Scar.Common.WPF.Controls.AutoCompleteTextBox.Provider
{
    public interface IAutoAppendDataProvider
    {
        string GetAppendText(string textPattern, string firstMatch);
    }
}