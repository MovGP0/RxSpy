using System.Text.Json;

namespace RxSpy.AspNet;

public sealed class RxSpyOptions
{
    public string Endpoint { get; set; } = "/rxspy/stream";

    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        WriteIndented = false
    };
}