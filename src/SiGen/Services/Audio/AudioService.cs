using MeltySynth;
using PortAudioSharp;
using SiGen.Data.Common;
using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SiGen.Services.Audio
{
    /// <summary>
    /// Singleton service for MIDI audio playback using MeltySynth and PortAudio.
    /// Handles SoundFont profile selection and real-time note playback.
    /// </summary>
    /// <remarks>
    /// Uses PortAudio (cross-platform audio I/O library).
    /// Requires native PortAudio library (portaudio.dll / libportaudio.so / libportaudio.dylib).
    /// </remarks>
    public class AudioService : IDisposable
    {
        private const int SampleRate = 44100;
        private const int FramesPerBuffer = 512;

        private PortAudioSharp.Stream? _stream;

        private Synthesizer? _activeSynthesizer;
        private readonly Dictionary<string, Synthesizer> _synthesizerCache = new();
        private readonly List<SoundFontProfile> _profiles = new();

        private readonly float[] _renderBufferLeft = new float[FramesPerBuffer];
        private readonly float[] _renderBufferRight = new float[FramesPerBuffer];

        private bool _profilesLoaded;
        private bool _initialized;
        private bool _disposed;

        private readonly string _soundFontDirectory;

        public IReadOnlyList<SoundFontProfile> Profiles => _profiles;

        public AudioService()
        {
            _soundFontDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "SoundFonts");
        }

        /// <summary>
        /// Selects the best SoundFont profile for the given instrument type and string gauge.
        /// Profiles are loaded from the SoundFonts directory on first call.
        /// </summary>
        public SoundFontProfile? SelectProfile(InstrumentType instrumentType, Measure? gauge)
        {
            EnsureProfilesLoaded();
            if (_profiles.Count == 0) return null;

            double gaugeMm = !Measure.IsNullOrEmpty(gauge) ? gauge!.Value[LengthUnit.Mm] : 0;

            var candidates = _profiles
                .Where(p => p.IsAvailable && p.PreferredInstruments.Contains(instrumentType))
                .ToList();

            if (candidates.Count == 0)
                candidates = _profiles.Where(p => p.IsAvailable && p.PreferredInstruments.Contains(InstrumentType.ElectricGuitar)).ToList();

            if (candidates.Count == 0) return null;

            if (gaugeMm > 0)
            {
                var gaugeMatch = candidates.FirstOrDefault(p => gaugeMm >= p.MinGaugeMm && gaugeMm <= p.MaxGaugeMm);
                if (gaugeMatch != null)
                    return gaugeMatch;
            }

            return candidates.FirstOrDefault();
        }

        /// <summary>
        /// Plays a MIDI note using the given SoundFont profile.
        /// Stops any currently playing note on channel 0 before starting the new one.
        /// </summary>
        /// <param name="midiNote">MIDI note number (0-127).</param>
        /// <param name="profile">SoundFont profile to use.</param>
        /// <param name="velocity">Note velocity (0-127).</param>
        /// <param name="pitchBendCents">Optional pitch bend in cents for microtonal tuning.</param>
        public void PlayNote(int midiNote, SoundFontProfile? profile, int velocity = 100, double? pitchBendCents = null)
        {
            if (_disposed || profile == null || !profile.IsAvailable) return;

            var synthesizer = GetOrLoadSynthesizer(profile.FilePath);
            if (synthesizer == null) return;

            if (!EnsureInitialized(synthesizer)) return;

            lock (_activeSynthesizer!)
            {
                _activeSynthesizer.NoteOffAll(true);

                // Always set pitch bend (even if 0) to ensure previous bend doesn't persist
                double bendCents = pitchBendCents ?? 0.0;
                int pitchBendValue = MidiPitchConverter.CentsToMidiPitchBend(bendCents);
                // MeltySynth uses ProcessMidiMessage for pitch bend: 0xE0 | channel
                _activeSynthesizer.ProcessMidiMessage(0, 0xE0, pitchBendValue & 0x7F, (pitchBendValue >> 7) & 0x7F);

                _activeSynthesizer.NoteOn(0, Math.Clamp(midiNote, 0, 127), Math.Clamp(velocity, 0, 127));
            }
        }

        /// <summary>
        /// Plays a note from a PitchInterval with automatic MIDI note + pitch bend calculation.
        /// </summary>
        /// <param name="pitchInterval">Pitch interval (in cents from C0).</param>
        /// <param name="profile">SoundFont profile to use.</param>
        /// <param name="velocity">Note velocity (0-127).</param>
        public void PlayNote(PitchInterval pitchInterval, SoundFontProfile? profile, int velocity = 100)
        {
            var midiNote = MidiPitchConverter.FromPitchInterval(pitchInterval);
            PlayNote(midiNote.MidiNote, profile, velocity, midiNote.PitchBendCents);
        }

        private void EnsureProfilesLoaded()
        {
            if (_profilesLoaded) return;
            _profilesLoaded = true;

            if (!Directory.Exists(_soundFontDirectory)) return;

            foreach (var file in Directory.GetFiles(_soundFontDirectory, "*.sf2"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                _profiles.Add(BuildProfileFromFileName(name, file));
            }
        }

        private static SoundFontProfile BuildProfileFromFileName(string name, string filePath)
        {
            var lower = name.ToLowerInvariant();

            if (lower.Contains("bass"))
                return new SoundFontProfile
                {
                    Name = name,
                    FilePath = filePath,
                    PreferredInstruments = [InstrumentType.ElectricBass, InstrumentType.AcousticBass],
                    MinGaugeMm = 1.0
                };

            if (lower.Contains("acoustic"))
                return new SoundFontProfile
                {
                    Name = name,
                    FilePath = filePath,
                    PreferredInstruments = [InstrumentType.AcousticGuitar, InstrumentType.ClassicalGuitar],
                    MaxGaugeMm = 1.65
                };

            if (lower.Contains("guitar"))
                return new SoundFontProfile
                {
                    Name = name,
                    FilePath = filePath,
                    PreferredInstruments = [InstrumentType.ElectricGuitar],
                    MaxGaugeMm = 1.8
                };

            return new SoundFontProfile { Name = name, FilePath = filePath };
        }

        private bool EnsureInitialized(Synthesizer synthesizer)
        {
            if (_initialized)
            {
                if (_activeSynthesizer != synthesizer)
                {
                    lock (_activeSynthesizer!)
                        _activeSynthesizer.NoteOffAll(true);
                    _activeSynthesizer = synthesizer;
                }
                return true;
            }

            try
            {
                PortAudio.Initialize();
                _activeSynthesizer = synthesizer;

                var streamParams = new StreamParameters
                {
                    device = PortAudio.DefaultOutputDevice,
                    channelCount = 2,
                    sampleFormat = SampleFormat.Float32,
                    suggestedLatency = PortAudio.GetDeviceInfo(PortAudio.DefaultOutputDevice).defaultLowOutputLatency
                };

                _stream = new PortAudioSharp.Stream(
                    inParams: null,
                    outParams: streamParams,
                    sampleRate: SampleRate,
                    framesPerBuffer: (uint)FramesPerBuffer,
                    streamFlags: StreamFlags.NoFlag,
                    callback: AudioCallback,
                    userData: IntPtr.Zero
                );

                _stream.Start();
                _initialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[AudioService] PortAudio init failed: {ex.Message}");
                return false;
            }
        }

        private StreamCallbackResult AudioCallback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref StreamCallbackTimeInfo timeInfo,
            StreamCallbackFlags statusFlags,
            IntPtr userData)
        {
            if (_activeSynthesizer == null)
            {
                unsafe
                {
                    var outputPtr = (float*)output;
                    for (int i = 0; i < frameCount * 2; i++)
                        outputPtr[i] = 0f;
                }
                return StreamCallbackResult.Continue;
            }

            lock (_activeSynthesizer)
            {
                _activeSynthesizer.Render(_renderBufferLeft, _renderBufferRight);
            }

            // Interleave left/right channels
            unsafe
            {
                var outputPtr = (float*)output;
                for (int i = 0; i < frameCount; i++)
                {
                    outputPtr[i * 2] = _renderBufferLeft[i];
                    outputPtr[i * 2 + 1] = _renderBufferRight[i];
                }
            }

            return StreamCallbackResult.Continue;
        }

        private Synthesizer? GetOrLoadSynthesizer(string soundFontPath)
        {
            if (_synthesizerCache.TryGetValue(soundFontPath, out var cached))
                return cached;

            try
            {
                var synth = new Synthesizer(soundFontPath, SampleRate);

                _synthesizerCache[soundFontPath] = synth;
                return synth;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[AudioService] Failed to load SoundFont '{soundFontPath}': {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_stream != null)
            {
                _stream.Stop();
                _stream.Close();
                _stream.Dispose();
            }

            if (_initialized)
            {
                PortAudio.Terminate();
            }

            GC.SuppressFinalize(this);
        }
    }
}
