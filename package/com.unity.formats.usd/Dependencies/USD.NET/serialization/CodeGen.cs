// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Reflection;
using System.Reflection.Emit;


#if NET_4_6
static internal class CodeGen
{
    private static int m_funcCount = 0;

    // XXX : All Emit methods must have an iOS version which does not actually emit executable code,
    //       since the OS does not allow allocation of new code paths at runtime. Or this code must
    //       be emitted at build time.

    public static Delegate EmitToCs<T>(MethodInfo toVtArray, MethodInfo toCsArray)
    {
        // Generates a function of the form:
        //   object ToCs(VtValue val, object vtArray) {
        //     toVtArray(value, ref vtArray);
        //     return (object)(toCsArray(vtArray));
        //   }

        // define the signature of the dynamic method
        var fn = new DynamicMethod(
            "ToCs" + m_funcCount++,
            typeof(object),
            new Type[] { typeof(pxr.VtValue), typeof(object) /*vtArray*/ },
            typeof(CodeGen).Module
        );

        var il = fn.GetILGenerator();

        // Push the VtValue onto the stack as the first argument.
        il.Emit(OpCodes.Ldarg_0); // VtValue
        il.Emit(OpCodes.Ldarg_1); // VtArray

        // Call the converter: toVtArray(vtValue, ref vtArray);
        il.Emit(OpCodes.Call, toVtArray);

        // Push the vtArray again
        il.Emit(OpCodes.Ldarg_1);

        // vtArrayToCsArray( vtArray );
        il.Emit(OpCodes.Call, toCsArray);

        // Convert the return value from the specific C# type to "object"
        il.Emit(OpCodes.Castclass, typeof(object));

        // Return from the method pushing a return value from the converter onto the caller's stack.
        il.Emit(OpCodes.Ret);

        // build a delegate from the dynamic method
        return fn.CreateDelegate(typeof(T));
    }

    public static Delegate EmitToVt<T>(MethodInfo converter, Type csType, Type vtArrayType)
    {
        // Generates a function of the form:
        //   VtValue ToVt(object CSharpNativeArray) {
        //     VtArray<T> = converter( (StrongC#Type)CSharpNativeArray) );
        //     return VtValue(vtArray<T>);
        //   }

        // define the signature of the dynamic method
        var fn = new DynamicMethod(
            "ToVt" + m_funcCount++,
            typeof(pxr.VtValue),
            new Type[] { typeof(object) },
            typeof(CodeGen).Module
        );

        var il = fn.GetILGenerator();

        // Push the C# object (array) onto the stack as the first argument.
        il.Emit(OpCodes.Ldarg_0);

        // Convert the object-typed argument to the specific VtArray type the converter wants.
        il.Emit(OpCodes.Castclass, csType);

        // Call the converter.
        il.Emit(OpCodes.Call, converter);

        // Convert the return value to a VtValue via construction.
        var valueCtor = typeof(pxr.VtValue).GetConstructor(new Type[] { vtArrayType });
        il.Emit(OpCodes.Newobj, valueCtor);

        // Return the newly constructed VtValue to the caller.
        il.Emit(OpCodes.Ret);

        // build a delegate from the dynamic method
        return fn.CreateDelegate(typeof(T));
    }
}
#endif
