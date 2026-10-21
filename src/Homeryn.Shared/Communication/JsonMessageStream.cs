using System.Buffers.Binary;
using System.Text.Json;

namespace Homeryn.Shared.Communication;

public static class JsonMessageStream
{
    public const int MaximumPayloadBytes = 4096;

    public static async Task<T?> ReadAsync<T>(Stream stream, CancellationToken cancellationToken)
        where T : class
    {
        byte[] header = new byte[sizeof(int)];
        int bytesRead = await stream.ReadAsync(header, cancellationToken);
        if (bytesRead == 0)
        {
            return null;
        }

        await stream.ReadExactlyAsync(header.AsMemory(bytesRead), cancellationToken);
        int length = BinaryPrimitives.ReadInt32BigEndian(header);
        if (length is <= 0 or > MaximumPayloadBytes)
        {
            throw new InvalidDataException("The JSON payload length is outside the permitted range.");
        }

        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload, cancellationToken);
        return JsonSerializer.Deserialize<T>(payload, JsonSerializerOptions.Web)
            ?? throw new JsonException("A JSON message must not be null.");
    }

    public static async Task WriteAsync<T>(Stream stream, T message, CancellationToken cancellationToken)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message, JsonSerializerOptions.Web);
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