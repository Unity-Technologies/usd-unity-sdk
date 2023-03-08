#if RECORDER_AVAILABLE
using System;
using System.Collections.Generic;
using pxr;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [RecorderSettings(typeof(UsdRecorder), "USD Clip", "usd_recorder")]
    public class UsdRecorderSettings : RecorderSettings
    {
        [SerializeField] UsdRecorderInputSettings inputSettings = new UsdRecorderInputSettings();
        [SerializeField] Format exportFormat;
        [SerializeField] UsdInterpolationType interpolationType = UsdInterpolationType.UsdInterpolationTypeLinear;
        [SerializeField] BasisTransformation coordinateConversion = BasisTransformation.SlowAndSafe;
        [SerializeField] ActiveExportPolicy activePolicy = ActiveExportPolicy.ExportAsVisibility;
        [SerializeField] bool exportMaterials = true;
        [SerializeField] float scale = 1;
        [SerializeField] ExportOverridesSetting exportTransformOverrides;

        public UsdRecorderSettings()
        {
            FileNameGenerator.FileName = DefaultWildcard.Recorder + "_" + DefaultWildcard.Take;
        }

        public enum Format
        {
            USD,
            USDA,
            USDZ,
        }

        public enum ExportOverridesSetting
        {
            ExportInFull,
            ExportTransformOverridesOnly
        }

        public UsdInterpolationType InterpolationType
        {
            get => interpolationType;
            set => interpolationType = value;
        }

        public float Scale
        {
            get => scale;
            set => scale = value;
        }

        public bool ExportMaterials
        {
            get => exportMaterials;
            set => exportMaterials = value;
        }

        public bool ExportTransformOverrides
        {
            get => exportTransformOverrides == ExportOverridesSetting.ExportTransformOverridesOnly ? true : false;
            set => exportTransformOverrides = value ? ExportOverridesSetting.ExportTransformOverridesOnly : ExportOverridesSetting.ExportInFull;
        }

        public ActiveExportPolicy ActivePolicy
        {
            get => activePolicy;
            set => activePolicy = value;
        }

        public BasisTransformation BasisTransformation
        {
            get => coordinateConversion;
            set => coordinateConversion = value;
        }

        public Format ExportFormat => exportFormat;

        protected override string Extension
        {
            get
            {
                switch (exportFormat)
                {
                    case Format.USD:
                        return "usd";
                    case Format.USDA:
                        return "usda";
                    case Format.USDZ:
                        return "usdz";
                    default:
                        throw new ArgumentException("Unhandled format");
                }
            }
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return inputSettings; }
        }
    }
}

#endif
