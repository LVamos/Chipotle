using OpenTK;
using OpenTK.Audio.OpenAL;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;
using EfxEaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EfxEaxReverb;
using Presets = OpenTK.Audio.OpenAL.EffectsExtension.ReverbPresets;

namespace Sound
{
    /// <summary>
    /// Default settings of EAX reverb effect
    /// </summary>
    public static class EaxReverbDefaults
    {
        /// <summary>
        /// Returns default settings of the EAX reverb effect.
        /// </summary>
        /// <returns>default settings of the EAX reverb effect</returns>
        public static EfxEaxReverb GetDefaultSetting()
        {
            EfxEaxReverb _ = new EfxEaxReverb();
            _.AirAbsorptionGainHF = AirAbsorptionGainHF;
            _.DecayHFLimit = DecayHFLimit ? 1 : 0;
            _.DecayHFRatio = DecayHFRatio;
            _.DecayLFRatio = DecayLFRatio;
            _.DecayTime = DecayTime;
            _.Density = Density;
            _.Diffusion = Diffusion;
            _.EchoDepth = EchoDepth;
            _.EchoTime = EchoTime;
            _.Gain = Gain;
            _.GainHF = GainHF;
            _.GainLF = GainLF;
            _.HFReference = HFReference;
            _.LateReverbDelay = LateReverbDelay;
            _.LateReverbGain = LateReverbGain;
            _.LateReverbPan = LateReverbPan;
            _.LFReference = LFReference;
            _.ModulationDepth = ModulationDepth;
            _.ModulationTime = ModulationTime;
            _.ReflectionsDelay = ReflectionsDelay;
            _.ReflectionsGain = ReflectionsGain;
            _.ReflectionsPan = ReflectionsPan;
            _.RoomRolloffFactor = RoomRolloffFactor;

            return _;
        }

        /// <summary>
        /// Converts an int to bool.
        /// </summary>
        /// <param name="num">An integer to be converted</param>
        /// <returns>A boolean value</returns>
        public static bool ToBool(this int num) => num != 0;

        /// <summary>
        /// Exports an EAX reverb preset to a text file.
        /// </summary>
        /// <param name="name">Name of the preset</param>
        /// <param name="preset">The preset</param>
        /// <param name="folder">Path to a folder where the file should be saved</param>
        public static void SaveEaxPreset(string name, EfxEaxReverb preset, string folder)
        {
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, name + " preset.txt");
            using (StreamWriter sw = new StreamWriter(file))
            {
                void Save(bool expression, string parameterName, object value) //local function
                {
                    if (expression)
                        sw.WriteLine($"{parameterName} {value.ToString()}");
                }

                // Here are the 13 standard reverb settings
                Save(preset.Density != EaxReverbDefaults.Density, "density", preset.Density);
                Save(preset.Diffusion != EaxReverbDefaults.Diffusion, "diffusion", preset.Diffusion);
                Save(preset.Gain != EaxReverbDefaults.Gain, "gain", preset.Gain);
                Save(preset.GainHF != EaxReverbDefaults.GainHF, "gain_hf", preset.GainHF);
                Save(preset.DecayTime != EaxReverbDefaults.DecayTime, "decay_time", preset.DecayTime);
                Save(preset.DecayHFRatio != EaxReverbDefaults.DecayHFRatio, "decay_hf_ratio", preset.DecayHFRatio);
                Save(preset.ReflectionsGain != EaxReverbDefaults.ReflectionsGain, "reflections_gain", preset.ReflectionsGain);
                Save(preset.ReflectionsDelay != EaxReverbDefaults.ReflectionsDelay, "reflections_delay", preset.ReflectionsDelay);
                Save(preset.LateReverbGain != EaxReverbDefaults.LateReverbGain, "late_reverb_gain", preset.LateReverbGain);
                Save(preset.LateReverbDelay != EaxReverbDefaults.LateReverbDelay, "late_reverb_delay", preset.LateReverbDelay);
                Save(preset.AirAbsorptionGainHF != EaxReverbDefaults.AirAbsorptionGainHF, "air_absorption_gain_hf", preset.AirAbsorptionGainHF);
                Save(preset.RoomRolloffFactor != EaxReverbDefaults.RoomRolloffFactor, "room_rolloff_factor", preset.RoomRolloffFactor);
                Save(preset.DecayHFLimit.ToBool() != EaxReverbDefaults.DecayHFLimit, "decay_hf_limit", preset.DecayHFLimit.ToBool());

                // Above are all the standard reverb settings, now to do the Eax specific ones.
                Save(preset.DecayLFRatio != EaxReverbDefaults.DecayLFRatio, "decay_lf_ratio", preset.DecayLFRatio);
                Save(preset.EchoDepth != EaxReverbDefaults.EchoDepth, "echo_depth", preset.EchoDepth);
                Save(preset.EchoTime != EaxReverbDefaults.EchoTime, "echo_time", preset.EchoTime);
                Save(preset.GainLF != EaxReverbDefaults.GainLF, "gain_lf", preset.GainLF);
                Save(preset.HFReference != EaxReverbDefaults.HFReference, "hf_reference", preset.HFReference);
                Save(preset.LFReference != EaxReverbDefaults.LFReference, "lf_reference", preset.LFReference);
                Save(preset.ModulationDepth != EaxReverbDefaults.ModulationDepth, "modulation_depth", preset.ModulationDepth);
                Save(preset.ModulationTime != EaxReverbDefaults.ModulationTime, "modulation_time", preset.ModulationTime);
                Save(preset.ReflectionsPan != ReflectionsPan, "//reflections_pan", preset.ReflectionsPan);
                Save(preset.LateReverbPan != LateReverbPan, "//late_reverb_pan", preset.LateReverbPan);

            }
        }

