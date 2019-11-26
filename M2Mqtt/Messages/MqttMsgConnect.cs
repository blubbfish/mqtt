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
  /// Class for CONNECT message from client to broker
  /// </summary>
  public class MqttMsgConnect : MqttMsgBase {
    #region Constants...

    // protocol name supported
    internal const String PROTOCOL_NAME_V3_1 = "MQIsdp";
    internal const String PROTOCOL_NAME_V3_1_1 = "MQTT"; // [v.3.1.1]

    // max length for client id (removed in 3.1.1)
    internal const Int32 CLIENT_ID_MAX_LENGTH = 23;

    // variable header fields
    internal const Byte PROTOCOL_NAME_LEN_SIZE = 2;
    internal const Byte PROTOCOL_NAME_V3_1_SIZE = 6;
    internal const Byte PROTOCOL_NAME_V3_1_1_SIZE = 4; // [v.3.1.1]
    internal const Byte PROTOCOL_VERSION_SIZE = 1;
    internal const Byte CONNECT_FLAGS_SIZE = 1;
    internal const Byte KEEP_ALIVE_TIME_SIZE = 2;

    internal const Byte PROTOCOL_VERSION_V3_1 = 0x03;
    internal const Byte PROTOCOL_VERSION_V3_1_1 = 0x04; // [v.3.1.1]
    internal const UInt16 KEEP_ALIVE_PERIOD_DEFAULT = 60; // seconds
    internal const UInt16 MAX_KEEP_ALIVE = 65535; // 16 bit

    // connect flags
    internal const Byte USERNAME_FLAG_MASK = 0x80;
    internal const Byte USERNAME_FLAG_OFFSET = 0x07;
    internal const Byte USERNAME_FLAG_SIZE = 0x01;
    internal const Byte PASSWORD_FLAG_MASK = 0x40;
    internal const Byte PASSWORD_FLAG_OFFSET = 0x06;
    internal const Byte PASSWORD_FLAG_SIZE = 0x01;
    internal const Byte WILL_RETAIN_FLAG_MASK = 0x20;
    internal const Byte WILL_RETAIN_FLAG_OFFSET = 0x05;
    internal const Byte WILL_RETAIN_FLAG_SIZE = 0x01;
    internal const Byte WILL_QOS_FLAG_MASK = 0x18;
    internal const Byte WILL_QOS_FLAG_OFFSET = 0x03;
    internal const Byte WILL_QOS_FLAG_SIZE = 0x02;
    internal const Byte WILL_FLAG_MASK = 0x04;
    internal const Byte WILL_FLAG_OFFSET = 0x02;
    internal const Byte WILL_FLAG_SIZE = 0x01;
    internal const Byte CLEAN_SESSION_FLAG_MASK = 0x02;
    internal const Byte CLEAN_SESSION_FLAG_OFFSET = 0x01;
    internal const Byte CLEAN_SESSION_FLAG_SIZE = 0x01;
    // [v.3.1.1] lsb (reserved) must be now 0
    internal const Byte RESERVED_FLAG_MASK = 0x01;
    internal const Byte RESERVED_FLAG_OFFSET = 0x00;
    internal const Byte RESERVED_FLAG_SIZE = 0x01;

    #endregion

    #region Properties...

    /// <summary>
    /// Protocol name
    /// </summary>
    public String ProtocolName { get; set; }

    /// <summary>
    /// Protocol version
    /// </summary>
    public Byte ProtocolVersion { get; set; }

    /// <summary>
    /// Client identifier
    /// </summary>
    public String ClientId { get; set; }

    /// <summary>
    /// Will retain flag
    /// </summary>
    public Boolean WillRetain { get; set; }

    /// <summary>
    /// Will QOS level
    /// </summary>
    public Byte WillQosLevel { get; set; }

    /// <summary>
    /// Will flag
    /// </summary>
    public Boolean WillFlag { get; set; }

    /// <summary>
    /// Will topic
    /// </summary>
    public String WillTopic { get; set; }

    /// <summary>
    /// Will message
    /// </summary>
    public String WillMessage { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public String Username { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public String Password { get; set; }

    /// <summary>
    /// Clean session flag
    /// </summary>
    public Boolean CleanSession { get; set; }

    /// <summary>
    /// Keep alive period
    /// </summary>
    public UInt16 KeepAlivePeriod { get; set; }

    #endregion

    // protocol name

    // will retain flag
    // will quality of service level

    /// <summary>
    /// Constructor
    /// </summary>
    public MqttMsgConnect() => this.Type = MQTT_MSG_CONNECT_TYPE;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    public MqttMsgConnect(String clientId) :
        this(clientId, null, null, false, QOS_LEVEL_AT_LEAST_ONCE, false, null, null, true, KEEP_ALIVE_PERIOD_DEFAULT, PROTOCOL_VERSION_V3_1_1) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="willRetain">Will retain flag</param>
    /// <param name="willQosLevel">Will QOS level</param>
    /// <param name="willFlag">Will flag</param>
    /// <param name="willTopic">Will topic</param>
    /// <param name="willMessage">Will message</param>
    /// <param name="cleanSession">Clean sessione flag</param>
    /// <param name="keepAlivePeriod">Keep alive period</param>
    /// <param name="protocolVersion">Protocol version</param>
    public MqttMsgConnect(String clientId,
        String username,
        String password,
        Boolean willRetain,
        Byte willQosLevel,
        Boolean willFlag,
        String willTopic,
        String willMessage,
        Boolean cleanSession,
        UInt16 keepAlivePeriod,
        Byte protocolVersion
        ) {
      this.Type = MQTT_MSG_CONNECT_TYPE;

      this.ClientId = clientId;
      this.Username = username;
      this.Password = password;
      this.WillRetain = willRetain;
      this.WillQosLevel = willQosLevel;
      this.WillFlag = willFlag;
      this.WillTopic = willTopic;
      this.WillMessage = willMessage;
      this.CleanSession = cleanSession;
      this.KeepAlivePeriod = keepAlivePeriod;
      // [v.3.1.1] added new protocol name and version
      this.ProtocolVersion = protocolVersion;
      this.ProtocolName = (this.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) ? PROTOCOL_NAME_V3_1_1 : PROTOCOL_NAME_V3_1;
    }

    /// <summary>
    /// Parse bytes for a CONNECT message
    /// </summary>
    /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
    /// <param name="protocolVersion">Protocol Version</param>
    /// <param name="channel">Channel connected to the broker</param>
    /// <returns>CONNECT message instance</returns>
    public static MqttMsgConnect Parse(Byte fixedHeaderFirstByte, Byte protocolVersion, IMqttNetworkChannel channel) {
      Byte[] buffer;
      Int32 index = 0;
      Int32 protNameUtf8Length;
      Byte[] protNameUtf8;
      Boolean isUsernameFlag;
      Boolean isPasswordFlag;
      Int32 clientIdUtf8Length;
      Byte[] clientIdUtf8;
      Int32 willTopicUtf8Length;
      Byte[] willTopicUtf8;
      Int32 willMessageUtf8Length;
      Byte[] willMessageUtf8;
      Int32 usernameUtf8Length;
      Byte[] usernameUtf8;
      Int32 passwordUtf8Length;
      Byte[] passwordUtf8;
      MqttMsgConnect msg = new MqttMsgConnect();

      // get remaining length and allocate buffer
      Int32 remainingLength = MqttMsgBase.DecodeRemainingLength(channel);
      buffer = new Byte[remainingLength];

      // read bytes from socket...
      _ = channel.Receive(buffer);

      // protocol name
      protNameUtf8Length = (buffer[index++] << 8) & 0xFF00;
      protNameUtf8Length |= buffer[index++];
      protNameUtf8 = new Byte[protNameUtf8Length];
      Array.Copy(buffer, index, protNameUtf8, 0, protNameUtf8Length);
      index += protNameUtf8Length;
      msg.ProtocolName = new String(Encoding.UTF8.GetChars(protNameUtf8));

      // [v3.1.1] wrong protocol name
      if (!msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1) && !msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1_1)) {
        throw new MqttClientException(MqttClientErrorCode.InvalidProtocolName);
      }

      // protocol version
      msg.ProtocolVersion = buffer[index];
      index += PROTOCOL_VERSION_SIZE;

      // connect flags
      // [v3.1.1] check lsb (reserved) must be 0
      if (msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1 &&
          (buffer[index] & RESERVED_FLAG_MASK) != 0x00) {
        throw new MqttClientException(MqttClientErrorCode.InvalidConnectFlags);
      }

      isUsernameFlag = (buffer[index] & USERNAME_FLAG_MASK) != 0x00;
      isPasswordFlag = (buffer[index] & PASSWORD_FLAG_MASK) != 0x00;
      msg.WillRetain = (buffer[index] & WILL_RETAIN_FLAG_MASK) != 0x00;
      msg.WillQosLevel = (Byte)((buffer[index] & WILL_QOS_FLAG_MASK) >> WILL_QOS_FLAG_OFFSET);
      msg.WillFlag = (buffer[index] & WILL_FLAG_MASK) != 0x00;
      msg.CleanSession = (buffer[index] & CLEAN_SESSION_FLAG_MASK) != 0x00;
      index += CONNECT_FLAGS_SIZE;

      // keep alive timer
      msg.KeepAlivePeriod = (UInt16)((buffer[index++] << 8) & 0xFF00);
      msg.KeepAlivePeriod |= buffer[index++];

      // client identifier [v3.1.1] it may be zero bytes long (empty string)
      clientIdUtf8Length = (buffer[index++] << 8) & 0xFF00;
      clientIdUtf8Length |= buffer[index++];
      clientIdUtf8 = new Byte[clientIdUtf8Length];
      Array.Copy(buffer, index, clientIdUtf8, 0, clientIdUtf8Length);
      index += clientIdUtf8Length;
      msg.ClientId = new String(Encoding.UTF8.GetChars(clientIdUtf8));
      // [v3.1.1] if client identifier is zero bytes long, clean session must be true
      if (msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1 && clientIdUtf8Length == 0 && !msg.CleanSession) {
        throw new MqttClientException(MqttClientErrorCode.InvalidClientId);
      }

      // will topic and will message
      if (msg.WillFlag) {
        willTopicUtf8Length = (buffer[index++] << 8) & 0xFF00;
        willTopicUtf8Length |= buffer[index++];
        willTopicUtf8 = new Byte[willTopicUtf8Length];
        Array.Copy(buffer, index, willTopicUtf8, 0, willTopicUtf8Length);
        index += willTopicUtf8Length;
        msg.WillTopic = new String(Encoding.UTF8.GetChars(willTopicUtf8));

        willMessageUtf8Length = (buffer[index++] << 8) & 0xFF00;
        willMessageUtf8Length |= buffer[index++];
        willMessageUtf8 = new Byte[willMessageUtf8Length];
        Array.Copy(buffer, index, willMessageUtf8, 0, willMessageUtf8Length);
        index += willMessageUtf8Length;
        msg.WillMessage = new String(Encoding.UTF8.GetChars(willMessageUtf8));
      }

      // username
      if (isUsernameFlag) {
        usernameUtf8Length = (buffer[index++] << 8) & 0xFF00;
        usernameUtf8Length |= buffer[index++];
        usernameUtf8 = new Byte[usernameUtf8Length];
        Array.Copy(buffer, index, usernameUtf8, 0, usernameUtf8Length);
        index += usernameUtf8Length;
        msg.Username = new String(Encoding.UTF8.GetChars(usernameUtf8));
      }

      // password
      if (isPasswordFlag) {
        passwordUtf8Length = (buffer[index++] << 8) & 0xFF00;
        passwordUtf8Length |= buffer[index++];
        passwordUtf8 = new Byte[passwordUtf8Length];
        Array.Copy(buffer, index, passwordUtf8, 0, passwordUtf8Length);
        msg.Password = new String(Encoding.UTF8.GetChars(passwordUtf8));
      }

      return msg;
    }

    public override Byte[] GetBytes(Byte protocolVersion) {
      Int32 varHeaderSize = 0;
      Int32 payloadSize = 0;
      Int32 remainingLength = 0;
      Byte[] buffer;
      Int32 index = 0;

      Byte[] clientIdUtf8 = Encoding.UTF8.GetBytes(this.ClientId);
      Byte[] willTopicUtf8 = (this.WillFlag && this.WillTopic != null) ? Encoding.UTF8.GetBytes(this.WillTopic) : null;
      Byte[] willMessageUtf8 = (this.WillFlag && this.WillMessage != null) ? Encoding.UTF8.GetBytes(this.WillMessage) : null;
      Byte[] usernameUtf8 = (this.Username != null && this.Username.Length > 0) ? Encoding.UTF8.GetBytes(this.Username) : null;
      Byte[] passwordUtf8 = (this.Password != null && this.Password.Length > 0) ? Encoding.UTF8.GetBytes(this.Password) : null;

      // [v3.1.1]
      if (this.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) {
        // will flag set, will topic and will message MUST be present
        if (this.WillFlag && (this.WillQosLevel >= 0x03 ||
                               willTopicUtf8 == null || willMessageUtf8 == null ||
                               willTopicUtf8 != null && willTopicUtf8.Length == 0 ||
                               willMessageUtf8 != null && willMessageUtf8.Length == 0)) {
          throw new MqttClientException(MqttClientErrorCode.WillWrong);
        }
        // willflag not set, retain must be 0 and will topic and message MUST NOT be present
        else if (!this.WillFlag && (this.WillRetain ||
                                    willTopicUtf8 != null || willMessageUtf8 != null ||
                                    willTopicUtf8 != null && willTopicUtf8.Length != 0 ||
                                    willMessageUtf8 != null && willMessageUtf8.Length != 0)) {
          throw new MqttClientException(MqttClientErrorCode.WillWrong);
        }
      }

      if (this.KeepAlivePeriod > MAX_KEEP_ALIVE) {
        throw new MqttClientException(MqttClientErrorCode.KeepAliveWrong);
      }

      // check on will QoS Level
      if (this.WillQosLevel < MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE ||
                this.WillQosLevel > MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE) {
        throw new MqttClientException(MqttClientErrorCode.WillWrong);
      }

      // protocol name field size
      // MQTT version 3.1
      if (this.ProtocolVersion == PROTOCOL_VERSION_V3_1) {
        varHeaderSize += PROTOCOL_NAME_LEN_SIZE + PROTOCOL_NAME_V3_1_SIZE;
      }
            // MQTT version 3.1.1
            else {
        varHeaderSize += PROTOCOL_NAME_LEN_SIZE + PROTOCOL_NAME_V3_1_1_SIZE;
      }
      // protocol level field size
      varHeaderSize += PROTOCOL_VERSION_SIZE;
      // connect flags field size
      varHeaderSize += CONNECT_FLAGS_SIZE;
      // keep alive timer field size
      varHeaderSize += KEEP_ALIVE_TIME_SIZE;

      // client identifier field size
      payloadSize += clientIdUtf8.Length + 2;
      // will topic field size
      payloadSize += (willTopicUtf8 != null) ? (willTopicUtf8.Length + 2) : 0;
      // will message field size
      payloadSize += (willMessageUtf8 != null) ? (willMessageUtf8.Length + 2) : 0;
      // username field size
      payloadSize += (usernameUtf8 != null) ? (usernameUtf8.Length + 2) : 0;
      // password field size
      payloadSize += (passwordUtf8 != null) ? (passwordUtf8.Length + 2) : 0;

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
      buffer[index++] = (MQTT_MSG_CONNECT_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_CONNECT_FLAG_BITS; // [v.3.1.1]

      // encode remaining length
      index = this.EncodeRemainingLength(remainingLength, buffer, index);

      // protocol name
      buffer[index++] = 0; // MSB protocol name size
                           // MQTT version 3.1
      if (this.ProtocolVersion == PROTOCOL_VERSION_V3_1) {
        buffer[index++] = PROTOCOL_NAME_V3_1_SIZE; // LSB protocol name size
        Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1), 0, buffer, index, PROTOCOL_NAME_V3_1_SIZE);
        index += PROTOCOL_NAME_V3_1_SIZE;
        // protocol version
        buffer[index++] = PROTOCOL_VERSION_V3_1;
      }
      // MQTT version 3.1.1
      else {
        buffer[index++] = PROTOCOL_NAME_V3_1_1_SIZE; // LSB protocol name size
        Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1_1), 0, buffer, index, PROTOCOL_NAME_V3_1_1_SIZE);
        index += PROTOCOL_NAME_V3_1_1_SIZE;
        // protocol version
        buffer[index++] = PROTOCOL_VERSION_V3_1_1;
      }

      // connect flags
      Byte connectFlags = 0x00;
      connectFlags |= (usernameUtf8 != null) ? (Byte)(1 << USERNAME_FLAG_OFFSET) : (Byte)0x00;
      connectFlags |= (passwordUtf8 != null) ? (Byte)(1 << PASSWORD_FLAG_OFFSET) : (Byte)0x00;
      connectFlags |= this.WillRetain ? (Byte)(1 << WILL_RETAIN_FLAG_OFFSET) : (Byte)0x00;
      // only if will flag is set, we have to use will QoS level (otherwise is MUST be 0)
      if (this.WillFlag) {
        connectFlags |= (Byte)(this.WillQosLevel << WILL_QOS_FLAG_OFFSET);
      }

      connectFlags |= this.WillFlag ? (Byte)(1 << WILL_FLAG_OFFSET) : (Byte)0x00;
      connectFlags |= this.CleanSession ? (Byte)(1 << CLEAN_SESSION_FLAG_OFFSET) : (Byte)0x00;
      buffer[index++] = connectFlags;

      // keep alive period
      buffer[index++] = (Byte)((this.KeepAlivePeriod >> 8) & 0x00FF); // MSB
      buffer[index++] = (Byte)(this.KeepAlivePeriod & 0x00FF); // LSB

      // client identifier
      buffer[index++] = (Byte)((clientIdUtf8.Length >> 8) & 0x00FF); // MSB
      buffer[index++] = (Byte)(clientIdUtf8.Length & 0x00FF); // LSB
      Array.Copy(clientIdUtf8, 0, buffer, index, clientIdUtf8.Length);
      index += clientIdUtf8.Length;

      // will topic
      if (this.WillFlag && willTopicUtf8 != null) {
        buffer[index++] = (Byte)((willTopicUtf8.Length >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(willTopicUtf8.Length & 0x00FF); // LSB
        Array.Copy(willTopicUtf8, 0, buffer, index, willTopicUtf8.Length);
        index += willTopicUtf8.Length;
      }

      // will message
      if (this.WillFlag && willMessageUtf8 != null) {
        buffer[index++] = (Byte)((willMessageUtf8.Length >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(willMessageUtf8.Length & 0x00FF); // LSB
        Array.Copy(willMessageUtf8, 0, buffer, index, willMessageUtf8.Length);
        index += willMessageUtf8.Length;
      }

      // username
      if (usernameUtf8 != null) {
        buffer[index++] = (Byte)((usernameUtf8.Length >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(usernameUtf8.Length & 0x00FF); // LSB
        Array.Copy(usernameUtf8, 0, buffer, index, usernameUtf8.Length);
        index += usernameUtf8.Length;
      }

      // password
      if (passwordUtf8 != null) {
        buffer[index++] = (Byte)((passwordUtf8.Length >> 8) & 0x00FF); // MSB
        buffer[index++] = (Byte)(passwordUtf8.Length & 0x00FF); // LSB
        Array.Copy(passwordUtf8, 0, buffer, index, passwordUtf8.Length);
        _ = passwordUtf8.Length;
      }

      return buffer;
    }

    public override String ToString() =>
#if TRACE
      this.GetTraceString(
          "CONNECT",
          new Object[] { "protocolName", "protocolVersion", "clientId", "willFlag", "willRetain", "willQosLevel", "willTopic", "willMessage", "username", "password", "cleanSession", "keepAlivePeriod" },
          new Object[] { this.ProtocolName, this.ProtocolVersion, this.ClientId, this.WillFlag, this.WillRetain, this.WillQosLevel, this.WillTopic, this.WillMessage, this.Username, this.Password, this.CleanSession, this.KeepAlivePeriod });
#else
            base.ToString();
#endif

  }
}
