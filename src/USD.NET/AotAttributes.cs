using System;


/// <summary>
/// Prevent a method, class, field, or property from being stripped by bytecode optimization.
/// </summary>
/// <remarks>
/// When IL2CPP optimizes the generated IL, unused code will be stripped. Any methods only called by reflection will
/// also be stripped in this way. To prevent this, this attribute is copied from Unity.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
class PreserveAttribute : Attribute
{
}
