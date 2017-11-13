//
// Copyright 2016 Pixar
//
// Licensed under the Apache License, Version 2.0 (the "Apache License")
// with the following modification; you may not use this file except in
// compliance with the Apache License and the following modification to it:
// Section 6. Trademarks. is deleted and replaced with:
//
// 6. Trademarks. This License does not grant permission to use the trade
//    names, trademarks, service marks, or product names of the Licensor
//    and its affiliates, except as required to comply with Section 4(c) of
//    the License and to reproduce the content of the NOTICE file.
//
// You may obtain a copy of the Apache License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the Apache License with the above modification is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the Apache License for the specific
// language governing permissions and limitations under the Apache License.
//

/// Provides a container which may hold any type, and provides introspection
/// and iteration over array types.  See \a VtIsArray for more info.
///
/// \section VtValue_Casting Held-type Conversion with VtValue::Cast
///
/// VtValue provides a suite of "Cast" methods that convert or create a
/// VtValue holding a requested type (via template parameter, typeid, or
/// type-matching to another VtValue) from the type of the currently-held
/// value.  Clients can add conversions between their own types using the
/// RegisterCast(), RegisterSimpleCast(), and
/// RegisterSimpleBidirectionalCast() methods.  Conversions from plugins can
/// be guaranteed to be registered before they are needed by registering them
/// from within a
/// \code
/// TF_REGISTRY_FUNCTION(VtValue) {
/// }
/// \endcode
/// block.
///
/// \subsection VtValue_builtin_conversions Builtin Type Conversion
///
/// Conversions between most of the basic "value types" that are intrinsically
/// convertible are builtin, including all numeric types (including Gf's \c
/// half), std::string/TfToken, GfVec* (for vecs of the same dimension), and
/// VtArray<T> for floating-point POD and GfVec of the preceding.
///
/// \subsection VtValue_numeric_conversion Numeric Conversion Safety
///
/// The conversions between all scalar numeric types are performed with range
/// checks such as provided by boost::numeric_cast(), and will fail, returning
/// an empty VtValue if the source value is out of range of the destination
/// type.
///
/// Conversions between GfVec and other compound-numeric types provide no more
/// or less safety or checking than the conversion constructors of the types
/// themselves.  This includes VtArray, even VtArray<T> for T in scalar types
/// that are range-checked when held singly.
class VtValue
{
public:

  /// Default ctor gives empty VtValue.
  VtValue() {}

  /// Copy construct with \p other.
  VtValue(VtValue const &other) {
    // If other is local, can memcpy without derefing info ptrs.
    _info = other._info;
    if (other._IsLocalAndTriviallyCopyable()) {
      _storage = other._storage;
    }
    else if (_info) {
      _info->CopyInit(other._storage, _storage);
    }
  }

  /// Construct a VtValue holding a copy of \p obj.
  /// 
  /// If T is a char pointer or array, produce a VtValue holding a
  /// std::string. If T is boost::python::object, produce a VtValue holding
  /// a TfPyObjWrapper.
  template <class T>
  explicit VtValue(T const &obj) {
    _Init(obj);
  }

  /// Create a new VtValue, taking its contents from \p obj.
  /// 
  /// This is equivalent to creating a VtValue holding a value-initialized
  /// \p T instance, then invoking swap(<held-value>, obj), leaving obj in a
  /// default-constructed (value-initialized) state.  In the case that \p
  /// obj is expensive to copy, it may be significantly faster to use this
  /// idiom when \p obj need not retain its contents:
  ///
  /// \code
  /// MyExpensiveObject obj = CreateObject();
  /// return VtValue::Take(obj);
  /// \endcode
  ///
  /// Rather than:
  ///
  /// \code
  /// MyExpensiveObject obj = CreateObject();
  /// return VtValue(obj);
  /// \endcode
  template <class T>
  static VtValue Take(T &obj) {
    VtValue ret;
    ret.Swap(obj);
    return ret;
  }

  /// Destructor.
  ~VtValue() { _Clear(); }

  /// Swap this with \a rhs.
  VtValue &Swap(VtValue &rhs) {
    // Do nothing if both empty.  Otherwise general swap.
    if (not IsEmpty() or not rhs.IsEmpty()) {
      VtValue tmp;
      _Move(*this, tmp);
      _Move(rhs, *this);
      _Move(tmp, rhs);
    }
    return *this;
  }

