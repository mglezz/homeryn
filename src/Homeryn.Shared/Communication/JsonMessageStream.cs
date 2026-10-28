using System.Buffers.Binary;
using System.Text.Json;

namespace Homeryn.Shared.Communication
{
    public static class JsonMessageStream
    {
        public const int MaximumPayloadBytes = 4096;
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            RespectRequiredConstructorParameters = true
        };

        public static async Task<T?> ReadAsync<T>(Stream stream, CancellationToken cancellationToken)
            where T : class
        {
            byte[] header = new byte[sizeof(int)];
            int bytesRead = await stream.ReadAsync(header, cancellationToken);
            if (bytesRead == 0)
            {
                return null;
            }

            // TCP may deliver the header in several reads.
            await stream.ReadExactlyAsync(header.AsMemory(bytesRead), cancellationToken);
            int length = BinaryPrimitives.ReadInt32BigEndian(header);
            if (length <= 0 || length > MaximumPayloadBytes)
            {
                throw new InvalidDataException("The JSON payload length is outside the permitted range.");
            }

            byte[] payload = new byte[length];
            await stream.ReadExactlyAsync(payload, cancellationToken);
            T? message = JsonSerializer.Deserialize<T>(payload, options);
            if (message == null)
            {
                throw new JsonException("A JSON message must not be null.");
            }

            return message;
        }

        public static async Task WriteAsync<T>(Stream stream, T message, CancellationToken cancellationToken)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message, options);
            if (payload.Length > MaximumPayloadBytes)
            {
                throw new InvalidDataException("The JSON payload exceeds the permitted length.");
            }

            byte[] header = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
            await stream.WriteAsync(header, cancellationToken);
            await stream.WriteAsync(payload, cancellationToken);
        }
    }
}
