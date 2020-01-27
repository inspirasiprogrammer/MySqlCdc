using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Parses <see cref="FormatDescriptionEvent"/> events.
    /// Supports all versions of MariaDB and MySQL 5.0+ (V4 header format).
    /// </summary>
    public class FormatDescriptionEventParser : IEventParser
    {
        private const int EventTypesOffset = 2 + 50 + 4 + 1;

        /// <summary>
        /// Parses <see cref="FormatDescriptionEvent"/> from the buffer.
        /// </summary>       
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var binlogVersion = reader.ReadInt(2);
            var serverVersion = reader.ReadString(50).Trim((char)0);

            // Redundant timestamp & header length which is always 19
            reader.Skip(5);

            // Get size of the event payload to determine beginning of the checksum part
            reader.Skip((int)EventType.FORMAT_DESCRIPTION_EVENT - 1);
            var eventPayloadLength = reader.ReadInt(1);

            var checksumType = ChecksumType.None;
            if (eventPayloadLength != header.EventLength - EventConstants.HeaderSize)
            {
                reader.Skip(eventPayloadLength - (EventTypesOffset + (int)EventType.FORMAT_DESCRIPTION_EVENT));
                checksumType = (ChecksumType)reader.ReadInt(1);
            }

            return new FormatDescriptionEvent(header, binlogVersion, serverVersion, checksumType);
        }
    }
}
