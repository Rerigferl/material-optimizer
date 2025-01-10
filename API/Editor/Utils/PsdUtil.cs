using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls;
using UnityEngine;

namespace Numeira.MaterialOptimizer.API.Utils;

public static class PsdUtil
{
    public static Texture2D? LoadImage(string path)
    {
        using var fs = File.OpenRead(path);
        if (!FileHeader.TryRead(fs, out var header))
            return null;

        // めんどいから変なのは無視
        if (header is not { Channels: 3 or 4, Depth: 8, ColorMode: ColorMode.RGB })
            return null;
                
        // Color Mode Data 
        {
            var pos = (int)fs.Position;
            var length = fs.ReadUInt32BigEndian();
            fs.Position += length;
        }

        // Image Resources
        {
            var pos = (int)fs.Position;
            var length = fs.ReadUInt32BigEndian();
            fs.Position += length;
        }

        // Layer and Mask Information
        {
            var pos = (int)fs.Position;
            var length = header.Version is 1 ? fs.ReadUInt32BigEndian() : (long)fs.ReadUInt64BigEndian();
            fs.Position += length;
        }

        var compressionMethod = (CompressionMode)fs.ReadUInt16BigEndian();
        var resultLength = header.GetImageDataSize();
        var dataRemains = (int)(fs.Length - fs.Position);

        var resultArray = ArrayPool<byte>.Shared.Rent(resultLength);
        var result = resultArray.AsSpan(0, resultLength);

        byte[]? dataArray;
        Span<byte> data;
        if (resultLength == dataRemains)
        {
            dataArray = null;
            data = result;
        }
        else
        {
            dataArray = ArrayPool<byte>.Shared.Rent(dataRemains);
            data = dataArray.AsSpan(0, dataRemains);
        }

        fs.Read(data);

        try
        {
            if (header.Depth == 1)
            {
                DecodeBitArray(data, result);
            }
            else if (compressionMethod == CompressionMode.RLE)
            {
                DecodeRunLengthEncodedImageData(data, result, header);
            }
            else if (header.Depth > 8)
            {
                if (header.Depth == 16)
                    ReverseEndianness<ushort>(result);
                else
                    ReverseEndianness<uint>(result);
            }

            var texture = new Texture2D(header.Width, header.Height, header switch
            {
                { Channels: 3, Depth: 8 } => TextureFormat.RGB24,
                { Channels: 4, Depth: 8 } => TextureFormat.RGBA32,
                _ => TextureFormat.RGBA32,
            }, false);

            unsafe
            {
                PackingRGB32(header, result);
                texture.LoadRawTextureData((IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(result)), result.Length);
            }

            return texture;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(resultArray);
            ArrayPool<byte>.Shared.Return(dataArray);
        }
    }

    static void PackingRGB32(in FileHeader header, Span<byte> write)
    {
        var width = header.Width;
        var height = header.Height;
        var channelByteCount = width * height;

        Span<byte> tempBuf = new byte[write.Length];

        var r = write.Slice(channelByteCount * 0, channelByteCount);
        var g = write.Slice(channelByteCount * 1, channelByteCount);
        var b = write.Slice(channelByteCount * 2, channelByteCount);

        for (var i = 0; channelByteCount > i; i += 1)
        {
            var wi = i * 3;
            tempBuf[wi + 0] = r[i];
            tempBuf[wi + 1] = g[i];
            tempBuf[wi + 2] = b[i];
        }


        var lineBuffer = new byte[width * 3];
        for(int i = 0; i < height / 2; i++)
        {
            int i2 = height - 1 - i;
            if (i == i2)
                break;

            int a = width * 3;
            var line1 = tempBuf.Slice(a * i, lineBuffer.Length);
            var line2 = tempBuf.Slice(a * i2, lineBuffer.Length);
            line1.CopyTo(lineBuffer);
            line2.CopyTo(line1);
            lineBuffer.CopyTo(line2);
        }

        tempBuf.CopyTo(write);
    }


    private static void DecodeBitArray(ReadOnlySpan<byte> data, Span<byte> result)
    {
        var resultAs64Bits = MemoryMarshal.Cast<byte, ulong>(result);
        if (data.Length != resultAs64Bits.Length)
            return;

        for(int i = 0; i < data.Length; i++)
        {
            var x = data[i];
            ref var y = ref resultAs64Bits[i];

            for (int i2 = 8 - 1; i2 >= 0; i2--)
            {
                bool flag = (x & (1 << i2)) != 0;
                Unsafe.Add(ref Unsafe.As<ulong, byte>(ref y), 7 - i2) = flag ? byte.MaxValue : byte.MinValue;
            }
        }
    }

