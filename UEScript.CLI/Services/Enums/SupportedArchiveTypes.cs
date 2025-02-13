namespace UEScript.CLI.Services.Enums;

public enum SupportedArchivePackTypes
{
    zip,
    tar,
    gz,
    unknown
}

public enum SupportedArchiveExtractionTypes
{
    zip,
    rar,
    sevenz,
    tar,
    gz,
    unknown
}

public static class Archives
{

    public static string GetArchiveExtension(SupportedArchivePackTypes value)
        => value switch
        {
            SupportedArchivePackTypes.zip => ".zip",
            SupportedArchivePackTypes.tar => ".tar",
            SupportedArchivePackTypes.gz => ".gz",
            _ => "unknown"
        };

    public static string GetArchiveExtension(SupportedArchiveExtractionTypes value)
        => value switch
        {
            SupportedArchiveExtractionTypes.zip => ".zip",
            SupportedArchiveExtractionTypes.tar => ".tar",
            SupportedArchiveExtractionTypes.gz => ".gz",
            SupportedArchiveExtractionTypes.rar => ".rar",
            SupportedArchiveExtractionTypes.sevenz => ".7z",
            _ => "unknown"
        };

    public static SupportedArchivePackTypes GetArchivePackType(string extension)
        => extension switch
        {
            ".zip" => SupportedArchivePackTypes.zip,
            ".tar" => SupportedArchivePackTypes.tar,
            ".gz" => SupportedArchivePackTypes.gz,
            _ => SupportedArchivePackTypes.unknown
        };

    public static SupportedArchiveExtractionTypes GetArchiveExtractionType(string extension)
        => extension switch
        {
            ".zip" => SupportedArchiveExtractionTypes.zip,
            ".tar" => SupportedArchiveExtractionTypes.tar,
            ".gz" => SupportedArchiveExtractionTypes.gz,
            ".rar" => SupportedArchiveExtractionTypes.rar,
            ".7z" => SupportedArchiveExtractionTypes.sevenz,
            _ => SupportedArchiveExtractionTypes.unknown
        };
}