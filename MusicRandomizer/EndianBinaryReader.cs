using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
    sealed class EndianBinaryReader : IDisposable
    {
        private bool disposed;
        private byte[] buffer;

        private delegate void ArrayReverse(byte[] array, int count);
        private static readonly ArrayReverse[] fastReverse = new ArrayReverse[] { null, null, ArrayReverse2, null, ArrayReverse4, null, null, null, ArrayReverse8 };

        private static readonly Dictionary<Type, List<Tuple<int, TypeCode>>> parserCache = new Dictionary<Type, List<Tuple<int, TypeCode>>>();

        public Stream BaseStream { get; private set; }
        public Endianness Endianness { get; set; }
        public static Endianness SystemEndianness { get { return BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian; } }

        private bool Reverse { get { return SystemEndianness != Endianness; } }

        public EndianBinaryReader(Stream baseStream)
            : this(baseStream, Endianness.BigEndian)
        { }

        public EndianBinaryReader(Stream baseStream, Endianness endianness)
        {
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            if (!baseStream.CanRead) throw new ArgumentException("base stream is not readable.", "baseStream");

            BaseStream = baseStream;
            Endianness = endianness;
        }

        ~EndianBinaryReader()
        {
            Dispose(false);
        }

        /// <summary>
        /// Fills the buffer with bytes bytes, possibly reversing every stride. Bytes must be a multiple of stide. Stide must be 1 or 2 or 4 or 8.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="stride"></param>
        private void FillBuffer(int bytes, int stride)
        {
            if (buffer == null || buffer.Length < bytes)
                buffer = new byte[bytes];

            for (int i = 0, read = 0; i < bytes; i += read)
            {
                read = BaseStream.Read(buffer, i, bytes - i);
                if (read <= 0) throw new EndOfStreamException();
            }

            if (Reverse)
            {
                if (fastReverse[stride] != null)
                    fastReverse[stride](buffer, bytes);
                else
                    for (int i = 0; i < bytes; i += stride)
                    {
                        Array.Reverse(buffer, i, stride);
                    }
            }
        }

        private static void ArrayReverse2(byte[] array, int arrayLength)
        {
            byte temp;

            while (arrayLength > 0)
            {
                temp = array[arrayLength - 2];
                array[arrayLength - 2] = array[arrayLength - 1];
                array[arrayLength - 1] = temp;
                arrayLength -= 2;
            }
        }

        private static void ArrayReverse4(byte[] array, int arrayLength)
        {
            byte temp;

            while (arrayLength > 0)
            {
                temp = array[arrayLength - 3];
                array[arrayLength - 3] = array[arrayLength - 2];
                array[arrayLength - 2] = temp;
                temp = array[arrayLength - 4];
                array[arrayLength - 4] = array[arrayLength - 1];
                array[arrayLength - 1] = temp;
                arrayLength -= 4;
            }
        }

        private static void ArrayReverse8(byte[] array, int arrayLength)
        {
            byte temp;

            while (arrayLength > 0)
            {
                temp = array[arrayLength - 5];
                array[arrayLength - 5] = array[arrayLength - 4];
                array[arrayLength - 4] = temp;
                temp = array[arrayLength - 6];
                array[arrayLength - 6] = array[arrayLength - 3];
                array[arrayLength - 3] = temp;
                temp = array[arrayLength - 7];
                array[arrayLength - 7] = array[arrayLength - 2];
                array[arrayLength - 2] = temp;
                temp = array[arrayLength - 8];
                array[arrayLength - 8] = array[arrayLength - 1];
                array[arrayLength - 1] = temp;
                arrayLength -= 8;
            }
        }

        public byte ReadByte()
        {
            FillBuffer(1, 1);

            return buffer[0];
        }

        public byte[] ReadBytes(int count)
        {
            byte[] temp;

            FillBuffer(count, 1);
            temp = new byte[count];
            Array.Copy(buffer, 0, temp, 0, count);
            return temp;
        }

        public sbyte ReadSByte()
        {
            FillBuffer(1, 1);

            unchecked
            {
                return (sbyte)buffer[0];
            }
        }

        public sbyte[] ReadSBytes(int count)
        {
            sbyte[] temp;

            temp = new sbyte[count];
            FillBuffer(count, 1);

            unchecked
            {
                for (int i = 0; i < count; i++)
                {
                    temp[i] = (sbyte)buffer[i];
                }
            }

            return temp;
        }

        public char ReadChar(Encoding encoding)
        {
            int size;

            if (encoding == null) throw new ArgumentNullException("encoding");

            size = GetEncodingSize(encoding);
            FillBuffer(size, size);
            return encoding.GetChars(buffer, 0, size)[0];
        }

        public char[] ReadChars(Encoding encoding, int count)
        {
            int size;

            if (encoding == null) throw new ArgumentNullException("encoding");

            size = GetEncodingSize(encoding);
            FillBuffer(size * count, size);
            return encoding.GetChars(buffer, 0, size * count);
        }

        private static int GetEncodingSize(Encoding encoding)
        {
            if (encoding == Encoding.UTF8 || encoding == Encoding.ASCII)
                return 1;
            else if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
                return 2;

            return 1;
        }

        public string ReadStringNT(Encoding encoding)
        {
            string text;

            text = "";

            do
            {
                text += ReadChar(encoding);
            } while (!text.EndsWith("\0", StringComparison.Ordinal));

            return text.Remove(text.Length - 1);
        }

        public string ReadString(Encoding encoding, int count)
        {
            return new string(ReadChars(encoding, count));
        }

        public double ReadDouble()
        {
            const int size = sizeof(double);
            FillBuffer(size, size);
            return BitConverter.ToDouble(buffer, 0);
        }

        public double[] ReadDoubles(int count)
        {
            const int size = sizeof(double);
            double[] temp;

            temp = new double[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToDouble(buffer, size * i);
            }
            return temp;
        }

        public float ReadSingle()
        {
            const int size = sizeof(float);
            FillBuffer(size, size);
            return BitConverter.ToSingle(buffer, 0);
        }

        public float[] ReadSingles(int count)
        {
            const int size = sizeof(float);
            float[] temp;

            temp = new float[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToSingle(buffer, size * i);
            }
            return temp;
        }

        public int ReadInt32()
        {
            const int size = sizeof(int);
            FillBuffer(size, size);
            return BitConverter.ToInt32(buffer, 0);
        }

        public int[] ReadInt32s(int count)
        {
            const int size = sizeof(int);
            int[] temp;

            temp = new int[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToInt32(buffer, size * i);
            }
            return temp;
        }

        public long ReadInt64()
        {
            const int size = sizeof(long);
            FillBuffer(size, size);
            return BitConverter.ToInt64(buffer, 0);
        }

        public long[] ReadInt64s(int count)
        {
            const int size = sizeof(long);
            long[] temp;

            temp = new long[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToInt64(buffer, size * i);
            }
            return temp;
        }

        public short ReadInt16()
        {
            const int size = sizeof(short);
            FillBuffer(size, size);
            return BitConverter.ToInt16(buffer, 0);
        }

        public short[] ReadInt16s(int count)
        {
            const int size = sizeof(short);
            short[] temp;

            temp = new short[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToInt16(buffer, size * i);
            }
            return temp;
        }

        public ushort ReadUInt16()
        {
            const int size = sizeof(ushort);
            FillBuffer(size, size);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public ushort[] ReadUInt16s(int count)
        {
            const int size = sizeof(ushort);
            ushort[] temp;

            temp = new ushort[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToUInt16(buffer, size * i);
            }
            return temp;
        }

        public uint ReadUInt32()
        {
            const int size = sizeof(uint);
            FillBuffer(size, size);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public uint[] ReadUInt32s(int count)
        {
            const int size = sizeof(uint);
            uint[] temp;

            temp = new uint[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToUInt32(buffer, size * i);
            }
            return temp;
        }

        public ulong ReadUInt64()
        {
            const int size = sizeof(ulong);
            FillBuffer(size, size);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public ulong[] ReadUInt64s(int count)
        {
            const int size = sizeof(ulong);
            ulong[] temp;

            temp = new ulong[count];
            FillBuffer(size * count, size);

            for (int i = 0; i < count; i++)
            {
                temp[i] = BitConverter.ToUInt64(buffer, size * i);
            }
            return temp;
        }

        private List<Tuple<int, TypeCode>> GetParser(Type type)
        {
            List<Tuple<int, TypeCode>> parser;

            /* A parser describes how to read in a type in an Endian
             * appropriate manner. Basically it describes as series of calls to
             * the Read* methods above to parse the structure.
             * The parser runs through each element in the list in order. If
             * the TypeCode is not Empty then it reads an array of values
             * according to the integer. Otherwise it skips a number of bytes. */

            try
            {
                parser = parserCache[type];
            }
            catch (KeyNotFoundException)
            {
                parser = new List<Tuple<int, TypeCode>>();

                if (Endianness != SystemEndianness)
                {
                    int pos, sz;

                    pos = 0;
                    foreach (var item in type.GetFields())
                    {
                        int off = Marshal.OffsetOf(type, item.Name).ToInt32();
                        if (off != pos)
                        {
                            parser.Add(new Tuple<int, TypeCode>(off - pos, TypeCode.Empty));
                            pos = off;
                        }
                        switch (Type.GetTypeCode(item.FieldType))
                        {
                            case TypeCode.Byte:
                            case TypeCode.SByte:
                                pos += 1;
                                parser.Add(new Tuple<int, TypeCode>(1, Type.GetTypeCode(item.FieldType)));
                                break;
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                                pos += 2;
                                parser.Add(new Tuple<int, TypeCode>(1, Type.GetTypeCode(item.FieldType)));
                                break;
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Single:
                                pos += 4;
                                parser.Add(new Tuple<int, TypeCode>(1, Type.GetTypeCode(item.FieldType)));
                                break;
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Double:
                                pos += 8;
                                parser.Add(new Tuple<int, TypeCode>(1, Type.GetTypeCode(item.FieldType)));
                                break;
                            case TypeCode.Object:
                                if (item.FieldType.IsArray)
                                {
                                    /* array */
                                    Type elementType;
                                    MarshalAsAttribute[] attrs;
                                    MarshalAsAttribute attr;

                                    attrs = (MarshalAsAttribute[])item.GetCustomAttributes(typeof(MarshalAsAttribute), false);
                                    if (attrs.Length != 1)
                                        throw new ArgumentException(string.Format("Field `{0}' is an array without a MarshalAs attribute.", item.Name), "type");

                                    attr = attrs[0];
                                    if (attr.Value != UnmanagedType.ByValArray)
                                        throw new ArgumentException(string.Format("Field `{0}' is not a ByValArray.", item.Name), "type");

                                    elementType = item.FieldType.GetElementType();
                                    switch (Type.GetTypeCode(elementType))
                                    {
                                        case TypeCode.Byte:
                                        case TypeCode.SByte:
                                            pos += 1 * attr.SizeConst;
                                            parser.Add(new Tuple<int, TypeCode>(attr.SizeConst, Type.GetTypeCode(elementType)));
                                            break;
                                        case TypeCode.Int16:
                                        case TypeCode.UInt16:
                                            pos += 2 * attr.SizeConst;
                                            parser.Add(new Tuple<int, TypeCode>(attr.SizeConst, Type.GetTypeCode(elementType)));
                                            break;
                                        case TypeCode.Int32:
                                        case TypeCode.UInt32:
                                        case TypeCode.Single:
                                            pos += 4 * attr.SizeConst;
                                            parser.Add(new Tuple<int, TypeCode>(attr.SizeConst, Type.GetTypeCode(elementType)));
                                            break;
                                        case TypeCode.Int64:
                                        case TypeCode.UInt64:
                                        case TypeCode.Double:
                                            pos += 8 * attr.SizeConst;
                                            parser.Add(new Tuple<int, TypeCode>(attr.SizeConst, Type.GetTypeCode(elementType)));
                                            break;
                                        case TypeCode.Object:
                                            /* nested structure */
                                            for (int i = 0; i < attr.SizeConst; i++)
                                            {
                                                pos += Marshal.SizeOf(elementType);
                                                parser.AddRange(GetParser(elementType));
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    /* nested structure */
                                    pos += Marshal.SizeOf(item.FieldType);
                                    parser.AddRange(GetParser(item.FieldType));
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    sz = Marshal.SizeOf(type);
                    if (sz != pos)
                    {
                        parser.Add(new Tuple<int, TypeCode>(sz - pos, TypeCode.Empty));
                    }
                }
                else
                {
                    int sz;

                    sz = Marshal.SizeOf(type);
                    parser.Add(new Tuple<int, TypeCode>(sz, TypeCode.Byte));
                }
                parserCache.Add(type, parser);
            }

            return parser;
        }

        private void RunParser(List<Tuple<int, TypeCode>> parser, BinaryWriter wr)
        {
            foreach (var item in parser)
            {
                /* Assumption: Types of the same size can be interchanged. */
                switch (item.Item2)
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        wr.Write(ReadBytes(item.Item1), 0, item.Item1);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        foreach (var val in ReadInt16s(item.Item1))
                            wr.Write(val);
                        break;
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Single:
                        foreach (var val in ReadInt32s(item.Item1))
                            wr.Write(val);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Double:
                        foreach (var val in ReadInt64s(item.Item1))
                            wr.Write(val);
                        break;
                    case TypeCode.Empty:
                        BaseStream.Seek(item.Item1, SeekOrigin.Current);
                        wr.BaseStream.Seek(item.Item1, SeekOrigin.Current);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public object ReadStructure(Type type)
        {
            List<Tuple<int, TypeCode>> parser;
            object result;

            parser = GetParser(type);

            using (var ms = new MemoryStream())
            {
                using (var wr = new BinaryWriter(ms))
                {
                    RunParser(parser, wr);
                }
                result = Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(ms.ToArray(), 0), type);
            }
            return result;
        }

        public Array ReadStructures(Type type, int count)
        {
            List<Tuple<int, TypeCode>> parser;
            Array result;

            parser = GetParser(type);

            result = Array.CreateInstance(type, count);

            using (var ms = new MemoryStream())
            {
                using (var wr = new BinaryWriter(ms))
                {
                    for (int i = 0; i < count; i++)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        RunParser(parser, wr);
                        result.SetValue(Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(ms.ToArray(), 0), type), i);
                    }
                }
            }
            return result;
        }

        public void Close()
        {
            BaseStream.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                BaseStream = null;
                buffer = null;

                disposed = true;
            }
        }
    }

    enum Endianness
    {
        BigEndian,
        LittleEndian,
    }
}