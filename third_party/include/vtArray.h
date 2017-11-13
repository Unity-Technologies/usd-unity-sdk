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


/// \class VtArray 
///
/// Represents an arbitrary dimensional rectangular container class.
///
/// Originally, VtArray was built to mimic the arrays in menv2x's MDL language,
/// but since VtArray has typed elements, the multidimensionality has found
/// little use.  For example, if you have only scalar elements, then to
/// represent a list of vectors you need an two dimensional array.  To represent
/// a list of matrices you need a three dimensional array.  However with
/// VtArray<GfVec3d> and VtArray<GfMatrix4d>, the VtArray is one dimensional,
/// the extra dimensions are encoded in the element types themselves.
///
/// For this reason, VtArray has been moving toward being more like std::vector,
/// and it now has much of std::vector's API, but there are still important
/// differences.
///
/// First, VtArray shares data between instances using a copy-on-write scheme.
/// This means that making copies of VtArray instances is cheap: it only copies
/// the pointer to the data.  But on the other hand, invoking any non-const
/// member function will incur a copy of the underlying data if it is not
/// uniquely owned.  For example, assume 'a' and 'b' are VtArray<int>:
///
/// \code
/// a = b;       // No copy; a and b now share ownership of underlying data.
/// a[0] = 123;  // A copy is incurred, to detach a's data from b.
///              // a and b no longer share data.
/// a[1] = 234;  // No copy: a's data is uniquely owned by a.
/// \endcode
///
/// Note that since all non-const member functions will potentially cause a
/// copy, it's possible to accidentally incur a copy even when unintended, or
/// when no actual data mutation occurs.  For example:
/// 
/// \code
/// int sum = 0;
/// for (VtArray<int>::iterator i = a.begin(), end = a.end(); i != end; ++i) {
///    sum += *i;
/// }
/// \endcode
///
/// Invoking a.begin() here will incur a copy if a's data is shared.  This is
/// required since it's possible to mutate the data through the returned
/// iterator, even though the subsequent code doesn't do any mutation.  This can
/// be avoided by explicitly const-iterating like the following:
///
/// \code
/// int sum = 0;
/// for (VtArray<int>::const_iterator i = a.cbegin(), end = a.cend(); i != end; ++i) {
///    sum += *i;
/// }
/// \endcode
///


template<typename ELEM>
class VtArray {
  public:

    /// Type this array holds.
    typedef ELEM ElementType;
    typedef ELEM value_type;

    /// Create a size=0 array.
    VtArray() {}

    /// Create a size=n array.
    explicit VtArray(unsigned int n);

    /// Return a pointer to this array's data.
    //ELEM* data() { _Detach(); return _data ? _data->vec.data() : NULL; }
    /// Return a const pointer to this array's data.
    //const_pointer data() const { return _data ? _data->vec.data() : NULL; }
    /// Return a const pointer to the data held by this array.
    //const_pointer cdata() const { return data(); }

    /// Append an element to array.
    void push_back(ElementType const &elem);

    /// Remove the last element of an array.
    void pop_back();

    /// Return the total number of elements in this array.
    size_t size() const { return _data ? _data->vec.size() : 0; }

    /// Equivalent to size() == 0.
    bool empty() const { return size() == 0; }
    
    /// Ensure enough memory is allocated to hold \p num elements.
    void reserve(size_t num);

    /// Return a reference to the first element in this array.  Invokes
    /// undefined behavior if the array is empty.
    //reference front() { return *begin(); }
    /// Return a const reference to the first element in this array.
    /// Invokes undefined behavior if the array is empty.
    //const_reference front() const { return *begin(); }

    /// Return a reference to the last element in this array.  Invokes
    /// undefined behavior if the array is empty.
    //reference back() { return *rbegin(); }
    /// Return a const reference to the last element in this array.
    /// Invokes undefined behavior if the array is empty.
    //const_reference back() const { return *rbegin(); }

    /// Resize this array.
    /// Preserves existing data that remains, value-initializes any newly added
    /// data.  For example, resize(10) on an array of size 5 would change the
    /// size to 10, the first 5 elements would be left unchanged and the last
    /// 5 elements would be value-initialized.
    void resize(size_t num);       

    /// Equivalent to resize(0).
    void clear();

    /// Assign array contents.
    /// Equivalent to:
    /// \code
    /// array.resize(std::distance(first, last));
    /// std::copy(first, last, array.begin());
    /// \endcode
    template <class ForwardIter>
    void assign(ForwardIter first, ForwardIter last);

    /// Assign array contents.
    /// Equivalent to:
    /// \code
    /// array.resize(n);
    /// std::fill(array.begin(), array.end(), fill);
    /// \endcode
    void assign(size_t n, const value_type &fill);

    /// Swap the contents of this array with \p other.
    void swap(VtArray &other);

    /// @}

    /// Tests if two arrays are identical, i.e. that they share
    /// the same underlying copy-on-write data.
    bool IsIdentical(VtArray const & other) const;

  %typemap(cscode) VtArray %{
	  public $typemap(cstype, ELEM) this[int index] {
		  get { return GetValue(index); }
		  set { SetValue(index, value); }
	  }
  %}
};