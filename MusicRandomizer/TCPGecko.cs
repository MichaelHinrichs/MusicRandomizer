#define DIRECT

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace MusicRandomizer
{
    public class ByteSwap
    {
        public static ushort Swap(ushort input)
        {
            if (BitConverter.IsLittleEndian)
                return ((ushort)(
                    ((0xFF00 & input) >> 8) |
                    ((0x00FF & input) << 8)));
            else
                return input;
        }

        public static uint Swap(uint input)
        {
            if (BitConverter.IsLittleEndian)
                return ((uint)(
                    ((0xFF000000 & input) >> 24) |
                    ((0x00FF0000 & input) >> 8) |
                    ((0x0000FF00 & input) << 8) |
                    ((0x000000FF & input) << 24)));
            else
                return input;
        }

        public static ulong Swap(ulong input)
        {
            if (BitConverter.IsLittleEndian)
                return ((ulong)(
                    ((0xFF00000000000000 & input) >> 56) |
                    ((0x00FF000000000000 & input) >> 40) |
                    ((0x0000FF0000000000 & input) >> 24) |
                    ((0x000000FF00000000 & input) >> 8) |
                    ((0x00000000FF000000 & input) << 8) |
                    ((0x0000000000FF0000 & input) << 24) |
                    ((0x000000000000FF00 & input) << 40) |
                    ((0x00000000000000FF & input) << 56)));
            else
                return input;
        }
    }

    public class Dump
    {
        public Dump(uint theStartAddress, uint theEndAddress)
        {
            Construct(theStartAddress, theEndAddress, 0);
        }

        public Dump(uint theStartAddress, uint theEndAddress, int theFileNumber)
        {
            Construct(theStartAddress, theEndAddress, theFileNumber);
        }

        private void Construct(uint theStartAddress, uint theEndAddress, int theFileNumber)
        {
            startAddress = theStartAddress;
            endAddress = theEndAddress;
            readCompletedAddress = theStartAddress;
            mem = new byte[endAddress - startAddress];
            fileNumber = theFileNumber;
        }


        public uint ReadAddress32(uint addressToRead)
        {
            //dumpStream.Seek(addressToRead - startAddress, SeekOrigin.Begin);
            //byte [] buffer = new byte[4];

            //dumpStream.Read(buffer, 0, 4);
            if (addressToRead < startAddress) return 0;
            if (addressToRead > endAddress - 4) return 0;
            byte[] buffer = new byte[4];
            Buffer.BlockCopy(mem, index(addressToRead), buffer, 0, 4);
            //GeckoApp.SubArray<byte> buffer = new GeckoApp.SubArray<byte>(mem, (int)(addressToRead - startAddress), 4);

            //Read buffer
            uint result = BitConverter.ToUInt32(buffer, 0);

            //Swap to machine endianness and return
            return ByteSwap.Swap(result);
        }

        private int index(uint addressToRead)
        {
            return (int)(addressToRead - startAddress);
        }

        public uint ReadAddress(uint addressToRead, int numBytes)
        {
            if (addressToRead < startAddress) return 0;
            if (addressToRead > endAddress - numBytes) return 0;

            byte[] buffer = new byte[4];
            Buffer.BlockCopy(mem, index(addressToRead), buffer, 0, numBytes);

            //Read buffer
            switch (numBytes)
            {
                case 4:
                    uint result = BitConverter.ToUInt32(buffer, 0);

                    //Swap to machine endianness and return
                    return ByteSwap.Swap(result);

                case 2:
                    ushort result16 = BitConverter.ToUInt16(buffer, 0);

                    //Swap to machine endianness and return
                    return ByteSwap.Swap(result16);

                default:
                    return buffer[0];
            }
        }

        public void WriteStreamToDisk()
        {
            string myDirectory = Environment.CurrentDirectory + @"\searchdumps\";
            if (!Directory.Exists(myDirectory))
            {
                Directory.CreateDirectory(myDirectory);
            }
            string myFile = myDirectory + "dump" + fileNumber.ToString() + ".dmp";

            WriteStreamToDisk(myFile);
        }

        public void WriteStreamToDisk(string filepath)
        {
            FileStream foo = new FileStream(filepath, FileMode.Create);
            foo.Write(mem, 0, (int)(endAddress - startAddress));
            foo.Close();
            foo.Dispose();
        }

        /*
        public void WriteCompressedStreamToDisk(string filepath)
        {
            ZipFile foo = new ZipFile(filepath);
            foo.AddEntry("mem", mem);
            foo.Dispose();
        }
        */

        public byte[] mem;
        private uint startAddress;
        public uint StartAddress
        {
            get { return startAddress; }
        }
        private uint endAddress;
        public uint EndAddress
        {
            get { return endAddress; }
        }
        private uint readCompletedAddress;
        public uint ReadCompletedAddress
        {
            get { return readCompletedAddress; }
            set { readCompletedAddress = value; }
        }
        private int fileNumber;
    }

    public enum ETCPErrorCode
    {
        FTDIQueryError,
        noFTDIDevicesFound,
        noTCPGeckoFound,
        FTDIResetError,
        FTDIPurgeRxError,
        FTDIPurgeTxError,
        FTDITimeoutSetError,
        FTDITransferSetError,
        FTDICommandSendError,
        FTDIReadDataError,
        FTDIInvalidReply,
        TooManyRetries,
        REGStreamSizeInvalid,
        CheatStreamSizeInvalid
    }

    public enum FTDICommand
    {
        CMD_ResultError,
        CMD_FatalError,
        CMD_OK
    }

    public enum WiiStatus
    {
        Running,
        Paused,
        Breakpoint,
        Loader,
        Unknown
    }

    public enum WiiLanguage
    {
        NoOverride,
        Japanese,
        English,
        German,
        French,
        Spanish,
        Italian,
        Dutch,
        ChineseSimplified,
        ChineseTraditional,
        Korean
    }
    public enum WiiPatches
    {
        NoPatches,
        PAL60,
        VIDTV,
        PAL60VIDTV,
        NTSC,
        NTSCVIDTV,
        PAL50,
        PAL50VIDTV
    }
    public enum WiiHookType
    {
        VI,
        WiiRemote,
        GamecubePad
    }

    public delegate void GeckoProgress(uint address, uint currentchunk, uint allchunks, uint transferred, uint length, bool okay, bool dump);

    public class ETCPGeckoException : Exception
    {
        private readonly ETCPErrorCode PErrorCode;
        public ETCPErrorCode ErrorCode
        {
            get
            {
                return PErrorCode;
            }
        }

        public ETCPGeckoException(ETCPErrorCode code)
            : base()
        {
            PErrorCode = code;
        }
        public ETCPGeckoException(ETCPErrorCode code, string message)
            : base(message)
        {
            PErrorCode = code;
        }
        public ETCPGeckoException(ETCPErrorCode code, string message, Exception inner)
            : base(message, inner)
        {
            PErrorCode = code;
        }
    }

    public class TCPGecko
    {
        private tcpconn PTCP;

        #region base constants
        private const uint packetsize = 0x400;
        private const uint uplpacketsize = 0x400;

        private const byte cmd_poke08 = 0x01;
        private const byte cmd_poke16 = 0x02;
        private const byte cmd_pokemem = 0x03;
        private const byte cmd_readmem = 0x04;
        private const byte cmd_pause = 0x06;
        private const byte cmd_unfreeze = 0x07;
        private const byte cmd_breakpoint = 0x09;
        private const byte cmd_writekern = 0x0b;
        private const byte cmd_readkern = 0x0c;
        private const byte cmd_breakpointx = 0x10;
        private const byte cmd_sendregs = 0x2F;
        private const byte cmd_getregs = 0x30;
        private const byte cmd_cancelbp = 0x38;
        private const byte cmd_sendcheats = 0x40;
        private const byte cmd_upload = 0x41;
        private const byte cmd_hook = 0x42;
        private const byte cmd_hookpause = 0x43;
        private const byte cmd_step = 0x44;
        private const byte cmd_status = 0x50;
        private const byte cmd_cheatexec = 0x60;
        private const byte cmd_rpc = 0x70;
        private const byte cmd_nbreakpoint = 0x89;
        private const byte cmd_version = 0x99;
        private const byte cmd_os_version = 0x9A;

        private const byte GCBPHit = 0x11;
        private const byte GCACK = 0xAA;
        private const byte GCRETRY = 0xBB;
        private const byte GCFAIL = 0xCC;
        private const byte GCDONE = 0xFF;

        private const byte BlockZero = 0xB0;
        private const byte BlockNonZero = 0xBD;

        private const byte GCWiiVer = 0x80;
        private const byte GCNgcVer = 0x81;
        private const byte GCWiiUVer = 0x82;

        private static readonly byte[] GCAllowedVersions = new byte[] { GCWiiUVer };

        private const byte BPExecute = 0x03;
        private const byte BPRead = 0x05;
        private const byte BPWrite = 0x06;
        private const byte BPReadWrite = 0x07;
        #endregion

        private event GeckoProgress PChunkUpdate;

        public event GeckoProgress chunkUpdate
        {
            add
            {
                PChunkUpdate += value;
            }
            remove
            {
                PChunkUpdate -= value;
            }
        }

        private bool PConnected;

        public bool connected
        {
            get
            {
                return PConnected;
            }
        }

        private bool PCancelDump;

        public bool CancelDump
        {
            get
            {
                return PCancelDump;
            }
            set
            {
                PCancelDump = value;
            }
        }

        public string Host
        {
            get
            {
                return PTCP.Host;
            }
            set
            {
                if (!PConnected)
                {
                    PTCP = new tcpconn(value, PTCP.Port);
                }
            }
        }

        public TCPGecko(string host, int port)
        {
            PTCP = new tcpconn(host, port);
            PConnected = false;
            PChunkUpdate = null;
        }

        ~TCPGecko()
        {
            if (PConnected)
                Disconnect();
        }

        protected bool InitGecko()
        {
            //Reset device
            return true;
        }

        public bool Connect()
        {
            if (PConnected)
                Disconnect();

            PConnected = false;

            //Open TCP Gecko
            try
            {
                PTCP.Connect();
                /*Byte[] init = new Byte[1];
                if (GeckoRead(init, 1) != FTDICommand.CMD_OK || init[0] != 1)
                    throw new IOException("init byte missing");*/
            }
            catch (IOException)
            {
                // Don't disconnect if there's nothing connected
                Disconnect();
                throw new ETCPGeckoException(ETCPErrorCode.noTCPGeckoFound);
            }

            //Initialise TCP Gecko
            if (InitGecko())
            {
                System.Threading.Thread.Sleep(150);
                PConnected = true;
                return true;
            }
            else
                return false;
        }

        public void Disconnect()
        {
            PConnected = false;
            PTCP.Close();
        }

        protected FTDICommand GeckoRead(byte[] recbyte, uint nobytes)
        {
            uint bytes_read = 0;

            try
            {
                PTCP.Read(recbyte, nobytes, ref bytes_read);
            }
            catch (IOException)
            {
                Disconnect();
                return FTDICommand.CMD_FatalError;       // fatal error
            }
            if (bytes_read != nobytes)
            {
                return FTDICommand.CMD_ResultError;   // lost bytes in transmission
            }

            return FTDICommand.CMD_OK;
        }

        protected FTDICommand GeckoWrite(byte[] sendbyte, int nobytes)
        {
            uint bytes_written = 0;

            try
            {
                PTCP.Write(sendbyte, nobytes, ref bytes_written);
            }
            catch (IOException)
            {
                Disconnect();
                return FTDICommand.CMD_FatalError;       // fatal error
            }
            if (bytes_written != nobytes)
            {
                return FTDICommand.CMD_ResultError;   // lost bytes in transmission
            }

            return FTDICommand.CMD_OK;
        }

        //Send update on a running process to the parent class
        protected void SendUpdate(uint address, uint currentchunk, uint allchunks, uint transferred, uint length, bool okay, bool dump)
        {
            if (PChunkUpdate != null)
                PChunkUpdate(address, currentchunk, allchunks, transferred, length, okay, dump);
        }

        public void Dump(Dump dump)
        {
            //Stream[] tempStream = { dump.dumpStream, dump.getOutputStream() };
            //Stream[] tempStream = { dump.dumpStream };
            //Dump(dump.startAddress, dump.endAddress, tempStream);
            //dump.getOutputStream().Dispose();
            //dump.WriteStreamToDisk();
            Dump(dump.StartAddress, dump.EndAddress, dump);
        }

        
        public void Dump(uint startdump, uint enddump, Stream saveStream)
        {
            Stream[] tempStream = { saveStream };
            Dump(startdump, enddump, tempStream);
        }
        

        
        public void Dump(uint startdump, uint enddump, Stream[] saveStream)
        {
            //Reset connection
            InitGecko();

            if (ValidMemory.rangeCheckId(startdump) != ValidMemory.rangeCheckId(enddump))
            {
                enddump = ValidMemory.ValidAreas[ValidMemory.rangeCheckId(startdump)].high;
            }

            if (!ValidMemory.validAddress(startdump)) return;

            //How many bytes of data have to be transferred
            uint memlength = enddump - startdump;

            //How many chunks do I need to split this data into
            //How big ist the last chunk
            uint fullchunks = memlength / packetsize;
            uint lastchunk = memlength % packetsize;

            //How many chunks do I need to transfer
            uint allchunks = fullchunks;
            if (lastchunk > 0)
                allchunks++;

            ulong GeckoMemRange = ByteSwap.Swap((ulong)(((ulong)startdump << 32) + ((ulong)enddump)));
            if (GeckoWrite(BitConverter.GetBytes(cmd_readmem), 1) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Read reply - expcecting GCACK -- nope, too slow, TCP is reliable!
            byte retry = 0;
            //while (retry < 10)
            //{
            //    Byte[] response = new Byte[1];
            //    if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
            //        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
            //    Byte reply = response[0];
            //    if (reply == GCACK)
            //        break;
            //   if (retry == 9)
            //        throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
            //}

            //Now let's send the dump information
            if (GeckoWrite(BitConverter.GetBytes(GeckoMemRange), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //We start with chunk 0
            uint chunk = 0;
            retry = 0;

            // Reset cancel flag
            bool done = false;
            CancelDump = false;

            byte[] buffer = new byte[packetsize]; //read buffer
            while (chunk < fullchunks && !done)
            {
                //No output yet availible
                SendUpdate(startdump + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, true);
                //Set buffer
                byte[] response = new byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                byte reply = response[0];
                if (reply == BlockZero)
                {
                    for (int i = 0; i < packetsize; i++)
                    {
                        buffer[i] = 0;
                    }
                }
                else
                {
                    FTDICommand returnvalue = GeckoRead(buffer, packetsize);
                    if (returnvalue == FTDICommand.CMD_ResultError)
                    {
                        retry++;
                        if (retry >= 3)
                        {
                            //Give up, too many retries
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                        }
                        //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                        continue;
                    }
                    else if (returnvalue == FTDICommand.CMD_FatalError)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }
                }
                //write received package to output stream
                foreach (Stream stream in saveStream)
                {
                    stream.Write(buffer, 0, ((int)packetsize));
                }

                //reset retry counter
                retry = 0;
                //next chunk
                chunk++;

                if (!CancelDump)
                {
                    //ackowledge package -- nope, too slow, TCP is reliable!
                    //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
                }
                else
                {
                    // User requested a cancel
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    done = true;
                }
            }

            //Final package?
            while (!done && lastchunk > 0)
            {
                //No output yet availible
                SendUpdate(startdump + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, true);
                //Set buffer
                // buffer = new Byte[lastchunk];
                byte[] response = new byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                byte reply = response[0];
                if (reply == BlockZero)
                {
                    for (int i = 0; i < lastchunk; i++)
                    {
                        buffer[i] = 0;
                    }
                }
                else
                {
                    FTDICommand returnvalue = GeckoRead(buffer, lastchunk);
                    if (returnvalue == FTDICommand.CMD_ResultError)
                    {
                        retry++;
                        if (retry >= 3)
                        {
                            //Give up, too many retries
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                        }
                        //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                        continue;
                    }
                    else if (returnvalue == FTDICommand.CMD_FatalError)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }
                }
                //write received package to output stream
                foreach (Stream stream in saveStream)
                {
                    stream.Write(buffer, 0, ((int)lastchunk));
                }
                //reset retry counter
                retry = 0;
                //cancel while loop
                done = true;
                //ackowledge package -- nope, too slow, TCP is reliable!
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }
            SendUpdate(enddump, allchunks, allchunks, memlength, memlength, true, true);
        }
        
        


        public void Dump(uint startdump, uint enddump, Dump memdump)
        {
            //Reset connection
            InitGecko();

            //How many bytes of data have to be transferred
            uint memlength = enddump - startdump;

            //How many chunks do I need to split this data into
            //How big ist the last chunk
            uint fullchunks = memlength / packetsize;
            uint lastchunk = memlength % packetsize;

            //How many chunks do I need to transfer
            uint allchunks = fullchunks;
            if (lastchunk > 0)
                allchunks++;

            ulong GeckoMemRange = ByteSwap.Swap((ulong)(((ulong)startdump << 32) + ((ulong)enddump)));
            if (GeckoWrite(BitConverter.GetBytes(cmd_readmem), 1) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Read reply - expcecting GCACK -- nope, too slow, TCP is reliable!
            byte retry = 0;
            /*while (retry < 10)
            {
                Byte[] response = new Byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                Byte reply = response[0];
                if (reply == GCACK)
                    break;
                if (retry == 9)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
            }*/

            //Now let's send the dump information
            if (GeckoWrite(BitConverter.GetBytes(GeckoMemRange), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //We start with chunk 0
            uint chunk = 0;
            retry = 0;

            // Reset cancel flag
            bool done = false;
            CancelDump = false;

            byte[] buffer = new byte[packetsize]; //read buffer
            //GeckoApp.SubArray<Byte> buffer;
            while (chunk < fullchunks && !done)
            {
                //buffer = new SubArray<byte>(mem, chunk*packetsize, packetsize);
                //No output yet availible
                SendUpdate(startdump + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, true);
                //Set buffer
                byte[] response = new byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                byte reply = response[0];
                if (reply == BlockZero)
                {
                    for (int i = 0; i < packetsize; i++)
                    {
                        buffer[i] = 0;
                    }
                }
                else
                {
                    FTDICommand returnvalue = GeckoRead(buffer, packetsize);
                    if (returnvalue == FTDICommand.CMD_ResultError)
                    {
                        retry++;
                        if (retry >= 3)
                        {
                            //Give up, too many retries
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                        }
                        //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                        continue;
                    }
                    else if (returnvalue == FTDICommand.CMD_FatalError)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }
                }
                //write received package to output stream
                //foreach (Stream stream in saveStream)
                //{
                //    stream.Write(buffer, 0, ((Int32)packetsize));
                //}

                Buffer.BlockCopy(buffer, 0, memdump.mem, (int)(chunk * packetsize + (startdump - memdump.StartAddress)), (int)packetsize);

                memdump.ReadCompletedAddress = (uint)((chunk + 1) * packetsize + startdump);

                //reset retry counter
                retry = 0;
                //next chunk
                chunk++;

                if (!CancelDump)
                {
                    //ackowledge package -- nope, too slow, TCP is reliable!
                    //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
                }
                else
                {
                    // User requested a cancel
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    done = true;
                }
            }

            //Final package?
            while (!done && lastchunk > 0)
            {
                //buffer = new SubArray<byte>(mem, chunk * packetsize, lastchunk);
                //No output yet availible
                SendUpdate(startdump + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, true);
                //Set buffer
                // buffer = new Byte[lastchunk];
                byte[] response = new byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                byte reply = response[0];
                if (reply == BlockZero)
                {
                    for (int i = 0; i < lastchunk; i++)
                    {
                        buffer[i] = 0;
                    }
                }
                else
                {
                    FTDICommand returnvalue = GeckoRead(buffer, lastchunk);
                    if (returnvalue == FTDICommand.CMD_ResultError)
                    {
                        retry++;
                        if (retry >= 3)
                        {
                            //Give up, too many retries
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                        }
                        //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                        continue;
                    }
                    else if (returnvalue == FTDICommand.CMD_FatalError)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }
                }
                //write received package to output stream
                //foreach (Stream stream in saveStream)
                //{
                //    stream.Write(buffer, 0, ((Int32)lastchunk));
                //}

                Buffer.BlockCopy(buffer, 0, memdump.mem, (int)(chunk * packetsize + (startdump - memdump.StartAddress)), (int)lastchunk);


                //reset retry counter
                retry = 0;
                //cancel while loop
                done = true;
                //ackowledge package -- nope, too slow, TCP is reliable!
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }
            SendUpdate(enddump, allchunks, allchunks, memlength, memlength, true, true);
        }

        public void Upload(uint startupload, uint endupload, Stream sendStream)
        {
            //Reset connection
            InitGecko();

            //How many bytes of data have to be transferred
            uint memlength = endupload - startupload;

            //How many chunks do I need to split this data into
            //How big ist the last chunk
            uint fullchunks = memlength / uplpacketsize;
            uint lastchunk = memlength % uplpacketsize;

            //How many chunks do I need to transfer
            uint allchunks = fullchunks;
            if (lastchunk > 0)
                allchunks++;

            ulong GeckoMemRange = ByteSwap.Swap((ulong)(((ulong)startupload << 32) + ((ulong)endupload)));
            if (GeckoWrite(BitConverter.GetBytes(cmd_upload), 1) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Read reply - expcecting GCACK -- nope, too slow, TCP is reliable!
            byte retry = 0;
            /*while (retry < 10)
            {
                Byte[] response = new Byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                Byte reply = response[0];
                if (reply == GCACK)
                    break;
                if (retry == 9)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
            }*/

            //Now let's send the upload information
            if (GeckoWrite(BitConverter.GetBytes(GeckoMemRange), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //We start with chunk 0
            uint chunk = 0;
            retry = 0;

            byte[] buffer; //read buffer
            while (chunk < fullchunks)
            {
                //No output yet availible
                SendUpdate(startupload + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, false);
                //Set buffer
                buffer = new byte[uplpacketsize];
                //Read buffer from stream
                sendStream.Read(buffer, 0, (int)uplpacketsize);
                FTDICommand returnvalue = GeckoWrite(buffer, (int)uplpacketsize);
                if (returnvalue == FTDICommand.CMD_ResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        Disconnect();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    sendStream.Seek((-1) * ((int)uplpacketsize), SeekOrigin.Current);
                    //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CMD_FatalError)
                {
                    //Major fail, give it up
                    Disconnect();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                //reset retry counter
                retry = 0;
                //next chunk
                chunk++;
                //ackowledge package -- nope, too slow, TCP is reliable!
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }

            //Final package?
            while (lastchunk > 0)
            {
                //No output yet availible
                SendUpdate(startupload + chunk * packetsize, chunk, allchunks, chunk * packetsize, memlength, retry == 0, false);
                //Set buffer
                buffer = new byte[lastchunk];
                //Read buffer from stream
                sendStream.Read(buffer, 0, (int)lastchunk);
                FTDICommand returnvalue = GeckoWrite(buffer, (int)lastchunk);
                if (returnvalue == FTDICommand.CMD_ResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        Disconnect();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    sendStream.Seek((-1) * ((int)lastchunk), SeekOrigin.Current);
                    //GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CMD_FatalError)
                {
                    //Major fail, give it up
                    Disconnect();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                //reset retry counter
                retry = 0;
                //cancel while loop
                lastchunk = 0;
                //ackowledge package -- nope, too slow, TCP is reliable!
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }

            byte[] response = new byte[1];
            if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
            byte reply = response[0];
            if (reply != GCACK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
            SendUpdate(endupload, allchunks, allchunks, memlength, memlength, true, false);
        }

        public bool Reconnect()
        {
            Disconnect();
            try
            {
                return Connect();
            }
            catch
            {
                return false;
            }
        }

        //Allows sending a basic one byte command to the Wii
        public FTDICommand RawCommand(byte id)
        {
            return GeckoWrite(BitConverter.GetBytes(id), 1);
        }

        //Pauses the game
        public void Pause()
        {
            //Only needs to send a cmd_pause to Wii
            if (RawCommand(cmd_pause) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        // Tries to repeatedly pause the game until it succeeds
        public void SafePause()
        {
            bool WasRunning = (status() == WiiStatus.Running);
            while (WasRunning)
            {
                Pause();
                System.Threading.Thread.Sleep(100);
                // Sometimes, the game doesn't actually pause...
                // So loop repeatedly until it does!
                WasRunning = (status() == WiiStatus.Running);
            }
        }

        //Unpauses the game
        public void Resume()
        {
            //Only needs to send a cmd_unfreeze to Wii
            if (RawCommand(cmd_unfreeze) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        /*
        // Tries repeatedly to resume the game until it succeeds
        public void SafeResume()
        {
            bool NotRunning = (status() != WiiStatus.Running);
            int failCounter = 0;
            while (NotRunning && failCounter < 10)
            {
                Resume();
                System.Threading.Thread.Sleep(100);
                // Sometimes, the game doesn't actually resume...
                // So loop repeatedly until it does!
                try
                {
                    NotRunning = (status() != WiiStatus.Running);
                }
                catch (TCPTCPGecko.ETCPGeckoException ex)
                {
                    NotRunning = true;
                    failCounter++;
                }
            }
        }
        */

        //Sends a GCFAIL to the game.. in case the Gecko handler hangs.. sendfail might solve it!
        public void sendfail()
        {
            //Only needs to send a cmd_unfreeze to Wii
            //Ignores the reply, send this command multiple times!
            RawCommand(GCFAIL);
        }

        #region poke commands
        //Poke a 32 bit value - note: address and value must be all in endianness of sending platform
        public void poke(uint address, uint value)
        {
            //Lower address
            address &= 0xFFFFFFFC;

            //value = send [address in big endian] [value in big endian]
            ulong PokeVal = (((ulong)address) << 32) | ((ulong)value);

            PokeVal = ByteSwap.Swap(PokeVal);

            //Send poke
            if (RawCommand(cmd_pokemem) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //write value
            if (GeckoWrite(BitConverter.GetBytes(PokeVal), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        //Copy of poke, just poke32 to make clear it is a 32-bit poke
        public void poke32(uint address, uint value)
        {
            poke(address, value);
        }

        //Poke a 16 bit value - note: address and value must be all in endianness of sending platform
        public void poke16(uint address, ushort value)
        {
            //Lower address
            address &= 0xFFFFFFFE;

            //value = send [address in big endian] [value in big endian]
            ulong PokeVal = (((ulong)address) << 32) | ((ulong)value);

            PokeVal = ByteSwap.Swap(PokeVal);

            //Send poke16
            if (RawCommand(cmd_poke16) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //write value
            if (GeckoWrite(BitConverter.GetBytes(PokeVal), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        //Poke a 08 bit value - note: address and value must be all in endianness of sending platform
        public void poke08(uint address, byte value)
        {
            //value = send [address in big endian] [value in big endian]
            ulong PokeVal = (((ulong)address) << 32) | ((ulong)value);

            PokeVal = ByteSwap.Swap(PokeVal);

            //Send poke08
            if (RawCommand(cmd_poke08) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //write value
            if (GeckoWrite(BitConverter.GetBytes(PokeVal), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }
        #endregion

        #region kern commands
        //Poke a 32 bit value to kernel. note: address and value must be all in endianness of sending platform
        public void poke_kern(uint address, uint value)
        {
            //value = send [address in big endian] [value in big endian]
            ulong PokeVal = (((ulong)address) << 32) | ((ulong)value);

            PokeVal = ByteSwap.Swap(PokeVal);

            //Send poke
            if (RawCommand(cmd_writekern) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //write value
            if (GeckoWrite(BitConverter.GetBytes(PokeVal), 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        //Read a 32 bit value from kernel. note: address must be all in endianness of sending platform
        public uint peek_kern(uint address)
        {
            //value = send [address in big endian] [value in big endian]
            address = ByteSwap.Swap(address);

            //Send read
            if (RawCommand(cmd_readkern) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //write value
            if (GeckoWrite(BitConverter.GetBytes(address), 4) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            byte[] buffer = new byte[4];
            if (GeckoRead(buffer, 4) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
        }
        #endregion

        //Returns the console status
        public WiiStatus status()
        {
            System.Threading.Thread.Sleep(100);
            //Initialise Gecko
            if (!InitGecko())
                throw new ETCPGeckoException(ETCPErrorCode.FTDIResetError);

            //Send status command
            if (RawCommand(cmd_status) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //			System.Threading.Thread.Sleep(10);

            //Read status
            byte[] buffer = new byte[1];
            if (GeckoRead(buffer, 1) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

            //analyse reply
            switch (buffer[0])
            {
                case 0: return WiiStatus.Running;
                case 1: return WiiStatus.Paused;
                case 2: return WiiStatus.Breakpoint;
                case 3: return WiiStatus.Loader;
                default: return WiiStatus.Unknown;
            }
        }

        //Step to the next frame
        public void Step()
        {
            //Reset buffers
            if (!InitGecko())
                throw new ETCPGeckoException(ETCPErrorCode.FTDIResetError);

            //Send step command
            if (RawCommand(cmd_step) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        #region breakpoint crap
        //Initialise a basic data breakpoint
        //address = Which address should the breakpoint be added on
        //bptype = how many bytes need to be added to the 8 byte aligned address - 5 for read, 6 for write, 7 for rw
        //exact = only break if the exact address is being accessed
        protected void Breakpoint(uint address, byte bptype, bool exact)
        {
            InitGecko();

            uint lowaddr = (address & 0xFFFFFFF8) | bptype;
            //Actual address to put the breakpoint - the identity adder is applied to it

            bool useGeckoBP = false;
            if (exact)
                useGeckoBP = (VersionRequest() != GCNgcVer);

            if (!useGeckoBP) //classic PPC breakpoint
            {
                if (RawCommand(cmd_breakpoint) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //Convert lowaddr to BigEndian
                uint breakpaddr = ByteSwap.Swap(lowaddr);

                if (GeckoWrite(BitConverter.GetBytes(breakpaddr), 4) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
            else //advanced exact Gecko breakpoint
            {
                if (RawCommand(cmd_nbreakpoint) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                ulong breakpaddr = ((ulong)lowaddr) << 32 | ((ulong)address);
                breakpaddr = ByteSwap.Swap(breakpaddr);

                if (GeckoWrite(BitConverter.GetBytes(breakpaddr), 8) != FTDICommand.CMD_OK)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        //Read breakpoint
        public void BreakpointR(uint address, bool exact)
        {
            Breakpoint(address, BPRead, exact);
        }
        public void BreakpointR(uint address)
        {
            Breakpoint(address, BPRead, true);
        }

        //Write breakpoint
        public void BreakpointW(uint address, bool exact)
        {
            Breakpoint(address, BPWrite, exact);
        }
        public void BreakpointW(uint address)
        {
            Breakpoint(address, BPWrite, true);
        }

        //Read/Write breakpoint
        public void BreakpointRW(uint address, bool exact)
        {
            Breakpoint(address, BPReadWrite, exact);
        }
        public void BreakpointRW(uint address)
        {
            Breakpoint(address, BPReadWrite, true);
        }


        //Execute breakpoints require a different command and different parameters
        //address = address to put the breakpoint on
        public void BreakpointX(uint address)
        {
            InitGecko();

            //Unlike Data breakpoints Execute breakpoints are exact to 4 bytes
            uint baddress = ByteSwap.Swap(((uint)(address & 0xFFFFFFFC) | BPExecute));

            //Send breakpoint execute command
            if (RawCommand(cmd_breakpointx) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send address to handler
            if (GeckoWrite(BitConverter.GetBytes(baddress), 4) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        //Returns true once a Breakpoint has hit
        //Function is depricated use status function instead - only for backwards compatibility with Delphi ports!
        public bool BreakpointHit()
        {
            byte[] buffer = new byte[1];

            if (GeckoRead(buffer, 1) != FTDICommand.CMD_OK)
                return false;

            //did we receive a bphit signal?
            return (buffer[0] == GCBPHit);
        }

        //Cancels running breakpoints
        //doesn't work thanks to a malfunction of current gecko handlers!
        public void CancelBreakpoint()
        {
            if (RawCommand(cmd_cancelbp) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }
        #endregion

        //Is this version code a correct Gecko version?
        protected bool AllowedVersion(byte version)
        {
            for (int i = 0; i < GCAllowedVersions.Length; i++)
                if (GCAllowedVersions[i] == version)
                    return true;
            return false;
        }

        public byte VersionRequest()
        {
            InitGecko();

            if (RawCommand(cmd_version) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            byte retries = 0;
            byte result = 0;
            byte[] buffer = new byte[1];

            //try to receive a version 3 times.. if it really does not return anything useful give up!
            do
            {
                if (GeckoRead(buffer, 1) == FTDICommand.CMD_OK)
                {
                    if (AllowedVersion(buffer[0]))
                    {
                        result = buffer[0];
                        break;
                    }
                }
                retries++;
            } while (retries < 3);

            return result;
        }

        public uint OsVersionRequest()
        {
            if (RawCommand(cmd_os_version) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            byte[] buffer = new byte[4];

            if (GeckoRead(buffer, 4) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
        }

        
        public uint peek(uint address)
        {
            if (!ValidMemory.validAddress(address))
            {
                return 0;
            }

            //address will be alligned to 4
            uint paddress = address & 0xFFFFFFFC;

            //Create a memory stream for the actual dump
            MemoryStream stream = new MemoryStream();

            //make sure to not send data to the output
            GeckoProgress oldUpdate = PChunkUpdate;
            PChunkUpdate = null;

            try
            {

                //dump data
                Dump(paddress, paddress + 4, stream);

                //go to beginning
                stream.Seek(0, SeekOrigin.Begin);
                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, 4);

                //Read buffer
                uint result = BitConverter.ToUInt32(buffer, 0);

                //Swap to machine endianness and return
                result = ByteSwap.Swap(result);

                return result;
            }
            finally
            {
                PChunkUpdate = oldUpdate;

                //make sure the Stream is properly closed
                stream.Close();
            }
        }

        #region register operations
        //Read registers in breakpoint cases
        public void GetRegisters(Stream stream, uint contextAddress)
        {
            uint bytesExpected = 0x1B0;

            //Read registers
            MemoryStream buffer = new MemoryStream();
            Dump(contextAddress + 8, contextAddress + 8 + bytesExpected, buffer);

            byte[] bytes = buffer.ToArray();

            //Store registers to output stream!
            stream.Write(bytes, 0x80, 4); // cr
            stream.Write(bytes, 0x8c, 4); // xer
            stream.Write(bytes, 0x88, 4); // ctr
            stream.Write(new byte[8], 0, 8); // dsis, dar (dunno)
            stream.Write(bytes, 0x90, 8); // srr0, srr1
            stream.Write(bytes, 0x0, 4 * 32); // gprs
            stream.Write(bytes, 0x84, 4); // lr
            stream.Write(bytes, 0xb0, 8 * 32); // fprs
        }

        //Send registers
        public void SendRegisters(Stream sendStream, uint contextAddress)
        {
            MemoryStream buffer = new MemoryStream();
            byte[] bytes = new byte[0xA0];
            sendStream.Seek(0, SeekOrigin.Begin);
            sendStream.Read(bytes, 0, bytes.Length);
            buffer.Write(bytes, 0x1C, 4 * 32); // gprs
            buffer.Write(bytes, 0x0, 4); // cr
            buffer.Write(bytes, 0x9C, 4); // lr
            buffer.Write(bytes, 0x8, 4); //ctr
            buffer.Write(bytes, 0x4, 4); // xer
            buffer.Write(bytes, 0x14, 8); // srr0, srr1

            buffer.Seek(0, SeekOrigin.Begin);

            Upload(contextAddress + 8, contextAddress + 8 + 0x98, buffer);
        }
        #endregion

        #region Cheat related stuff
        private ulong readInt64(Stream inputstream)
        {
            byte[] buffer = new byte[8];
            inputstream.Read(buffer, 0, 8);
            ulong result = BitConverter.ToUInt64(buffer, 0);
            result = ByteSwap.Swap(result);
            return result;
        }

        private void writeInt64(Stream outputstream, ulong value)
        {
            ulong bvalue = ByteSwap.Swap(value);
            byte[] buffer = BitConverter.GetBytes(bvalue);
            outputstream.Write(buffer, 0, 8);
        }

        private void insertInto(Stream insertStream, ulong value)
        {
            MemoryStream tempstream = new MemoryStream();
            writeInt64(tempstream, value);
            insertStream.Seek(0, SeekOrigin.Begin);

            byte[] streambuffer = new byte[insertStream.Length];
            insertStream.Read(streambuffer, 0, (int)insertStream.Length);
            tempstream.Write(streambuffer, 0, (int)insertStream.Length);

            insertStream.Seek(0, SeekOrigin.Begin);
            tempstream.Seek(0, SeekOrigin.Begin);

            streambuffer = new byte[tempstream.Length];
            tempstream.Read(streambuffer, 0, (int)tempstream.Length);
            insertStream.Write(streambuffer, 0, (int)tempstream.Length);

            tempstream.Close();
        }

        public void sendCheats(Stream inputStream)
        {
            MemoryStream cheatStream = new MemoryStream();
            byte[] orgData = new byte[inputStream.Length];
            inputStream.Seek(0, SeekOrigin.Begin);
            inputStream.Read(orgData, 0, (int)inputStream.Length);
            cheatStream.Write(orgData, 0, (int)inputStream.Length);

            uint length = (uint)cheatStream.Length;
            //Cheat stream length must be multiple of 8
            if (length % 8 != 0)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.CheatStreamSizeInvalid);
            }

            //Reset buffers
            InitGecko();

            //Make sure the stream ends with F0/F1
            cheatStream.Seek(-8, SeekOrigin.End);
            ulong data = readInt64(cheatStream);
            data &= 0xFE00000000000000;
            if ((data != 0xF000000000000000) &&
                 (data != 0xFE00000000000000))
            {
                cheatStream.Seek(0, SeekOrigin.End);
                writeInt64(cheatStream, 0xF000000000000000);
            }

            //Make sure it starts with 00D0C0...
            cheatStream.Seek(0, SeekOrigin.Begin);
            data = readInt64(cheatStream);
            if (data != 0x00D0C0DE00D0C0DE)
            {
                insertInto(cheatStream, 0x00D0C0DE00D0C0DE);
            }

            cheatStream.Seek(0, SeekOrigin.Begin);

            length = (uint)cheatStream.Length;

            if (GeckoWrite(BitConverter.GetBytes(cmd_sendcheats), 1) != FTDICommand.CMD_OK)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }

            //How many chunks do I need to split this data into
            //How big ist the last chunk
            uint fullchunks = length / uplpacketsize;
            uint lastchunk = length % uplpacketsize;

            //How many chunks do I need to transfer
            uint allchunks = fullchunks;
            if (lastchunk > 0)
                allchunks++;

            //Read reply - expcecting GCACK
            byte retry = 0;
            while (retry < 10)
            {
                byte[] response = new byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CMD_OK)
                {
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                byte reply = response[0];
                if (reply == GCACK)
                    break;
                if (retry == 9)
                {
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
                }
            }

            uint blength = ByteSwap.Swap(length);
            if (GeckoWrite(BitConverter.GetBytes(blength), 4) != FTDICommand.CMD_OK)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }

            //We start with chunk 0
            uint chunk = 0;
            retry = 0;

            byte[] buffer; //read buffer
            while (chunk < fullchunks)
            {
                //No output yet availible
                SendUpdate(0x00d0c0de, chunk, allchunks, chunk * packetsize, length, retry == 0, false);
                //Set buffer
                buffer = new byte[uplpacketsize];
                //Read buffer from stream
                cheatStream.Read(buffer, 0, (int)uplpacketsize);
                FTDICommand returnvalue = GeckoWrite(buffer, (int)uplpacketsize);
                if (returnvalue == FTDICommand.CMD_ResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)uplpacketsize), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CMD_FatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }

                byte[] response = new byte[1];
                returnvalue = GeckoRead(response, 1);
                if ((returnvalue == FTDICommand.CMD_ResultError) || (response[0] != GCACK))
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)uplpacketsize), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CMD_FatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }

                //reset retry counter
                retry = 0;
                //next chunk
                chunk++;
                //ackowledge package
            }

            //Final package?
            while (lastchunk > 0)
            {
                //No output yet availible
                SendUpdate(0x00d0c0de, chunk, allchunks, chunk * packetsize, length, retry == 0, false);
                //Set buffer
                buffer = new byte[lastchunk];
                //Read buffer from stream
                cheatStream.Read(buffer, 0, (int)lastchunk);
                FTDICommand returnvalue = GeckoWrite(buffer, (int)lastchunk);
                if (returnvalue == FTDICommand.CMD_ResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)lastchunk), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CMD_FatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                //reset retry counter
                retry = 0;
                //cancel while loop
                lastchunk = 0;
                //ackowledge package
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }
            SendUpdate(0x00d0c0de, allchunks, allchunks, length, length, true, false);
            cheatStream.Close();
        }

        //Execute cheats
        public void ExecuteCheats()
        {
            if (RawCommand(cmd_cheatexec) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }
        #endregion

        #region hooking crap
        //Hook command:
        public void Hook(bool pause, WiiLanguage language, WiiPatches patches, WiiHookType hookType)
        {
            InitGecko();

            //Hookpause command or regular hook?
            byte command;
            if (pause)
                command = cmd_hookpause;
            else
                command = cmd_hook;

            //Perform hook command
            command += (byte)hookType;
            if (RawCommand(command) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send language
            if (language != WiiLanguage.NoOverride)
                command = (byte)(language - 1);
            else
                command = 0xCD;

            if (RawCommand(command) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send patches
            command = (byte)patches;
            if (RawCommand(command) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        public void Hook()
        {
            Hook(false, WiiLanguage.NoOverride, WiiPatches.NoPatches, WiiHookType.VI);
        }
        #endregion

        #region Screenshot processing
        private static byte ConvertSafely(double floatValue)
        {
            return (byte)Math.Round(Math.Max(0, Math.Min(floatValue, 255)));
        }

        private static Bitmap ProcessImage(uint width, uint height, Stream analyze)
        {

            Bitmap BitmapRGB = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
            BitmapData bData = BitmapRGB.LockBits(new Rectangle(0, 0, (int)width, (int)height),
                                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int size = bData.Stride * bData.Height;

            byte[] data = new byte[size];

            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            byte[] bufferBytes = new byte[width * height * 2];

            int y = 0;
            int u = 0;
            int v = 0;
            int yvpos = 0;
            int rgbpos = 0;

            analyze.Read(bufferBytes, 0, (int)(width * height * 2));
            for (int i = 0; i < width * height; i++)
            {
                yvpos = i * 2;
                //YV encoding is a bit awkward!
                if (i % 2 == 0) //Even
                {
                    y = bufferBytes[yvpos];
                    u = bufferBytes[yvpos + 1]; //U value is taken from current V block
                    v = bufferBytes[yvpos + 3]; //Take V from next data YV block
                }
                else //Odd
                    y = bufferBytes[yvpos];
                //u is taken from last pixel
                //v too!

                rgbpos = (i * 3);
                data[rgbpos] = ConvertSafely(1.164 * (y - 16) + 2.017 * (u - 128));                     //Blue pixel value
                data[rgbpos + 1] = ConvertSafely(1.164 * (y - 16) - 0.392 * (u - 128) - 0.813 * (v - 128)); //Greeen pixel value
                data[rgbpos + 2] = ConvertSafely(1.164 * (y - 16) + 1.596 * (v - 128));                     //Red pixel value
            }

            System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, data.Length);

            BitmapRGB.UnlockBits(bData);

            return BitmapRGB;
        }

        public Image Screenshot()
        {
            MemoryStream analyze;

            //Dump video registers
            analyze = new MemoryStream();
            Dump(0xCC002000, 0xCC002080, analyze);
            analyze.Seek(0, SeekOrigin.Begin);
            byte[] viregs = new byte[128];
            analyze.Read(viregs, 0, 128);
            analyze.Close();

            //Extract width, height and offset in memory
            uint swidth = (uint)(viregs[0x49] << 3);
            uint sheight = (uint)(((viregs[0] << 5) | (viregs[1] >> 3)) & 0x07FE);
            uint soffset = (uint)((viregs[0x1D] << 16) | (viregs[0x1E] << 8) | viregs[0x1F]);
            if ((viregs[0x1C] & 0x10) == 0x10)
                soffset <<= 5;
            soffset += 0x80000000;
            soffset -= (uint)((viregs[0x1C] & 0xF) << 3);

            //Dump video data
            analyze = new MemoryStream();
            Dump(soffset, soffset + sheight * swidth * 2, analyze);
            analyze.Seek(0, SeekOrigin.Begin);

            if (sheight > 600) //Progressive mode!
            {
                sheight /= 2;
                swidth *= 2;
            }

            Bitmap b = ProcessImage(swidth, sheight, analyze);
            analyze.Close();

            return b;
        }
        #endregion

        #region RPC

        /* values in host endianess. */
        public uint rpc(uint address, params uint[] args)
        {
            return (uint)(rpc64(address, args) >> 32);
        }

        /* values in host endianess. */
        public ulong rpc64(uint address, params uint[] args)
        {
            byte[] buffer = new byte[4 + 8 * 4];

            //value = send [address in big endian] [value in big endian]
            address = ByteSwap.Swap(address);

            BitConverter.GetBytes(address).CopyTo(buffer, 0);

            for (int i = 0; i < 8; i++)
            {
                if (i < args.Length)
                {
                    BitConverter.GetBytes(ByteSwap.Swap(args[i])).CopyTo(buffer, 4 + i * 4);
                }
                else
                {
                    BitConverter.GetBytes(0xfecad0ba).CopyTo(buffer, 4 + i * 4);
                }
            }

            //Send read
            if (RawCommand(cmd_rpc) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);


            //write value
            if (GeckoWrite(buffer, buffer.Length) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            if (GeckoRead(buffer, 8) != FTDICommand.CMD_OK)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            return ByteSwap.Swap(BitConverter.ToUInt64(buffer, 0));
        }

        #endregion
    }
}
