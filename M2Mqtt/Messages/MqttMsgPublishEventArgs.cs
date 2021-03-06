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
  /// Event Args class for PUBLISH message received from broker
  /// </summary>
  public class MqttMsgPublishEventArgs : EventArgs {
    #region Properties...

    /// <summary>
    /// Message topic
    /// </summary>
    public String Topic { get; internal set; }

    /// <summary>
    /// Message data
    /// </summary>
    public Byte[] Message { get; internal set; }

    /// <summary>
    /// Duplicate message flag
    /// </summary>
    public Boolean DupFlag { get; set; }

    /// <summary>
    /// Quality of Service level
    /// </summary>
    public Byte QosLevel { get; internal set; }

    /// <summary>
    /// Retain message flag
    /// </summary>
    public Boolean Retain { get; internal set; }

    #endregion

    // message topic

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="topic">Message topic</param>
    /// <param name="message">Message data</param>
    /// <param name="dupFlag">Duplicate delivery flag</param>
    /// <param name="qosLevel">Quality of Service level</param>
    /// <param name="retain">Retain flag</param>
    public MqttMsgPublishEventArgs(String topic,
        Byte[] message,
        Boolean dupFlag,
        Byte qosLevel,
        Boolean retain) {
      this.Topic = topic;
      this.Message = message;
      this.DupFlag = dupFlag;
      this.QosLevel = qosLevel;
      this.Retain = retain;
    }
  }
}
