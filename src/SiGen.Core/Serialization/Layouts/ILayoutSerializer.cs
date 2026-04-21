using SiGen.Layouts.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SiGen.Serialization.Layouts
{
    /// <summary>
    /// Defines methods for serializing and deserializing layout configurations.
    /// </summary>
    public interface ILayoutSerializer
    {
        /// <summary>
        /// Gets the file format this serializer handles.
        /// </summary>
        FileFormat Format { get; }

        /// <summary>
        /// Deserializes a layout configuration from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The deserialized layout configuration.</returns>
        Task<InstrumentLayoutConfiguration> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes a layout configuration to a stream.
        /// </summary>
        /// <param name="configuration">The configuration to serialize.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SerializeAsync(InstrumentLayoutConfiguration configuration, Stream stream, CancellationToken cancellationToken = default);
    }
}
