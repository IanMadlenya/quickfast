// Copyright (c) 2009, Object Computing, Inc.
// All rights reserved.
// See the file license.txt for licensing information.
#pragma once
#using <System.dll>

namespace QuickFASTDotNet{
  /// @brief access to System.Diagnostics.Stopwatch
  ///
  /// For some reason I wasn't able to fathom, I couldn't
  /// access System.Diagnostics.Stopwatch directly from C#.
  /// This wrapper class is a hack to fix that.  Feel free
  /// to fix it the right way if you know how. [and let me know!  Dale]
  public ref class Stopwatch
  {
  public:
    Stopwatch();
    void Stop();

    property unsigned __int64 ElapsedMilliseconds
    {
      unsigned __int64 get();
    }

  private:
    System::Diagnostics::Stopwatch ^ stopwatch_;
  };
}