        /// <summary>
        /// Enumerates all available EAX reverb presets.
        /// </summary>
        /// <returns>Enumeration of all presets</returns>
        public static IEnumerable<(string Name, EaxReverb Preset)> GetEaxPresets()
            => typeof(Presets).GetFields().Select(r => (r.Name, (EaxReverb)r.GetValue(null)));

        /// <summary>
        /// Saves all EAX reverb presets to text files.
        /// </summary>
        public static void SaveAllEaxPresets()
        {
            foreach ((string Name, EaxReverb Preset) preset in GetEaxPresets())
            {
                EaxReverb eaxReverb = preset.Preset;
                EfxEaxReverb efxReverb;
                EffectsExtension.GetEaxFromEfxEax(ref eaxReverb, out efxReverb);
                SaveEaxPreset(preset.Name, efxReverb, @"Data\TestReverb\");
            }

        }

        /// <summary>
        /// Default value of the Density parameter
        /// </summary>
        public const float Density = 1f;

        /// <summary>
        /// min value of the Density parameter
        /// </summary>
        public const float MinDensity = 0f;

        /// <summary>
        /// max value of the Density parameter
        /// </summary>
        public const float MaxDensity = 1f;

        /// <summary>
        /// Default value of the diffusion parameter
        /// </summary>
        public const float Diffusion = 1f;


        /// <summary>
        /// min value of the Diffusion parameter
        /// </summary>
        public const float MinDiffusion = 0f;


        /// <summary>
        /// Max value of the Diffusion parameter
        /// </summary>
        public const float MaxDiffusion = 1f;

        /// <summary>
        /// Default value of the Gain parameter
        /// </summary>
        public const float Gain = .32f;

        /// <summary>
        ///Min value of the Gain parameter
        /// </summary>
        public const float MinGain = 0f;

        /// <summary>
        ///Max value of the Gain parameter
        /// </summary>
        public const float MaxGain = 1f;

        /// <summary>
        ///Default value of the GainHF parameter
        /// </summary>
        public const float GainHF = .89f;

        /// <summary>
        ///Min value of the GainHF parameter
        /// </summary>
        public const float MinGainHF = 0f;

        /// <summary>
        ///Max value of the GainHF parameter
        /// </summary>
        public const float MaxGainHF = 1f;

        /// <summary>
        ///Default value of the DecayTime parameter
        /// </summary>
        public const float DecayTime = 1.49f;
        public const float MinDecayTime = .1f;

        /// <summary>
        ///Max value of the DecayTime parameter
        /// </summary>
        public const float MaxDecayTime = 20f;

        /// <summary>
        ///Default value of the DecayHFRatio parameter
        /// </summary>
        public const float DecayHFRatio = .83f;

        /// <summary>
        ///Min value of the DecayHFRatio parameter
        /// </summary>
        public const float MinDecayHFRatio = .1f;

        /// <summary>
        ///Max value of the DecayHFRatio parameter
        /// </summary>
        public const float MaxDecayHFRatio = 2f;

        /// <summary>
        ///Default value of the ReflectionsGain parameter
        /// </summary>
        public const float ReflectionsGain = .05f;

        /// <summary>
        ///Min value of the ReflectionsGain parameter
        /// </summary>
        public const float MinReflectionsGain = 0f;

        /// <summary>
        ///Max value of the ReflectionsGain parameter
        /// </summary>
        public const float MaxReflectionsGain = 3.1599f;

        /// <summary>
        ///Default value of the ReflectionsDelay parameter
        /// </summary>
        public const float ReflectionsDelay = .007f;

        /// <summary>
        ///Min value of the ReflectionsDelay parameter
        /// </summary>
        public const float MinReflectionsDelay = 0f;

        /// <summary>
        ///Max value of the ReflectionsDelay parameter
        /// </summary>
        public const float MaxReflectionsDelay = .2999f;

        /// <summary>
        ///Default value of the LateReverbGain parameter
        /// </summary>
        public const float LateReverbGain = 1.26f;

        /// <summary>
        ///Min value of the LateReverbGain parameter
        /// </summary>
        public const float MinLateReverbGain = 0f;

        /// <summary>
        ///Max value of the LateReverbGain parameter
        /// </summary>
        public const float MaxLateReverbGain = 10f;

        /// <summary>
        ///Default value of the LateReverbDelay parameter
        /// </summary>
        public const float LateReverbDelay = .011f;

        /// <summary>
        ///Min value of the LateReverbDelay parameter
        /// </summary>
        public const float MinLateReverbDelay = 0f;

        /// <summary>
        ///Max value of the LateReverbDelay parameter
        /// </summary>
        public const float MaxLateReverbDelay = .0999f;

        /// <summary>
        ///Default value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float AirAbsorptionGainHF = .994f;

        /// <summary>
        ///Min value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float MinAirAbsorptionGainHF = .892f;

        /// <summary>
        ///Max value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float MaxAirAbsorptionGainHF = 1f;

        /// <summary>
        ///Default value of the RoomRolloffFactor parameter
        /// </summary>
        public const float RoomRolloffFactor = 0f;

        /// <summary>
        ///Min value of the RoomRolloffFactor parameter
        /// </summary>
        public const float MinRoomRolloffFactor = 0f;

        /// <summary>
        ///Max value of the RoomRolloffFactor parameter
        /// </summary>
        public const float MaxRoomRolloffFactor = 10f;

        /// <summary>
        ///Default value of the DecayHFLimit parameter
        /// </summary>
        public const bool DecayHFLimit = true;

        // Here are the EAX specific settings
        public const float DecayLFRatio = 1f;

        /// <summary>
        ///Min value of the DecayHFLimit parameter
        /// </summary>
        public const float MinDecayLFRatio = .1f;

        /// <summary>
        ///Max value of the DecayHFLimit parameter
        /// </summary>
        public const float MaxDecayLFRatio = 2f;

        /// <summary>
        ///Default value of the EchoDepth parameter
        /// </summary>
        public const float EchoDepth = 0f;

        /// <summary>
        ///Min value of the EchoDepth parameter
        /// </summary>
        public const float MinEchoDepth = 0f;

        /// <summary>
        ///Max value of the EchoDepth parameter
        /// </summary>
        public const float MaxEchoDepth = 1f;

        /// <summary>
        ///Default value of the EchoTime parameter
        /// </summary>
        public const float EchoTime = .25f;

        /// <summary>
        ///Min value of the EchoTime parameter
        /// </summary>
        public const float MinEchoTime = .075f;

        /// <summary>
        ///Max value of the EchoTime parameter
        /// </summary>
        public const float MaxEchoTime = .25f;

        /// <summary>
        ///Default value of the GainLF parameter
        /// </summary>
        public const float GainLF = 1f;

        /// <summary>
        ///Min value of the GainLF parameter
        /// </summary>
        public const float MinGainLF = 0f;

        /// <summary>
        ///Max value of the GainLF parameter
        /// </summary>
        public const float MaxGainLF = 1f;

        /// <summary>
        ///Default value of the HFReference parameter
        /// </summary>
        public const float HFReference = 5000;

        /// <summary>
        ///Min value of the HFReference parameter
        /// </summary>
        public const float MinHFReference = 1000f;

        /// <summary>
        ///Max value of the HFReference parameter
        /// </summary>
        public const float MaxHFReference = 20000f;

        /// <summary>
        ///Default value of the LFReference parameter
        /// </summary>
        public const float LFReference = 250;

        /// <summary>
        ///Min value of the LFReference parameter
        /// </summary>
        public const float MinLFReference = 20f;

        /// <summary>
        ///Max value of the LFReference parameter
        /// </summary>
        public const float MaxLFReference = 1000f;

        /// <summary>
        ///Default value of the ModulationDepth parameter
        /// </summary>
        public const float ModulationDepth = 0f;

        /// <summary>
        ///Min value of the ModulationDepth parameter
        /// </summary>
        public const float MinModulationDepth = 0f;

        /// <summary>
        ///Max value of the ModulationDepth parameter
        /// </summary>
        public const float MaxModulationDepth = 1f;

        /// <summary>
        ///Default value of the ModulationTime parameter
        /// </summary>
        public const float ModulationTime = .25f;

        /// <summary>
        ///Min value of the ModulationTime parameter
        /// </summary>
        public const float MinModulationTime = .0401f;

        /// <summary>
        ///Max value of the ModulationTime parameter
        /// </summary>
        public const float MaxModulationTime = 4f;

        /// <summary>
        ///Default value of the ReflectionsPan parameter
        /// </summary>
        public static readonly Vector3 ReflectionsPan = Vector3.Zero;

        /// <summary>
        ///Default value of the LateReverbPan parameter
        /// </summary>
        public static readonly Vector3 LateReverbPan = Vector3.Zero; // Magnitude between 0 and 1

    }


}
