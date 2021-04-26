#if RECORDER_AVAILABLE
using System;
using System.Collections.Generic;
using pxr;
using Unity.Formats.USD;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [RecorderSettings(typeof(UsdRecorder), "Usd Clip", "usd_recorder")]
    public class UsdRecorderSettings : RecorderSettings
    {
        [SerializeField] UsdRecorderInputSettings inputSettings = new();
        [SerializeField] Format exportFormat;
        [SerializeField] UsdInterpolationType interpolationType = UsdInterpolationType.UsdInterpolationTypeLinear;
        [SerializeField] BasisTransformation basisTransformation = BasisTransformation.SlowAndSafe;
        [SerializeField] ActiveExportPolicy activePolicy = ActiveExportPolicy.ExportAsVisibility;
        [SerializeField] bool exportMaterials = true;
        [SerializeField] float scale = 1;
        public enum Format
        {
            Usd,
            Usda,
            UsdZ,
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

        public ActiveExportPolicy ActivePolicy
        {
            get => activePolicy;
            set => activePolicy = value;
        }

        public BasisTransformation BasisTransformation
        {
            get => basisTransformation;
            set => basisTransformation = value;
        }

        public Format ExportFormat => exportFormat;

        protected override string Extension
        {
            get
            {
                return exportFormat switch
                {
                    Format.Usd => "usd",
                    Format.Usda => "usda",
                    Format.UsdZ => "usdz",
                    _ => throw new ArgumentException("Unhandled format")
                };
            }
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return inputSettings; }
        }
    }
}

#endif