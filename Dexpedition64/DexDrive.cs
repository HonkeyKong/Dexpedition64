using System;
using System.IO.Ports;
using System.Threading;

namespace Dexpedition64
{
    class DexDrive
    {
        enum DexCommands { INIT = 0x00, STATUS = 0x01, READ = 0x02, SEEK = 0x03, WRITE = 0x04, LIGHT = 0x07, MAGIC_HANDSHAKE = 0x27 };
        enum DexResponses { POUT = 0x20, ERROR = 0x21, NOCARD = 0x22, CARD = 0x23, SEEK_OK = 0x27, WRITE_OK = 0x28, WRITE_SAME = 0x29, WAIT = 0x2A, ID = 0x40, DATA = 0x41 };

        //DexDrive communication port
        SerialPort OpenedPort = null;

        //Contains a firmware version of a detected device
        string FirmwareVersion = null;

        string ErrorMessage = null;

        string GetError()
        {
            return ErrorMessage;
        }

        //Init DexDrive (string returned if an error happened)
        public bool StartDexDrive(string ComPortName)
        {
            //Define a port to open
            OpenedPort = new SerialPort(ComPortName, 38400, Parity.None, 8, StopBits.One) { ReadBufferSize = 256 };

            //Try to open a selected port (in case of an error return a descriptive string)
            try { OpenedPort.Open(); }
            catch (Exception e) { 
                ErrorMessage = e.Message; 
                return false;
            }

            //Dexdrive won't respond if RTS is not toggled on/off
            OpenedPort.RtsEnable = false;
            Thread.Sleep(300);
            OpenedPort.RtsEnable = true;
            Thread.Sleep(300);

            //DTR line is used for additional power
            OpenedPort.DtrEnable = true;

            //Check if DexDrive is attached to the port
            //Detection may fail 1st or 2nd time, so the command is sent 5 times
            for (int i = 0; i < 5; i++)
            {
                OpenedPort.DiscardInBuffer();
                OpenedPort.Write("XXXXX");
                Thread.Sleep(20);
            }

            //Check for "IAI" string
            byte[] ReadData = ReadDataFromPort();
            if (ReadData[0] != 0x49 || ReadData[1] != 0x41 || ReadData[2] != 0x49) return false;

            //Wake DexDrive up (kick it from POUT mode)
            SendDataToPort((byte)DexCommands.INIT, new byte[] { 0x10, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0xAA, 0xBB, 0xCC, 0xDD }, 50);

            // Read data from serial port
            ReadData = ReadDataFromPort();

            // Check for "N64" string
            if (ReadData[5] != 0x4E || ReadData[6] != 0x36 || ReadData[7] != 0x34)
            {
                ErrorMessage = "Detected device is not a N64 DexDrive.";
                return false;
            }

            //Fetch the firmware version
            FirmwareVersion = (ReadData[8] >> 6).ToString() + "." + ((ReadData[8] >> 2) & 0xF).ToString() + (ReadData[8] & 0x3).ToString();

            //Send magic handshake signal 10 times
            for (int i = 0; i < 10; i++) SendDataToPort((byte)DexCommands.MAGIC_HANDSHAKE, null, 0);
            Thread.Sleep(50);

            //Turn on the status light
            SendDataToPort((byte)DexCommands.LIGHT, new byte[] { 1 }, 50);

            //Everything went well, DexDrive is ready to recieve commands
            return true;
        }

        //Cleanly stop working with DexDrive
        public void StopDexDrive()
        {
            if (OpenedPort.IsOpen == true) OpenedPort.Close();
        }

        //Get the firmware version of a DexDrive
        public string GetFirmwareVersion()
        {
            return FirmwareVersion;
        }

        //Send DexDrive command on the opened COM port with a delay
        private void SendDataToPort(byte Command, byte[] Data, int Delay)
        {
            //Clear everything in the input buffer
            OpenedPort.DiscardInBuffer();

            //Every command must begin with "IAI" string
            OpenedPort.Write("IAI" + (char)Command);
            if (Data != null) OpenedPort.Write(Data, 0, Data.Length);

            //Wait for a required timeframe (for the DexDrive response)
            if (Delay > 0) Thread.Sleep(Delay);
        }

        //Catch the response from a DexDrive
        private byte[] ReadDataFromPort()
        {
            //Buffer for reading data
            byte[] InputStream = new byte[261];

            //Read data from DexDrive
            if (OpenedPort.BytesToRead != 0) OpenedPort.Read(InputStream, 0, 261);

            return InputStream;
        }