  /// Overloaded swap() for generic code/stl/etc.
  friend void swap(VtValue &lhs, VtValue &rhs) { lhs.Swap(rhs); }

  /// Swap the held value with \a rhs.  If this value is holding a T,
  // make an unqualified call to swap(<held-value>, rhs).  If this value is
  // not holding a T, replace the held value with a value-initialized T
  // instance first, then swap.
  template <class T>
  typename boost::enable_if<
    boost::is_same<T, typename Vt_ValueGetStored<T>::Type> >::type
    Swap(T &rhs) {
    if (not IsHolding<T>())
      *this = T();
    UncheckedSwap(rhs);
  }

  /// Swap the held value with \a rhs.  This VtValue must be holding an
  /// object of type \p T.  If it does not, this invokes undefined behavior.
  /// Use Swap() if this VtValue is not known to contain an object of type
  /// \p T.
  template <class T>
  typename boost::enable_if<
    boost::is_same<T, typename Vt_ValueGetStored<T>::Type> >::type
    UncheckedSwap(T &rhs) {
    using std::swap;
    swap(_GetMutable<T>(), rhs);
  }

  /// \overload
  void UncheckedSwap(VtValue &rhs) { Swap(rhs); }

  /// Make this value empty and return the held \p T instance.  If
  /// this value does not hold a \p T instance, make this value empty and
  /// return a default-constructed \p T.
  template <class T>
  T Remove() {
    T result;
    Swap(result);
    _Clear();
    return result;
  }

  /// Make this value empty and return the held \p T instance.  If this
  /// value does not hold a \p T instance, this method invokes undefined
  /// behavior.
  template <class T>
  T UncheckedRemove() {
    T result;
    UncheckedSwap(result);
    _Clear();
    return result;
  }

  /// Return true if this value is holding an object of type \p T, false
  /// otherwise.
  template <class T>
  bool IsHolding() const {
    return _info and _TypeIs<T>();
  }

  /// Returns true iff this is holding an array type (see VtIsArray<>).
  VT_API bool IsArrayValued() const;

  /// Return the number of elements in the held value if IsArrayValued(),
  /// return 0 otherwise.
  VT_API size_t GetArraySize() const { return _GetNumElements(); }

  /// Returns the typeid of the type held by this value.
  VT_API std::type_info const &GetTypeid() const;

  /// Return the typeid of elements in a array valued type.  If not
  /// holding an array valued type, return typeid(void).
  VT_API std::type_info const &GetElementTypeid() const;

  /// Returns the TfType of the type held by this value.
  VT_API TfType GetType() const;

  /// Return the type name of the held typeid.
  VT_API std::string GetTypeName() const;

  /// Returns a const reference to the held object if the held object
  /// is of type \a T.  Invokes undefined behavior otherwise.  This is the
  /// fastest \a Get() method to use after a successful \a IsHolding() check.
  template <class T>
  T const &UncheckedGet() const { return _Get<T>(); }

  /// Returns a const reference to the held object if the held object
  /// is of type \a T.  Issues an error and returns a const reference to a
  /// default value if the held object is not of type \a T.  Use \a IsHolding
  /// to verify correct type before calling this function.  The default value
  /// returned in case of type mismatch is constructed using
  /// Vt_DefaultValueFactory<T>.  That may be specialized for client types.
  /// The default implementation of the default value factory produces a
  /// value-initialized T.
  template <class T>
  T const &Get() const {
    typedef Vt_DefaultValueFactory<T> Factory;

    // In the unlikely case that the types don't match, we obtain a default
    // value to return and issue an error via _FailGet.
    if (ARCH_UNLIKELY(not IsHolding<T>())) {
      return *(static_cast<T const *>(
        _FailGet(Factory::Invoke, typeid(T))));
    }

    return _Get<T>();
  }

  /// Return a copy of the held object if the held object is of type T.
  /// Return a copy of the default value \a def otherwise.  Note that this
  /// always returns a copy, as opposed to \a Get() which always returns a
  /// reference.
  template <class T>
  T GetWithDefault(T const &def = T()) const {
    return IsHolding<T>() ? UncheckedGet<T>() : def;
  }

