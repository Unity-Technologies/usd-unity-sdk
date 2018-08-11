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


%define IGNORE_OPERATORS( CTYPE )

%ignore CTYPE::data();
%ignore CTYPE::operator[](size_t);
%ignore CTYPE::operator[](size_t) const;
%ignore CTYPE::operator-() const;
%ignore operator-(CTYPE const &, CTYPE const &);
%ignore operator+(CTYPE const &, CTYPE const &);
%ignore CTYPE::operator+=(CTYPE const &);
%ignore CTYPE::operator-=(CTYPE const &);
%ignore CTYPE::operator*(double) const;
%ignore CTYPE::operator*=(double);
%ignore CTYPE::operator*=(double) const;
%ignore operator*(double, CTYPE const &);
%ignore CTYPE::operator*(CTYPE const &) const;
%ignore CTYPE::operator/(double) const;
%ignore CTYPE::operator/=(double);
%ignore CTYPE::operator/=(double) const;
%ignore operator<<(std::ostream &, CTYPE const &);

// Interval
%ignore CTYPE::operator<;
%ignore CTYPE::operator&=;
%ignore CTYPE::operator|=;
%ignore CTYPE::operator*=;
%ignore CTYPE::operator>;
%ignore CTYPE::operator<=;
%ignore CTYPE::operator>=;
%ignore CTYPE::operator|;
%ignore CTYPE::operator&;
%ignore CTYPE::operator+;
%ignore CTYPE::operator*;
%ignore CTYPE::operator-;

// Range
%ignore CTYPE::operator/;

// Matrix
%ignore CTYPE::operator[];
%ignore CTYPE::operator+=;
%ignore CTYPE::operator-=;
%ignore CTYPE::operator/;

%enddef // IGNORE_OPERATORS




%define WRAP_EQUAL( CTYPE )

%ignore operator==;
%ignore operator!=;

%extend CTYPE {
  static bool Equals(CTYPE const& lhs, CTYPE const& rhs) {
    return lhs == rhs;
  }

  %csmethodmodifiers GetHashCode() "override public";
  int GetHashCode() {
    return (int)TfHash()(self);
  }

  %proxycode %{
    public static bool operator==($typemap(cstype, CTYPE) lhs, $typemap(cstype, CTYPE) rhs){
      // The Swig binding glue will re-enter this operator comparing to null, so 
      // that case must be handled explicitly to avoid an infinite loop. This is still
      // not great, since it crosses the C#/C++ barrier twice. A better approache might
      // be to return a simple value from C++ that can be compared in C#.
      bool lnull = lhs as object == null;
      bool rnull = rhs as object == null;
      return (lnull == rnull) && ((lnull && rnull) || $typemap(cstype, CTYPE).Equals(lhs, rhs));
    }

    public static bool operator !=($typemap(cstype, CTYPE) lhs, $typemap(cstype, CTYPE) rhs) {
        return !(lhs == rhs);
    }

    override public bool Equals(object rhs) {
      return $typemap(cstype, CTYPE).Equals(this, rhs as $typemap(cstype, CTYPE));
    }
  %}

}
%enddef // WRAP_EQUAL
