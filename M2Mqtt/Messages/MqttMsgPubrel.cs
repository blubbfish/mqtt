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

using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages {
  /// <summary>
  /// Class for PUBREL message from client top broker
  /// </summary>
  public class MqttMsgPubrel : MqttMsgBase {
    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgPubrel() {
      this.Type = MQTT_MSG_PUBREL_TYPE;
      // PUBREL message use QoS Level 1 (not "officially" in 3.1.1)
      this.QosLevel = QOS_LEVEL_AT_LEAST_ONCE;
    }

    public override System.Byte[] GetBytes(System.Byte protocolVersion) {
      System.Int32 varHeaderSize = 0;
      System.Int32 payloadSize = 0;
      System.Int32 remainingLength = 0;
      System.Byte[] buffer;
      System.Int32 index = 0;

      // message identifier
      varHeaderSize += MESSAGE_ID_SIZE;

      remainingLength += varHeaderSize + payloadSize;

      // first byte of fixed header
      System.Int32 fixedHeaderSize = 1;

      System.Int32 temp = remainingLength;
      // increase fixed header size based on remaining length
      // (each remaining length byte can encode until 128)
      do {
        fixedHeaderSize++;
        temp /= 128;
      } while (temp > 0);

      // allocate buffer for message
      buffer = new System.Byte[fixedHeaderSize + varHeaderSize + payloadSize];

      // first fixed header byte
      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        buffer[index++] = (MQTT_MSG_PUBREL_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_PUBREL_FLAG_BITS; // [v.3.1.1]
      } else {
        buffer[index] = (System.Byte)((MQTT_MSG_PUBREL_TYPE << MSG_TYPE_OFFSET) |
                           (this.QosLevel << QOS_LEVEL_OFFSET));
        buffer[index] |= this.DupFlag ? (System.Byte)(1 << DUP_FLAG_OFFSET) : (System.Byte)0x00;
        index++;
      }

      // encode remaining length
      index = this.EncodeRemainingLength(remainingLength, buffer, index);

      // get next message identifier
      buffer[index++] = (System.Byte)((this.MessageId >> 8) & 0x00FF); // MSB
      buffer[index++] = (System.Byte)(this.MessageId & 0x00FF); // LSB 

      return buffer;
    }

    /// <summary>
    /// Parse bytes for a PUBREL message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>PUBREL message instance</returns>
    public static MqttMsgPubrel Parse(System.Byte fixedHeaderFirstByte, System.Byte protocolVersion, IMqttNetworkChannel channel) {
      System.Byte[] buffer;
      System.Int32 index = 0;
      MqttMsgPubrel msg = new MqttMsgPubrel();

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        // [v3.1.1] check flag bits
        if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_PUBREL_FLAG_BITS) {
          throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
        }
      }

      // get remaining length and allocate buffer
      System.Int32 remainingLength = MqttMsgBase.DecodeRemainingLength(channel);
      buffer = new System.Byte[remainingLength];

      // read bytes from socket...
      _ = channel.Receive(buffer);

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1) {
        // only 3.1.0

        // read QoS level from fixed header (would be QoS Level 1)
        msg.QosLevel = (System.Byte)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
        // read DUP flag from fixed header
        msg.DupFlag = (fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET == 0x01;
      }

      // message id
      msg.MessageId = (System.UInt16)((buffer[index++] << 8) & 0xFF00);
      msg.MessageId |= buffer[index++];

      return msg;
    }

    public override System.String ToString() =>
#if TRACE
      this.GetTraceString(
          "PUBREL",
          new System.Object[] { "messageId" },
          new System.Object[] { this.MessageId });
#else
            base.ToString();
#endif

  }
}
