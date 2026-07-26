using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace TechLeadTools.Protocol
{
    public static class TltProtocol
    {
        public const string Version = "TLT/1";

        private static readonly Regex BlockPattern = new Regex(
            @"\A([^\r\n]*)\r?\nTLT/1 ([^\r\n]+)\r?\n---(?:\r?\n|\z)([\s\S]*)\z",
            RegexOptions.CultureInvariant);

        public static string CreateHeader(TltPayload payload)
        {
            Validate(payload);
            var lines = payload.StartLine == payload.EndLine
                ? payload.StartLine.ToString()
                : $"{payload.StartLine}-{payload.EndLine}";

            return $"{payload.File}:{payload.ClassName}:{lines}";
        }

        public static string Serialize(TltPayload payload, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Validate(payload);
            var normalizedContent = content.Replace("\r\n", "\n");
            return $"{CreateHeader(payload)}\n{Version} {SerializeJson(payload)}\n---\n{normalizedContent}";
        }

        public static TltBlock Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var match = BlockPattern.Match(text);
            if (!match.Success)
            {
                throw new FormatException("A área de transferência não contém um bloco TLT/1 válido.");
            }

            TltPayload payload;
            try
            {
                payload = DeserializeJson(match.Groups[2].Value);
            }
            catch (Exception exception) when (
                exception is SerializationException
                || exception is InvalidDataContractException
                || exception is FormatException)
            {
                throw new FormatException("Os metadados JSON do bloco TLT/1 são inválidos.", exception);
            }

            Validate(payload);
            if (!string.Equals(match.Groups[1].Value, CreateHeader(payload), StringComparison.Ordinal))
            {
                throw new FormatException("O cabeçalho do bloco TLT/1 não corresponde aos metadados.");
            }

            return new TltBlock(payload, match.Groups[3].Value);
        }

        public static void Validate(TltPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            RequireValue(payload.Workspace, "workspace");
            RequireValue(payload.Path, "path");
            RequireValue(payload.File, "file");
            RequireValue(payload.ClassName, "class");

            if (payload.StartLine < 1 || payload.EndLine < payload.StartLine)
            {
                throw new FormatException("Intervalo de linhas TLT inválido.");
            }

            if (!IsSafeRelativePath(payload.Path))
            {
                throw new FormatException("O caminho do bloco TLT não é relativo e seguro.");
            }

            var lastSegment = payload.Path.Split('/').Last();
            if (!string.Equals(lastSegment, payload.File, StringComparison.Ordinal)
                || payload.File.IndexOfAny(new[] { '/', '\\' }) >= 0)
            {
                throw new FormatException("O nome do arquivo não corresponde ao caminho TLT.");
            }
        }

        public static bool IsSafeRelativePath(string value)
        {
            if (string.IsNullOrEmpty(value)
                || value.StartsWith("/", StringComparison.Ordinal)
                || value.StartsWith("\\", StringComparison.Ordinal)
                || value.Contains("\\")
                || Regex.IsMatch(value, @"^[A-Za-z]:", RegexOptions.CultureInvariant))
            {
                return false;
            }

            return value.Split('/').All(segment =>
                segment.Length > 0
                && !string.Equals(segment, ".", StringComparison.Ordinal)
                && !string.Equals(segment, "..", StringComparison.Ordinal));
        }

        private static void RequireValue(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException($"Campo TLT inválido: {name}.");
            }
        }

        private static string SerializeJson(TltPayload payload)
        {
            var serializer = new DataContractJsonSerializer(typeof(TltPayload));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, payload);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static TltPayload DeserializeJson(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(TltPayload));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (TltPayload)(serializer.ReadObject(stream)
                    ?? throw new SerializationException("Metadados TLT ausentes."));
            }
        }
    }
}
