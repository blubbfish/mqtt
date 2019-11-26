/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
*/

using System;
using System.Diagnostics;

namespace uPLibrary.Networking.M2Mqtt.Utility {
  /// <summary>
  /// Tracing levels
  /// </summary>
  public enum TraceLevel {
    Error = 0x01,
    Warning = 0x02,
    Information = 0x04,
    Verbose = 0x0F,
    Frame = 0x10,
    Queuing = 0x20
  }

  // delegate for writing trace
  public delegate void WriteTrace(String format, params Object[] args);

  /// <summary>
  /// Tracing class
  /// </summary>
  public static class Trace {
    public static TraceLevel TraceLevel;
    public static WriteTrace TraceListener;

    [Conditional("DEBUG")]
    public static void Debug(String format, params Object[] args) => TraceListener?.Invoke(format, args);

    public static void WriteLine(TraceLevel level, String format) {
      if ((level & TraceLevel) > 0) {
        TraceListener.Invoke(format);
      }
    }

    public static void WriteLine(TraceLevel level, String format, Object arg1) {
      if ( (level & TraceLevel) > 0) {
        TraceListener.Invoke(format, arg1);
      }
    }

    public static void WriteLine(TraceLevel level, String format, Object arg1, Object arg2) {
      if ((level & TraceLevel) > 0) {
        TraceListener.Invoke(format, arg1, arg2);
      }
    }

    public static void WriteLine(TraceLevel level, String format, Object arg1, Object arg2, Object arg3) {
      if ((level & TraceLevel) > 0) {
        TraceListener.Invoke(format, arg1, arg2, arg3);
      }
    }
  }
}