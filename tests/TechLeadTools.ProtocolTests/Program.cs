using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using TechLeadTools.Protocol;

var failures = new List<string>();
var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures.json");
var fixtures = ReadFixtures(fixturePath);

foreach (var fixture in fixtures)
{
    Run($"round-trip: {fixture.Name}", () =>
    {
        var payload = fixture.ToPayload();
        Equal(fixture.Header, TltProtocol.CreateHeader(payload));
        var serialized = TltProtocol.Serialize(payload, fixture.Content);
        var parsed = TltProtocol.Parse(serialized);
        Equal(fixture.Content, parsed.Content);
        AssertPayload(payload, parsed.Payload);

        var crlf = serialized.Replace("\n", "\r\n");
        AssertPayload(payload, TltProtocol.Parse(crlf).Payload);
    });
}

Run("rejeita travessia de diretório", () =>
{
    True(TltProtocol.IsSafeRelativePath("src/Service.cs"));
    False(TltProtocol.IsSafeRelativePath("../Service.cs"));
    False(TltProtocol.IsSafeRelativePath("src/../Service.cs"));
    False(TltProtocol.IsSafeRelativePath("C:/src/Service.cs"));
    False(TltProtocol.IsSafeRelativePath(@"src\Service.cs"));
});

Run("normaliza seleção terminada na coluna 1", () =>
{
    var range = LineRange.FromSelection(4, 7, 1, false);
    Equal(4, range.StartLine);
    Equal(6, range.EndLine);
});

Run("rejeita cabeçalho adulterado", () =>
{
    var fixture = fixtures[0];
    var text = TltProtocol.Serialize(fixture.ToPayload(), fixture.Content)
        .Replace(fixture.Header, "Outro.cs:Global:1");
    Throws<FormatException>(() => TltProtocol.Parse(text));
});

if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} teste(s) falharam:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"- {failure}");
    }

    return 1;
}

Console.WriteLine($"{fixtures.Count + 3} testes do protocolo TLT/1 passaram.");
return 0;

void Run(string name, Action action)
{
    try
    {
        action();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{name}: {exception.Message}");
    }
}

static List<Fixture> ReadFixtures(string path)
{
    var serializer = new DataContractJsonSerializer(typeof(List<Fixture>));
    using var stream = File.OpenRead(path);
    return (List<Fixture>)(serializer.ReadObject(stream)
        ?? throw new InvalidDataException("Fixtures não encontradas."));
}

static void AssertPayload(TltPayload expected, TltPayload actual)
{
    Equal(expected.Workspace, actual.Workspace);
    Equal(expected.Path, actual.Path);
    Equal(expected.File, actual.File);
    Equal(expected.ClassName, actual.ClassName);
    Equal(expected.StartLine, actual.StartLine);
    Equal(expected.EndLine, actual.EndLine);
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Esperado '{expected}', obtido '{actual}'.");
    }
}

static void True(bool value)
{
    Equal(true, value);
}

static void False(bool value)
{
    Equal(false, value);
}

static void Throws<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Era esperada uma exceção {typeof(TException).Name}.");
}

[DataContract]
internal sealed class Fixture
{
    [DataMember(Name = "name", IsRequired = true)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "header", IsRequired = true)]
    public string Header { get; set; } = string.Empty;

    [DataMember(Name = "workspace", IsRequired = true)]
    public string Workspace { get; set; } = string.Empty;

    [DataMember(Name = "path", IsRequired = true)]
    public string Path { get; set; } = string.Empty;

    [DataMember(Name = "file", IsRequired = true)]
    public string File { get; set; } = string.Empty;

    [DataMember(Name = "class", IsRequired = true)]
    public string ClassName { get; set; } = string.Empty;

    [DataMember(Name = "startLine", IsRequired = true)]
    public int StartLine { get; set; }

    [DataMember(Name = "endLine", IsRequired = true)]
    public int EndLine { get; set; }

    [DataMember(Name = "content", IsRequired = true)]
    public string Content { get; set; } = string.Empty;

    public TltPayload ToPayload() => new TltPayload
    {
        Workspace = Workspace,
        Path = Path,
        File = File,
        ClassName = ClassName,
        StartLine = StartLine,
        EndLine = EndLine
    };
}
