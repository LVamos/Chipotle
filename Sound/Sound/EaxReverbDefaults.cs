using OpenTK;
using OpenTK.Audio.OpenAL;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;
using EfxEaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EfxEaxReverb;
using Presets = OpenTK.Audio.OpenAL.EffectsExtension.ReverbPresets;
//using Luky;

namespace Sound
{
    public static class EaxReverbDefaults
    {
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

        public static bool ToBool(this int num) => num != 0;

        public static void SaveEaxPreset(string name, EfxEaxReverb preset, string folder)
        {
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, name + " preset.txt");
            using (StreamWriter sw = new StreamWriter(file))
            {
                void Save(bool expression, string parameterName, object value) //local function
                {
                    if (expression)
                    {
                        sw.WriteLine($"{parameterName} {value.ToString()}");
                    }
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

        public static IEnumerable<(string Name, EaxReverb Preset)> GetEaxPresets()
            => typeof(Presets).GetFields().Select(r => (r.Name, (EaxReverb)r.GetValue(null)));

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

        // Here are the standard reverb defaults
        public const float Density = 1f;
        public const float MinDensity = 0f;
        public const float MaxDensity = 1f;

        public const float Diffusion = 1f;
        public const float MinDiffusion = 0f;
        public const float MaxDiffusion = 1f;

        public const float Gain = .32f;
        public const float MinGain = 0f;
        public const float MaxGain = 1f;

        public const float GainHF = .89f;
        public const float MinGainHF = 0f;
        public const float MaxGainHF = 1f;

        public const float DecayTime = 1.49f;
        public const float MinDecayTime = .1f;
        public const float MaxDecayTime = 20f;

        public const float DecayHFRatio = .83f;
        public const float MinDecayHFRatio = .1f;
        public const float MaxDecayHFRatio = 2f;

        public const float ReflectionsGain = .05f;
        public const float MinReflectionsGain = 0f;
        public const float MaxReflectionsGain = 3.1599f;

        public const float ReflectionsDelay = .007f;
        public const float MinReflectionsDelay = 0f;
        public const float MaxReflectionsDelay = .2999f;

        public const float LateReverbGain = 1.26f;
        public const float MinLateReverbGain = 0f;
        public const float MaxLateReverbGain = 10f;

        public const float LateReverbDelay = .011f;
        public const float MinLateReverbDelay = 0f;
        public const float MaxLateReverbDelay = .0999f;

        public const float AirAbsorptionGainHF = .994f;
        public const float MinAirAbsorptionGainHF = .892f;
        public const float MaxAirAbsorptionGainHF = 1f;

        public const float RoomRolloffFactor = 0f;
        public const float MinRoomRolloffFactor = 0f;
        public const float MaxRoomRolloffFactor = 10f;

        public const bool DecayHFLimit = true;

        // Here are the EAX specific settings
        public const float DecayLFRatio = 1f;
        public const float MinDecayLFRatio = .1f;
        public const float MaxDecayLFRatio = 2f;

        public const float EchoDepth = 0f;
        public const float MinEchoDepth = 0f;
        public const float MaxEchoDepth = 1f;

        public const float EchoTime = .25f;
        public const float MinEchoTime = .075f;
        public const float MaxEchoTime = .25f;

        public const float GainLF = 1f;
        public const float MinGainLF = 0f;
        public const float MaxGainLF = 1f;

        public const float HFReference = 5000;
        public const float MinHFReference = 1000f;
        public const float MaxHFReference = 20000f;

        public const float LFReference = 250;
        public const float MinLFReference = 20f;
        public const float MaxLFReference = 1000f;

        public const float ModulationDepth = 0f;
        public const float MinModulationDepth = 0f;
        public const float MaxModulationDepth = 1f;

        public const float ModulationTime = .25f;
        public const float MinModulationTime = .0401f;
        public const float MaxModulationTime = 4f;

        public static readonly Vector3 ReflectionsPan = Vector3.Zero;
        public static readonly Vector3 LateReverbPan = Vector3.Zero; // Magnitude between 0 and 1

    }


}
