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
  /// Class for PUBCOMP message from broker to client
  /// </summary>
  public class MqttMsgPubcomp : MqttMsgBase {
    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgPubcomp() => this.Type = MQTT_MSG_PUBCOMP_TYPE;

    public override Byte[] GetBytes(Byte protocolVersion) {
      Int32 varHeaderSize = 0;
      Int32 payloadSize = 0;
      Int32 remainingLength = 0;
      Byte[] buffer;
      Int32 index = 0;

      // message identifier
      varHeaderSize += MESSAGE_ID_SIZE;

      remainingLength += varHeaderSize + payloadSize;

      // first byte of fixed header
      Int32 fixedHeaderSize = 1;

      Int32 temp = remainingLength;
      // increase fixed header size based on remaining length
      // (each remaining length byte can encode until 128)
      do {
        fixedHeaderSize++;
        temp /= 128;
      } while (temp > 0);

      // allocate buffer for message
      buffer = new Byte[fixedHeaderSize + varHeaderSize + payloadSize];

      // first fixed header byte
      buffer[index++] = protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1
        ? (Byte)((MQTT_MSG_PUBCOMP_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_PUBCOMP_FLAG_BITS)
        : (Byte)(MQTT_MSG_PUBCOMP_TYPE << MSG_TYPE_OFFSET);

      // encode remaining length
      index = this.EncodeRemainingLength(remainingLength, buffer, index);

      // get message identifier
      buffer[index++] = (Byte)((this.MessageId >> 8) & 0x00FF); // MSB
      buffer[index++] = (Byte)(this.MessageId & 0x00FF); // LSB 

      return buffer;
    }

    /// <summary>
    /// Parse bytes for a PUBCOMP message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>PUBCOMP message instance</returns>
    public static MqttMsgPubcomp Parse(Byte fixedHeaderFirstByte, Byte protocolVersion, IMqttNetworkChannel channel) {
      Byte[] buffer;
      Int32 index = 0;
      MqttMsgPubcomp msg = new MqttMsgPubcomp();

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        // [v3.1.1] check flag bits
        if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_PUBCOMP_FLAG_BITS) {
          throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
        }
      }

      // get remaining length and allocate buffer
      Int32 remainingLength = MqttMsgBase.DecodeRemainingLength(channel);
      buffer = new Byte[remainingLength];

      // read bytes from socket...
      _ = channel.Receive(buffer);

      // message id
      msg.MessageId = (UInt16)((buffer[index++] << 8) & 0xFF00);
      msg.MessageId |= buffer[index++];

      return msg;
    }

    public override String ToString() =>
#if TRACE
      this.GetTraceString(
          "PUBCOMP",
          new Object[] { "messageId" },
          new Object[] { this.MessageId });
#else
            base.ToString();
#endif

  }
}