    private static void DecodeRunLengthEncodedImageData(ReadOnlySpan<byte> data, Span<byte> result, in FileHeader header)
    {
        var (width, height) = (header.Width, header.Height);

        int rowCount = height * header.Channels;
        int pos = rowCount * 2;

        for (int i = 0; i < rowCount; i++)
        {
            var decoded = result.Slice(width * i, width);
            var count = BinaryPrimitives.ReadUInt16BigEndian(data[(i * 2)..]);
            if (count == 0)
            {
                decoded.Fill(0);
                continue;
            }

            var encoded = data.Slice(pos, count);
            UnpackBits(encoded, decoded);
            pos += count;
        }

        static void UnpackBits(ReadOnlySpan<byte> encoded, Span<byte> decoded)
        {
            int e = 0;
            int d = 0;

            while (e < encoded.Length)
            {
                var num = (sbyte)encoded[e++];
                if (num >= 0)
                {
                    // 非連続
                    var length = num + 1;
                    encoded.Slice(e, length).CopyTo(decoded[d..]);
                    e += length;
                    d += length;
                }
                else
                {
                    // 連続
                    var count = (-num) + 1;
                    var value = encoded[e++];
                    decoded.Slice(d, count).Fill(value);
                    d += count;
                }
            }
        }
    }

    private static void ReverseEndianness<T>(Span<byte> data) where T : struct
    {
        foreach (ref var x in MemoryMarshal.Cast<byte, T>(data))
        {
            if (typeof(T) == typeof(ushort))
            {
                var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ushort>(ref x));
                x = Unsafe.As<ushort, T>(ref value);
            }
            if (typeof(T) == typeof(uint))
            {
                var value = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, uint>(ref x));
                x = Unsafe.As<uint, T>(ref value);
            }
        }
    }

    private static ushort ReadUInt16BigEndian(this Stream stream)
    {
        var buffer = (stackalloc byte[sizeof(ushort)]);
        stream.Read(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    private static uint ReadUInt32BigEndian(this Stream stream)
    {
        var buffer = (stackalloc byte[sizeof(uint)]);
        stream.Read(buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    private static ulong ReadUInt64BigEndian(this Stream stream)
    {
        var buffer = (stackalloc byte[sizeof(ulong)]);
        stream.Read(buffer);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 26)]
    private struct FileHeader
    {
        public uint Signature; // "8BPS"u8

        public ushort Version; // 1, but PSB is 2

        private readonly uint Reserved1;
        private readonly ushort Reserved2; // 6 bytes, 0

        public short Channels; // 1 ~ 56

        public int Height; // 1 ~ 30_000 (PSB: 300_000)

        public int Width; // 1 ~ 30_000 (PSB: 300_000)

        public short Depth; // 1, 8, 16, 32

        public ColorMode ColorMode; // Bitmap = 0; Grayscale = 1; Indexed = 2; RGB = 3; CMYK = 4; Multichannel = 7; Duotone = 8; Lab = 9

        public static bool TryRead(Stream stream, out FileHeader header)
        {
            Unsafe.SkipInit(out header);
            if (stream.Length < Unsafe.SizeOf<FileHeader>())
                return false;

            var span = MemoryMarshal.CreateSpan(ref Unsafe.As<FileHeader, byte>(ref Unsafe.AsRef(header)), Unsafe.SizeOf<FileHeader>());
            stream.Read(span);
            if (BinaryPrimitives.ReverseEndianness(header.Signature) != 0x38_42_50_53u)
                return false;

            header.Version = BinaryPrimitives.ReverseEndianness(header.Version);
            header.Channels = BinaryPrimitives.ReverseEndianness(header.Channels);
            header.Height = BinaryPrimitives.ReverseEndianness(header.Height);
            header.Width = BinaryPrimitives.ReverseEndianness(header.Width);
            header.Depth = BinaryPrimitives.ReverseEndianness(header.Depth);
            header.ColorMode = (ColorMode)BinaryPrimitives.ReverseEndianness((ushort)header.ColorMode);

            Debug.Assert(header.Version is 1 or 2);
            Debug.Assert(header.Channels is >= 1 and <= 56);
            Debug.Assert(header is { Version: 1, Height: >= 1 and <= 30_000 } or { Version: 2, Height: >= 1 and <= 300_000 });
            Debug.Assert(header is { Version: 1, Width:  >= 1 and <= 30_000 } or { Version: 2, Width:  >= 1 and <= 300_000 });
            Debug.Assert(header.Depth is 1 or 8 or 16 or 32);

            return true;
        }

        public readonly int GetImageDataSize() => Height * Width * Math.Max(Depth / 8, 1) * Channels;
    }

    private enum ColorMode : ushort
    {
        Bitmap = 0,
        Grayscale = 1,
        Indexed = 2,
        RGB = 3,
        CMYK = 4,
        Multichannel = 7,
        Duotone = 8,
        Lab = 9,
    }

    private enum CompressionMode
    {
        Raw = 0,
        RLE = 1,
        Zip = 2,
        ZipWithPrediction = 3,
    }
}