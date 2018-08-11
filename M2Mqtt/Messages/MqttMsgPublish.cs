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
using System.Text;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages {
  /// <summary>
  /// Class for PUBLISH message from client to broker
  /// </summary>
  public class MqttMsgPublish : MqttMsgBase {
    #region Properties...

    /// <summary>
    /// Message topic
    /// </summary>
    public String Topic { get; set; }

    /// <summary>
    /// Message data
    /// </summary>
    public Byte[] Message { get; set; }

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgPublish() {
      this.type = MQTT_MSG_PUBLISH_TYPE;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="topic">Message topic</param>
    /// <param name="message">Message data</param>
    public MqttMsgPublish(String topic, Byte[] message) : this(topic, message, false, QOS_LEVEL_AT_MOST_ONCE, false) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="topic">Message topic</param>
    /// <param name="message">Message data</param>
    /// <param name="dupFlag">Duplicate flag</param>
    /// <param name="qosLevel">Quality of Service level</param>
    /// <param name="retain">Retain flag</param>
    public MqttMsgPublish(String topic, Byte[] message, Boolean dupFlag, Byte qosLevel, Boolean retain) : base() {
      this.type = MQTT_MSG_PUBLISH_TYPE;
      this.Topic = topic;
      this.Message = message;
      this.dupFlag = dupFlag;
      this.qosLevel = qosLevel;
      this.retain = retain;
      this.messageId = 0;
    }

    public override Byte[] GetBytes(Byte protocolVersion) {
      Int32 fixedHeaderSize = 0;
      Int32 varHeaderSize = 0;
      Int32 payloadSize = 0;
      Int32 remainingLength = 0;
      Byte[] buffer;
      Int32 index = 0;

      // topic can't contain wildcards
      if ((this.Topic.IndexOf('#') != -1) || (this.Topic.IndexOf('+') != -1)) {
        throw new MqttClientException(MqttClientErrorCode.TopicWildcard);
      }

      // check topic length
      if ((this.Topic.Length < MIN_TOPIC_LENGTH) || (this.Topic.Length > MAX_TOPIC_LENGTH)) {
        throw new MqttClientException(MqttClientErrorCode.TopicLength);
      }

      // check wrong QoS level (both bits can't be set 1)
      if (this.qosLevel > QOS_LEVEL_EXACTLY_ONCE) {
        throw new MqttClientException(MqttClientErrorCode.QosNotAllowed);
      }

      Byte[] topicUtf8 = Encoding.UTF8.GetBytes(this.Topic);

      // topic name
      varHeaderSize += topicUtf8.Length + 2;

      // message id is valid only with QOS level 1 or QOS level 2
      if ((this.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) || (this.qosLevel == QOS_LEVEL_EXACTLY_ONCE)) {
        varHeaderSize += MESSAGE_ID_SIZE;
      }

      // check on message with zero length
      if (this.Message != null) {
        // message data
        payloadSize += this.Message.Length;
      }

      remainingLength += (varHeaderSize + payloadSize);

      // first byte of fixed header
      fixedHeaderSize = 1;

      Int32 temp = remainingLength;
      // increase fixed header size based on remaining length
      // (each remaining length byte can encode until 128)
      do {
        fixedHeaderSize++;
        temp = temp / 128;
      } while (temp > 0);

      // allocate buffer for message
      buffer = new Byte[fixedHeaderSize + varHeaderSize + payloadSize];

      // first fixed header byte
      buffer[index] = (Byte)((MQTT_MSG_PUBLISH_TYPE << MSG_TYPE_OFFSET) | (this.qosLevel << QOS_LEVEL_OFFSET));
      buffer[index] |= this.dupFlag ? (Byte)(1 << DUP_FLAG_OFFSET) : (Byte)0x00;
      buffer[index] |= this.retain ? (Byte)(1 << RETAIN_FLAG_OFFSET) : (Byte)0x00;
      index++;

      // encode remaining length
      index = this.encodeRemainingLength(remainingLength, buffer, index);

      // topic name
      buffer[index++] = (Byte)((topicUtf8.Length >> 8) & 0x00FF); // MSB
      buffer[index++] = (Byte)(topicUtf8.Length & 0x00FF); // LSB
      Array.Copy(topicUtf8, 0, buffer, index, topicUtf8.Length);
      index += topicUtf8.Length;

      // message id is valid only with QOS level 1 or QOS level 2
      if ((this.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) ||
          (this.qosLevel == QOS_LEVEL_EXACTLY_ONCE)) {
        // check message identifier assigned
        if (this.messageId == 0) {
          throw new MqttClientException(MqttClientErrorCode.WrongMessageId);
        }

        buffer[index++] = (Byte)((this.messageId >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(this.messageId & 0x00FF); // LSB
      }

      // check on message with zero length
      if (this.Message != null) {
        // message data
        Array.Copy(this.Message, 0, buffer, index, this.Message.Length);
        index += this.Message.Length;
      }

      return buffer;
    }

    /// <summary>
    /// Parse bytes for a PUBLISH message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>PUBLISH message instance</returns>
    public static MqttMsgPublish Parse(Byte fixedHeaderFirstByte, Byte protocolVersion, IMqttNetworkChannel channel) {
      Byte[] buffer;
      Int32 index = 0;
      Byte[] topicUtf8;
      Int32 topicUtf8Length;
      MqttMsgPublish msg = new MqttMsgPublish();

      // get remaining length and allocate buffer
      Int32 remainingLength = decodeRemainingLength(channel);
      buffer = new Byte[remainingLength];

      // read bytes from socket...
      Int32 received = channel.Receive(buffer);

      // topic name
      topicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
      topicUtf8Length |= buffer[index++];
      topicUtf8 = new Byte[topicUtf8Length];
      Array.Copy(buffer, index, topicUtf8, 0, topicUtf8Length);
      index += topicUtf8Length;
      msg.Topic = new String(Encoding.UTF8.GetChars(topicUtf8));

      // read QoS level from fixed header
      msg.qosLevel = (Byte)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
      // check wrong QoS level (both bits can't be set 1)
      if (msg.qosLevel > QOS_LEVEL_EXACTLY_ONCE) {
        throw new MqttClientException(MqttClientErrorCode.QosNotAllowed);
      }
      // read DUP flag from fixed header
      msg.dupFlag = (((fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET) == 0x01);
      // read retain flag from fixed header
      msg.retain = (((fixedHeaderFirstByte & RETAIN_FLAG_MASK) >> RETAIN_FLAG_OFFSET) == 0x01);

      // message id is valid only with QOS level 1 or QOS level 2
      if ((msg.qosLevel == QOS_LEVEL_AT_LEAST_ONCE) ||
          (msg.qosLevel == QOS_LEVEL_EXACTLY_ONCE)) {
        // message id
        msg.messageId = (UInt16)((buffer[index++] << 8) & 0xFF00);
        msg.messageId |= (buffer[index++]);
      }

      // get payload with message data
      Int32 messageSize = remainingLength - index;
      Int32 remaining = messageSize;
      Int32 messageOffset = 0;
      msg.Message = new Byte[messageSize];

      // BUG FIX 26/07/2013 : receiving large payload

      // copy first part of payload data received
      Array.Copy(buffer, index, msg.Message, messageOffset, received - index);
      remaining -= (received - index);
      messageOffset += (received - index);

      // if payload isn't finished
      while (remaining > 0) {
        // receive other payload data
        received = channel.Receive(buffer);
        Array.Copy(buffer, 0, msg.Message, messageOffset, received);
        remaining -= received;
        messageOffset += received;
      }

      return msg;
    }

    public override String ToString() {
#if TRACE
      return this.GetTraceString(
          "PUBLISH",
          new Object[] { "messageId", "topic", "message" },
          new Object[] { this.messageId, this.Topic, this.Message });
#else
            return base.ToString();
#endif
    }
  }
}