        //Read a specified frame of a Memory Card
        public byte[] ReadMemoryCardFrame(ushort FrameNumber)
        {
            //256 byte frame data from a Memory Card
            byte[] ReturnDataBuffer = new byte[256];

            int DelayCounter = 0;

            byte FrameLsb = (byte)(FrameNumber & 0xFF);     //Least significant byte
            byte FrameMsb = (byte)(FrameNumber >> 8);       //Most significant byte
            byte XorData = (byte)(FrameLsb ^ FrameMsb);     //XOR variable for consistency checking

            //Read a frame from the Memory Card
            SendDataToPort((byte)DexCommands.READ, new byte[] { FrameLsb, FrameMsb }, 0);
            
            //Wait for the buffer to fill
            while (OpenedPort.BytesToRead < 261 && DelayCounter < 16)
            {
                Thread.Sleep(5);
                DelayCounter++;
            }

            //Read Memory Card data
            byte[] ReadData = ReadDataFromPort();

            //Copy received data (filter IAI prefix)
            Array.Copy(ReadData, 4, ReturnDataBuffer, 0, 256);
            
            //Calculate XOR checksum
            for (int i = 0; i < 256; i++)
            {
                XorData ^= ReturnDataBuffer[i];
            }

            //Return null if there is a checksum missmatch
            if (XorData != ReadData[260])
            {
                ErrorMessage = "Data Mismatch!";
                return null;
            }

            //Return read data
            return ReturnDataBuffer;
        }

        public static byte[] BlankPage()
        {
            byte[] zeroes = new byte[256];
            int i;
            for (i = 0; i < 256; i++)
            {
                zeroes[i] = 0x00;
            }
            return zeroes;
        }

        //Write a specified frame to a Memory Card
        public bool WriteMemoryCardFrame(ushort FrameNumber, byte[] FrameData)
        {
            //Buffer for storing read data from the DexDrive
            byte[] ReadData = null;

            byte FrameLsb = (byte)(FrameNumber & 0xFF);                                 //Least significant byte
            byte FrameMsb = (byte)(FrameNumber >> 8);                                   //Most significant byte
            byte RevFrameLsb = ReverseByte(FrameLsb);                                   //Reversed least significant byte
            byte RevFrameMsb = ReverseByte(FrameMsb);                                   //Reversed most significant byte
            byte XorData = (byte)(FrameMsb ^ FrameLsb ^ RevFrameMsb ^ RevFrameLsb);     //XOR variable for consistency checking

            int DelayCounter = 0;

            //Calculate XOR checksum
            for (int i = 0; i < 256; i++)
            {
                XorData ^= FrameData[i];
            }

            char[] writeCmd = { 'I', 'A', 'I', '\x04' };

            //Write a frame to a Memory Card
            SendDataToPort((byte)DexCommands.SEEK, new byte[] { FrameLsb, FrameMsb }, 20);
            while (OpenedPort.BytesToRead < 4 && DelayCounter < 20)
            {
                Thread.Sleep(5);
                DelayCounter++;
            }
            ReadData = ReadDataFromPort();

            if (ReadData[0x03] == (byte)DexResponses.NOCARD)
            {
                ErrorMessage = " No memory card inserted.";
                return false;
            }

            OpenedPort.Write(writeCmd, 0, writeCmd.Length);
            OpenedPort.Write(FrameData, 0, FrameData.Length);                                                               //Save data
            OpenedPort.Write(new byte[] { XorData }, 0, 1);                                                                 //XOR Checksum

            //Wait for the buffer to fill
            while (OpenedPort.BytesToRead < 4 && DelayCounter < 20)
            {
                Thread.Sleep(5);
                DelayCounter++;
            }

            //Fetch DexDrive's response to the last command
            ReadData = ReadDataFromPort();

            //Check the return status (return true if all went OK)
            if (ReadData[0x3] == (byte)DexResponses.WRITE_OK || ReadData[0x3] == (byte)DexResponses.WRITE_SAME) return true;
            else return false;
        }

        //Reverse order of bits in a byte
        byte ReverseByte(byte InputByte)
        {
            byte ReturnByte = new byte();

            int i = 0;
            int j = 7;

            while (i < 8)
            {
                if ((InputByte & (1 << i)) > 0) ReturnByte |= (byte)(1 << j);

                i++;
                j--;
            }

            //Return reversed byte
            return ReturnByte;
        }
    }
}
