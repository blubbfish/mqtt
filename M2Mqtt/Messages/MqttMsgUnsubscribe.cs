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
// if NOT .Net Micro Framework
#if !MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3
using System.Collections.Generic;
#endif
using System.Collections;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages {
  /// <summary>
  /// Class for UNSUBSCRIBE message from client to broker
  /// </summary>
  public class MqttMsgUnsubscribe : MqttMsgBase {
    #region Properties...

    /// <summary>
    /// List of topics to unsubscribe
    /// </summary>
    public String[] Topics { get; set; }

    #endregion

    // topics to unsubscribe

    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgUnsubscribe() => this.Type = MQTT_MSG_UNSUBSCRIBE_TYPE;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="topics">List of topics to unsubscribe</param>
    public MqttMsgUnsubscribe(String[] topics) {
      this.Type = MQTT_MSG_UNSUBSCRIBE_TYPE;

      this.Topics = topics;

      // UNSUBSCRIBE message uses QoS Level 1 (not "officially" in 3.1.1)
      this.QosLevel = QOS_LEVEL_AT_LEAST_ONCE;
    }

    /// <summary>
    /// Parse bytes for a UNSUBSCRIBE message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>UNSUBSCRIBE message instance</returns>
    public static MqttMsgUnsubscribe Parse(Byte fixedHeaderFirstByte, Byte protocolVersion, IMqttNetworkChannel channel) {
      Byte[] buffer;
      Int32 index = 0;
      Byte[] topicUtf8;
      Int32 topicUtf8Length;
      MqttMsgUnsubscribe msg = new MqttMsgUnsubscribe();

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        // [v3.1.1] check flag bits
        if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_UNSUBSCRIBE_FLAG_BITS) {
          throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
        }
      }

      // get remaining length and allocate buffer
      Int32 remainingLength = DecodeRemainingLength(channel);
      buffer = new Byte[remainingLength];

      // read bytes from socket...
      Int32 received = channel.Receive(buffer);

      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1) {
        // only 3.1.0

        // read QoS level from fixed header
        msg.QosLevel = (Byte)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
        // read DUP flag from fixed header
        msg.DupFlag = (fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET == 0x01;
        // retain flag not used
        msg.Retain = false;
      }

      // message id
      msg.MessageId = (UInt16)((buffer[index++] << 8) & 0xFF00);
      msg.MessageId |= buffer[index++];

      // payload contains topics
      // NOTE : before, I don't know how many topics will be in the payload (so use List)

      // if .Net Micro Framework
#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
            IList tmpTopics = new ArrayList();
// else other frameworks (.Net, .Net Compact, Mono, Windows Phone) 
#else
      IList<String> tmpTopics = new List<String>();
#endif
      do {
        // topic name
        topicUtf8Length = (buffer[index++] << 8) & 0xFF00;
        topicUtf8Length |= buffer[index++];
        topicUtf8 = new Byte[topicUtf8Length];
        Array.Copy(buffer, index, topicUtf8, 0, topicUtf8Length);
        index += topicUtf8Length;
        tmpTopics.Add(new String(Encoding.UTF8.GetChars(topicUtf8)));
      } while (index < remainingLength);

      // copy from list to array
      msg.Topics = new String[tmpTopics.Count];
      for (Int32 i = 0; i < tmpTopics.Count; i++) {
        msg.Topics[i] = (String)tmpTopics[i];
      }

      return msg;
    }

    public override Byte[] GetBytes(Byte protocolVersion) {
      Int32 varHeaderSize = 0;
      Int32 payloadSize = 0;
      Int32 remainingLength = 0;
      Byte[] buffer;
      Int32 index = 0;

      // topics list empty
      if (this.Topics == null || this.Topics.Length == 0) {
        throw new MqttClientException(MqttClientErrorCode.TopicsEmpty);
      }

      // message identifier
      varHeaderSize += MESSAGE_ID_SIZE;
      Byte[][] topicsUtf8 = new Byte[this.Topics.Length][];


      Int32 topicIdx;
      for (topicIdx = 0; topicIdx < this.Topics.Length; topicIdx++) {
        // check topic length
        if (this.Topics[topicIdx].Length < MIN_TOPIC_LENGTH || this.Topics[topicIdx].Length > MAX_TOPIC_LENGTH) {
          throw new MqttClientException(MqttClientErrorCode.TopicLength);
        }

        topicsUtf8[topicIdx] = Encoding.UTF8.GetBytes(this.Topics[topicIdx]);
        payloadSize += 2; // topic size (MSB, LSB)
        payloadSize += topicsUtf8[topicIdx].Length;
      }

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
      if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1) {
        buffer[index++] = (MQTT_MSG_UNSUBSCRIBE_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_UNSUBSCRIBE_FLAG_BITS; // [v.3.1.1]
      } else {
        buffer[index] = (Byte)((MQTT_MSG_UNSUBSCRIBE_TYPE << MSG_TYPE_OFFSET) |
                           (this.QosLevel << QOS_LEVEL_OFFSET));
        buffer[index] |= this.DupFlag ? (Byte)(1 << DUP_FLAG_OFFSET) : (Byte)0x00;
        index++;
      }

      // encode remaining length
      index = this.EncodeRemainingLength(remainingLength, buffer, index);

      // check message identifier assigned
      if (this.MessageId == 0) {
        throw new MqttClientException(MqttClientErrorCode.WrongMessageId);
      }

      buffer[index++] = (Byte)((this.MessageId >> 8) & 0x00FF); // MSB
      buffer[index++] = (Byte)(this.MessageId & 0x00FF); // LSB 

      for (topicIdx = 0; topicIdx < this.Topics.Length; topicIdx++) {
        // topic name
        buffer[index++] = (Byte)((topicsUtf8[topicIdx].Length >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(topicsUtf8[topicIdx].Length & 0x00FF); // LSB
        Array.Copy(topicsUtf8[topicIdx], 0, buffer, index, topicsUtf8[topicIdx].Length);
        index += topicsUtf8[topicIdx].Length;
      }

      return buffer;
    }

    public override String ToString() =>
#if TRACE
      this.GetTraceString(
          "UNSUBSCRIBE",
          new Object[] { "messageId", "topics" },
          new Object[] { this.MessageId, this.Topics });
#else
          base.ToString();
#endif

  }
}
