using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USD.NET;
using pxr;
namespace Unity.Formats.USD {

    // happens while hierarchy is loading
    public interface IImportProcessUsd
    {
        void ProcessUsd(Scene scene, SceneImportOptions sceneImportOptions);
    }

    // happens after GameObject Hierarchy created
    public interface IImportPostProcessHierarchy
    {
        void PostProcessHierarchy(PrimMap primMap, SceneImportOptions sceneImportOptions);
    }

    // happens after GameObject Hierarchy created and Geometry created
    public interface IImportPostProcessGeometry
    {
        void PostProcessGeometry(PrimMap primMap, SceneImportOptions sceneImportOptions);
    }
}