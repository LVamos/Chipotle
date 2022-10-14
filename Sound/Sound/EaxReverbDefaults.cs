using OpenTK;

using System.Collections.Generic;
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
        ///Default value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float AirAbsorptionGainHF = .994f;

        /// <summary>
        ///Default value of the DecayHFLimit parameter
        /// </summary>
        public const bool DecayHFLimit = true;

        /// <summary>
        ///Default value of the DecayHFRatio parameter
        /// </summary>
        public const float DecayHFRatio = .83f;

        // Here are the EAX specific settings
        public const float DecayLFRatio = 1f;

        /// <summary>
        ///Default value of the DecayTime parameter
        /// </summary>
        public const float DecayTime = 1.49f;

        /// <summary>
        /// Default value of the Density parameter
        /// </summary>
        public const float Density = 1f;

        /// <summary>
        /// Default value of the diffusion parameter
        /// </summary>
        public const float Diffusion = 1f;

        /// <summary>
        ///Default value of the EchoDepth parameter
        /// </summary>
        public const float EchoDepth = 0f;

        /// <summary>
        ///Default value of the EchoTime parameter
        /// </summary>
        public const float EchoTime = .25f;

        /// <summary>
        /// Default value of the Gain parameter
        /// </summary>
        public const float Gain = .32f;

        /// <summary>
        ///Default value of the GainHF parameter
        /// </summary>
        public const float GainHF = .89f;

        /// <summary>
        ///Default value of the GainLF parameter
        /// </summary>
        public const float GainLF = 1f;

        /// <summary>
        ///Default value of the HFReference parameter
        /// </summary>
        public const float HFReference = 5000;

        /// <summary>
        ///Default value of the LateReverbDelay parameter
        /// </summary>
        public const float LateReverbDelay = .011f;

        /// <summary>
        ///Default value of the LateReverbGain parameter
        /// </summary>
        public const float LateReverbGain = 1.26f;

        /// <summary>
        ///Default value of the LFReference parameter
        /// </summary>
        public const float LFReference = 250;

        /// <summary>
        ///Max value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float MaxAirAbsorptionGainHF = 1f;

        /// <summary>
        ///Max value of the DecayHFRatio parameter
        /// </summary>
        public const float MaxDecayHFRatio = 2f;

        /// <summary>
        ///Max value of the DecayHFLimit parameter
        /// </summary>
        public const float MaxDecayLFRatio = 2f;

        /// <summary>
        ///Max value of the DecayTime parameter
        /// </summary>
        public const float MaxDecayTime = 20f;

        /// <summary>
        /// max value of the Density parameter
        /// </summary>
        public const float MaxDensity = 1f;

        /// <summary>
        /// Max value of the Diffusion parameter
        /// </summary>
        public const float MaxDiffusion = 1f;

        /// <summary>
        ///Max value of the EchoDepth parameter
        /// </summary>
        public const float MaxEchoDepth = 1f;

        /// <summary>
        ///Max value of the EchoTime parameter
        /// </summary>
        public const float MaxEchoTime = .25f;

        /// <summary>
        ///Max value of the Gain parameter
        /// </summary>
        public const float MaxGain = 1f;

        /// <summary>
        ///Max value of the GainHF parameter
        /// </summary>
        public const float MaxGainHF = 1f;

        /// <summary>
        ///Max value of the GainLF parameter
        /// </summary>
        public const float MaxGainLF = 1f;

        /// <summary>
        ///Max value of the HFReference parameter
        /// </summary>
        public const float MaxHFReference = 20000f;

        /// <summary>
        ///Max value of the LateReverbDelay parameter
        /// </summary>
        public const float MaxLateReverbDelay = .0999f;

        /// <summary>
        ///Max value of the LateReverbGain parameter
        /// </summary>
        public const float MaxLateReverbGain = 10f;

        /// <summary>
        ///Max value of the LFReference parameter
        /// </summary>
        public const float MaxLFReference = 1000f;

        /// <summary>
        ///Max value of the ModulationDepth parameter
        /// </summary>
        public const float MaxModulationDepth = 1f;

        /// <summary>
        ///Max value of the ModulationTime parameter
        /// </summary>
        public const float MaxModulationTime = 4f;

        /// <summary>
        ///Max value of the ReflectionsDelay parameter
        /// </summary>
        public const float MaxReflectionsDelay = .2999f;

        /// <summary>
        ///Max value of the ReflectionsGain parameter
        /// </summary>
        public const float MaxReflectionsGain = 3.1599f;

        /// <summary>
        ///Max value of the RoomRolloffFactor parameter
        /// </summary>
        public const float MaxRoomRolloffFactor = 10f;

        /// <summary>
        ///Min value of the AirAbsorptionGainHF parameter
        /// </summary>
        public const float MinAirAbsorptionGainHF = .892f;

        /// <summary>
        ///Min value of the DecayHFRatio parameter
        /// </summary>
        public const float MinDecayHFRatio = .1f;

        /// <summary>
        ///Min value of the DecayHFLimit parameter
        /// </summary>
        public const float MinDecayLFRatio = .1f;

        public const float MinDecayTime = .1f;

        /// <summary>
        /// min value of the Density parameter
        /// </summary>
        public const float MinDensity = 0f;

        /// <summary>
        /// min value of the Diffusion parameter
        /// </summary>
        public const float MinDiffusion = 0f;

        /// <summary>
        ///Min value of the EchoDepth parameter
        /// </summary>
        public const float MinEchoDepth = 0f;

        /// <summary>
        ///Min value of the EchoTime parameter
        /// </summary>
        public const float MinEchoTime = .075f;

        /// <summary>
        ///Min value of the Gain parameter
        /// </summary>
        public const float MinGain = 0f;

        /// <summary>
        ///Min value of the GainHF parameter
        /// </summary>
        public const float MinGainHF = 0f;

        /// <summary>
        ///Min value of the GainLF parameter
        /// </summary>
        public const float MinGainLF = 0f;

        /// <summary>
        ///Min value of the HFReference parameter
        /// </summary>
        public const float MinHFReference = 1000f;

        /// <summary>
        ///Min value of the LateReverbDelay parameter
        /// </summary>
        public const float MinLateReverbDelay = 0f;

        /// <summary>
        ///Min value of the LateReverbGain parameter
        /// </summary>
        public const float MinLateReverbGain = 0f;

        /// <summary>
        ///Min value of the LFReference parameter
        /// </summary>
        public const float MinLFReference = 20f;

        /// <summary>
        ///Min value of the ModulationDepth parameter
        /// </summary>
        public const float MinModulationDepth = 0f;

        /// <summary>
        ///Min value of the ModulationTime parameter
        /// </summary>
        public const float MinModulationTime = .0401f;

        /// <summary>
        ///Min value of the ReflectionsDelay parameter
        /// </summary>
        public const float MinReflectionsDelay = 0f;

        /// <summary>
        ///Min value of the ReflectionsGain parameter
        /// </summary>
        public const float MinReflectionsGain = 0f;

        /// <summary>
        ///Min value of the RoomRolloffFactor parameter
        /// </summary>
        public const float MinRoomRolloffFactor = 0f;

        /// <summary>
        ///Default value of the ModulationDepth parameter
        /// </summary>
        public const float ModulationDepth = 0f;

        /// <summary>
        ///Default value of the ModulationTime parameter
        /// </summary>
        public const float ModulationTime = .25f;

        /// <summary>
        ///Default value of the ReflectionsDelay parameter
        /// </summary>
        public const float ReflectionsDelay = .007f;

        /// <summary>
        ///Default value of the ReflectionsGain parameter
        /// </summary>
        public const float ReflectionsGain = .05f;

        /// <summary>
        ///Default value of the RoomRolloffFactor parameter
        /// </summary>
        public const float RoomRolloffFactor = 0f;

        /// <summary>
        ///Default value of the LateReverbPan parameter
        /// </summary>
        public static readonly Vector3 LateReverbPan = Vector3.Zero;

        /// <summary>
        ///Default value of the ReflectionsPan parameter
        /// </summary>
        public static readonly Vector3 ReflectionsPan = Vector3.Zero;

        /// <summary>
        /// Returns default settings of the EAX reverb effect.
        /// </summary>
        /// <returns>default settings of the EAX reverb effect</returns>
        public static EfxEaxReverb GetDefaultSetting()
        {
            EfxEaxReverb _ = new EfxEaxReverb
            {
                AirAbsorptionGainHF = AirAbsorptionGainHF,
                DecayHFLimit = DecayHFLimit ? 1 : 0,
                DecayHFRatio = DecayHFRatio,
                DecayLFRatio = DecayLFRatio,
                DecayTime = DecayTime,
                Density = Density,
                Diffusion = Diffusion,
                EchoDepth = EchoDepth,
                EchoTime = EchoTime,
                Gain = Gain,
                GainHF = GainHF,
                GainLF = GainLF,
                HFReference = HFReference,
                LateReverbDelay = LateReverbDelay,
                LateReverbGain = LateReverbGain,
                LateReverbPan = LateReverbPan,
                LFReference = LFReference,
                ModulationDepth = ModulationDepth,
                ModulationTime = ModulationTime,
                ReflectionsDelay = ReflectionsDelay,
                ReflectionsGain = ReflectionsGain,
                ReflectionsPan = ReflectionsPan,
                RoomRolloffFactor = RoomRolloffFactor
            };

            return _;
        }

        /// <summary>
        /// Enumerates all available EAX reverb presets.
        /// </summary>
        /// <returns>Enumeration of all presets</returns>
        public static IEnumerable<(string Name, EaxReverb Preset)> GetEaxPresets()
            => typeof(Presets).GetFields().Select(r => (r.Name, (EaxReverb)r.GetValue(null)));

        /// <summary>
        /// Converts an int to bool.
        /// </summary>
        /// <param name="num">An integer to be converted</param>
        /// <returns>A boolean value</returns>
        public static bool ToBool(this int num) => num != 0;

        // Magnitude between 0 and 1
    }
}