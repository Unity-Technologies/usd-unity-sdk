using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using USD.NET.Unity;
using Unity.Formats.USD;
using pxr;
namespace Unity.Formats.USD {

    public interface IImportProcessUsd
    {
        void ProcessUsd(PrimMap primMap);
    }
    public interface IImportProcessPrimMap
    {
        void ProcessPrimMap(PrimMap primMap);
    }

    public interface IImportProcessGeometry
    {
        void ProcessGeometry(PrimMap primMap);
    }
}