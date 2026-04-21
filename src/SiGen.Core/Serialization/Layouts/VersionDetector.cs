using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts
{
    /// <summary>
    /// Information about a layout file's format and version.
    /// </summary>
    public class FileFormatInfo
    {
        /// <summary>
        /// Gets the detected file format.
        /// </summary>
        public FileFormat Format { get; init; }

        /// <summary>
        /// Gets the version number found in the file.
        /// </summary>
        public int Version { get; init; }

        /// <summary>
        /// Gets whether the format and version are supported.
        /// </summary>
        public bool IsSupported { get; init; }
    }

    /// <summary>
    /// Detects the format and version of layout files.
    /// </summary>
    public static class VersionDetector
    {
        private const int BufferSize = 512;

        /// <summary>
        /// Detects the format and version of a layout file.
        /// </summary>
        /// <param name="stream">The stream to examine. Stream position will be reset to 0.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the file format and version.</returns>
        public static async Task<FileFormatInfo> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.", nameof(stream));

            stream.Position = 0;

            var buffer = new byte[BufferSize];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            stream.Position = 0;

            if (bytesRead == 0)
            {
                return new FileFormatInfo
                {
                    Format = FileFormat.Unknown,
                    Version = 0,
                    IsSupported = false
                };
            }

            var content = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimStart();

            if (content.StartsWith("<"))
            {
                return DetectXmlVersion(content);
            }
            else if (content.StartsWith("{"))
            {
                return DetectJsonVersion(content);
            }

            return new FileFormatInfo
            {
                Format = FileFormat.Unknown,
                Version = 0,
                IsSupported = false
            };
        }

        private static FileFormatInfo DetectXmlVersion(string content)
        {
            int version = 1;

            var versionMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"<Layout[^>]*Version\s*=\s*[""'](\d+)[""']",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out var v))
            {
                version = v;
            }

            return new FileFormatInfo
            {
                Format = FileFormat.Xml,
                Version = version,
                IsSupported = version >= 1 && version <= 2
            };
        }

        private static FileFormatInfo DetectJsonVersion(string content)
        {
            int version = 2;

            var versionMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"""[Vv]ersion""\s*:\s*(\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out var v))
            {
                version = v;
            }

            return new FileFormatInfo
            {
                Format = FileFormat.Json,
                Version = version,
                IsSupported = version >= 2
            };
        }
    }
}
