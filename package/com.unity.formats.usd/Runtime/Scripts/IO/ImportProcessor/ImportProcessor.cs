using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using USD.NET.Unity;
using Unity.Formats.USD;
using pxr;
namespace Unity.Formats.USD {

    // happens while hierarchy is loading
    public interface IImportProcessUsd
    {
        void ProcessUsd(Scene scene);
    }

    // happens after GameObject Hierarchy created
    public interface IImportPostProcessHierarchy
    {
        void PostProcessHierarchy(PrimMap primMap);
    }

    // happens after GameObject Hierarchy created and Geometry created
    public interface IImportPostProcessGeometry
    {
        void PostProcessGeometry(PrimMap primMap);
    }
}