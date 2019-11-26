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
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages {
  /// <summary>
  /// Class for DISCONNECT message from client to broker
  /// </summary>
  public class MqttMsgDisconnect : MqttMsgBase {
    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgDisconnect() => this.Type = MQTT_MSG_DISCONNECT_TYPE;

    /// <summary>
    /// Parse bytes for a DISCONNECT message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>DISCONNECT message instance</returns>
    public static MqttMsgDisconnect Parse(Byte fixedHeaderFirstByte, Byte protocolVersion, IMqttNetworkChannel channel) {
      MqttMsgDisconnect msg = new MqttMsgDisconnect();

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        // [v3.1.1] check flag bits
        if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_DISCONNECT_FLAG_BITS) {
          throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
        }
      }

      // get remaining length and allocate buffer
      _ = DecodeRemainingLength(channel);
      // NOTE : remainingLength must be 0

      return msg;
    }

    public override Byte[] GetBytes(Byte protocolVersion) {
      Byte[] buffer = new Byte[2];
      Int32 index = 0;

      // first fixed header byte
      buffer[index++] = protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1
        ? (Byte)((MQTT_MSG_DISCONNECT_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_DISCONNECT_FLAG_BITS)
        : (Byte)(MQTT_MSG_DISCONNECT_TYPE << MSG_TYPE_OFFSET);

      buffer[index++] = 0x00;

      return buffer;
    }

    public override String ToString() =>
#if TRACE
      this.GetTraceString(
          "DISCONNECT",
          null,
          null);
#else
            base.ToString();
#endif

  }
}