  /// Register a cast from VtValue holding From to VtValue holding To.
  template <typename From, typename To>
  static void RegisterCast(VtValue(*castFn)(VtValue const &)) {
    _RegisterCast(typeid(From), typeid(To), castFn);
  }

  /// Register a simple cast from VtValue holding From to VtValue
  // holding To.
  template <typename From, typename To>
  static void RegisterSimpleCast() {
    _RegisterCast(typeid(From), typeid(To), _SimpleCast<From, To>);
  }

  /// Register a two-way cast from VtValue holding From to VtValue
  /// holding To.
  template <typename From, typename To>
  static void RegisterSimpleBidirectionalCast() {
    RegisterSimpleCast<From, To>();
    RegisterSimpleCast<To, From>();
  }

  /// Return a VtValue holding \c val cast to hold T.  Return empty VtValue
  /// if cast fails.
  ///
  /// This Cast() function is safe to call in multiple threads as it does
  /// not mutate the operant \p val.
  ///
  /// \sa \ref VtValue_Casting
  template <typename T>
  static VtValue Cast(VtValue const &val) {
    VtValue ret = val;
    return ret.Cast<T>();
  }

  /// Return a VtValue holding \c val cast to same type that \c other is
  /// holding.  Return empty VtValue if cast fails.
  ///
  /// This Cast() function is safe to call in multiple threads as it does not
  /// mutate the operant \p val.
  ///
  /// \sa \ref VtValue_Casting
  VT_API static VtValue CastToTypeOf(VtValue const &val, VtValue const &other);

  /// Return a VtValue holding \a val cast to \a type.  Return empty VtValue
  /// if cast fails.
  ///
  /// This Cast() function is safe to call in multiple threads as it does not
  /// mutate the operant \p val.
  ///
  /// \sa \ref VtValue_Casting
  VT_API static VtValue CastToTypeid(VtValue const &val, std::type_info const &type);

  /// Return if a value of type \a from can be cast to type \a to.
  ///
  /// \sa \ref VtValue_Casting
  static bool CanCastFromTypeidToTypeid(std::type_info const &from,
    std::type_info const &to) {
    return _CanCast(from, to);
  }

  /// Return \c this holding value type cast to T.  This value is left
  /// empty if the cast fails.
  ///
  /// \note Since this method mutates this value, it is not safe to invoke on
  /// the same VtValue in multiple threads simultaneously.
  ///
  /// \sa \ref VtValue_Casting
  template <typename T>
  VtValue &Cast() {
    if (IsHolding<T>())
      return *this;
    return *this = _PerformCast(typeid(T), *this);
  }

  /// Return \c this holding value type cast to same type that
  /// \c other is holding.  This value is left empty if the cast fails.
  ///
  /// \note Since this method mutates this value, it is not safe to invoke on
  /// the same VtValue in multiple threads simultaneously.
  ///
  /// \sa \ref VtValue_Casting
  //VtValue &CastToTypeOf(VtValue const &other) {
  //    return *this = _PerformCast(other.GetTypeid(), *this);
  //}

  /// Return \c this holding value type cast to \a type.  This value is
  /// left empty if the cast fails.
  ///
  /// \note Since this method mutates this value, it is not safe to invoke on
  /// the same VtValue in multiple threads simultaneously.
  ///
  /// \sa \ref VtValue_Casting
  //VtValue &CastToTypeid(std::type_info const &type) {
  //    return *this = _PerformCast(type, *this);
  //}

  /// Return if \c this can be cast to \a T.
  ///
  /// \sa \ref VtValue_Casting
  template <typename T>
  bool CanCast() const {
    return _CanCast(GetTypeid(), typeid(T));
  }

  /// Return if \c this can be cast to \a type.
  ///
  /// \sa \ref VtValue_Casting
  bool CanCastToTypeOf(VtValue const &other) const {
    return _CanCast(GetTypeid(), other.GetTypeid());
  }

  /// Return if \c this can be cast to \a type.
  ///
  /// \sa \ref VtValue_Casting
  //bool CanCastToTypeid(std::type_info const &type) const {
  //    return _CanCast(GetTypeid(), type);
  //}

  /// Returns true iff this value is empty.
  bool IsEmpty() const { return not _info; }

  /// Return a hash code for the held object by calling VtHashValue() on it.
  VT_API size_t GetHash() const;

  friend inline size_t hash_value(VtValue const &val) {
    return val.GetHash();
  }


private:

};