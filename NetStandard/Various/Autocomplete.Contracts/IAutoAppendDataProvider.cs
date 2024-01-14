namespace Scar.Common.Autocomplete.Contracts;

public interface IAutoAppendDataProvider
{
    string GetAppendText(string textPattern, string firstMatch);
}