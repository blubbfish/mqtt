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

#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
using System;
#else
using Microsoft.SPOT;
#endif

namespace uPLibrary.Networking.M2Mqtt.Messages {
  /// <summary>
  /// Event Args class for subscribe request on topics
  /// </summary>
  public class MqttMsgSubscribeEventArgs : EventArgs {
    #region Properties...

    /// <summary>
    /// Message identifier
    /// </summary>
    public UInt16 MessageId { get; internal set; }

    /// <summary>
    /// Topics requested to subscribe
    /// </summary>
    public String[] Topics { get; internal set; }

    /// <summary>
    /// List of QOS Levels requested
    /// </summary>
    public Byte[] QoSLevels { get; internal set; }

    #endregion

    // message identifier

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="messageId">Message identifier for subscribe topics request</param>
    /// <param name="topics">Topics requested to subscribe</param>
    /// <param name="qosLevels">List of QOS Levels requested</param>
    public MqttMsgSubscribeEventArgs(UInt16 messageId, String[] topics, Byte[] qosLevels) {
      this.MessageId = messageId;
      this.Topics = topics;
      this.QoSLevels = qosLevels;
    }
  }
}
