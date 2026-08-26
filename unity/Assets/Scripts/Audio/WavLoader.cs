using System;
using System.IO;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Runtime PCM-WAV → <see cref="AudioClip"/> loader. The repo's audio bank
    /// (<c>assets/audio/...</c>, see AUDIO.md §1 "short mono WAV one-shots" +
    /// "looping streamed tracks") ships as raw <c>.wav</c> files that Unity never
    /// imports, so there are no AudioClip assets/GUIDs. Mirroring
    /// <see cref="SpriteLibrary"/>'s load-off-disk pattern, this parses the RIFF
    /// header (fmt + data chunks) and builds a clip via
    /// <c>AudioClip.Create</c> + <c>SetData</c>.
    ///
    /// Handles uncompressed PCM: 8-bit (unsigned) or 16-bit (signed), mono or
    /// stereo, any sample rate (SFX = 44.1 kHz mono, music = 22.05 kHz mono in
    /// this project). Anything malformed logs a warning and returns null — callers
    /// must treat a null clip as a silent no-op so audio never throws into
    /// gameplay.
    /// </summary>
    public static class WavLoader
    {
        /// <summary>Parse a PCM <c>.wav</c> file into an AudioClip, or null on any failure.</summary>
        public static AudioClip Load(string absolutePath)
        {
            try
            {
                if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
                {
                    Debug.LogWarning($"[WavLoader] Missing file: {absolutePath}");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(absolutePath);
                if (bytes.Length < 44) { Warn(absolutePath, "too small to be a WAV"); return null; }

                // ---- RIFF / WAVE header ----
                if (bytes[0] != 'R' || bytes[1] != 'I' || bytes[2] != 'F' || bytes[3] != 'F')
                { Warn(absolutePath, "not a RIFF file"); return null; }
                if (bytes[8] != 'W' || bytes[9] != 'A' || bytes[10] != 'V' || bytes[11] != 'E')
                { Warn(absolutePath, "not a WAVE file"); return null; }

                int audioFormat = 0, channels = 0, sampleRate = 0, bitsPerSample = 0;
                int dataOffset = -1, dataLength = 0;

                // ---- Walk the chunks (fmt may be followed by fact/LIST/etc. before data) ----
                int pos = 12;
                while (pos + 8 <= bytes.Length)
                {
                    string id = new string(new[] { (char)bytes[pos], (char)bytes[pos + 1], (char)bytes[pos + 2], (char)bytes[pos + 3] });
                    int size = BitConverter.ToInt32(bytes, pos + 4);
                    int body = pos + 8;
                    if (size < 0 || body + size > bytes.Length) size = bytes.Length - body; // tolerate a bad/rounded size

                    if (id == "fmt ")
                    {
                        audioFormat = BitConverter.ToUInt16(bytes, body + 0);
                        channels = BitConverter.ToUInt16(bytes, body + 2);
                        sampleRate = BitConverter.ToInt32(bytes, body + 4);
                        bitsPerSample = BitConverter.ToUInt16(bytes, body + 14);
                    }
                    else if (id == "data")
                    {
                        dataOffset = body;
                        dataLength = size;
                    }

                    // Chunks are word-aligned: an odd size carries a pad byte.
                    pos = body + size + (size & 1);
                    if (dataOffset >= 0 && sampleRate > 0) break;
                }

                if (dataOffset < 0) { Warn(absolutePath, "no data chunk"); return null; }
                if (channels <= 0 || sampleRate <= 0) { Warn(absolutePath, "bad fmt chunk"); return null; }
                // Format 1 = PCM. Format 0xFFFE (extensible) is fine for us as long as it wraps PCM.
                if (audioFormat != 1 && audioFormat != unchecked((ushort)0xFFFE))
                { Warn(absolutePath, $"unsupported audioFormat {audioFormat} (only PCM)"); return null; }
                if (bitsPerSample != 8 && bitsPerSample != 16)
                { Warn(absolutePath, $"unsupported bit depth {bitsPerSample} (only 8/16)"); return null; }

                int bytesPerSample = bitsPerSample / 8;
                int frameStride = bytesPerSample * channels;
                if (frameStride <= 0) { Warn(absolutePath, "bad frame stride"); return null; }

                int available = Mathf.Min(dataLength, bytes.Length - dataOffset);
                int totalSamples = available / bytesPerSample;       // across all channels
                int frames = totalSamples / channels;                 // per-channel sample count
                if (frames <= 0) { Warn(absolutePath, "empty audio data"); return null; }

                var samples = new float[frames * channels];

                if (bitsPerSample == 16)
                {
                    int si = 0;
                    for (int f = 0; f < frames; f++)
                    {
                        int baseIdx = dataOffset + f * frameStride;
                        for (int c = 0; c < channels; c++)
                        {
                            int b = baseIdx + c * 2;
                            short s = (short)(bytes[b] | (bytes[b + 1] << 8));
                            samples[si++] = s / 32768f;
                        }
                    }
                }
                else // 8-bit PCM is unsigned (0..255, silence at 128)
                {
                    int si = 0;
                    for (int f = 0; f < frames; f++)
                    {
                        int baseIdx = dataOffset + f * frameStride;
                        for (int c = 0; c < channels; c++)
                            samples[si++] = (bytes[baseIdx + c] - 128) / 128f;
                    }
                }

                string clipName = Path.GetFileNameWithoutExtension(absolutePath);
                var clip = AudioClip.Create(clipName, frames, channels, sampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WavLoader] Failed to load '{absolutePath}': {e.Message}");
                return null;
            }
        }

        private static void Warn(string path, string why) =>
            Debug.LogWarning($"[WavLoader] {why}: {path}");
    }
}
