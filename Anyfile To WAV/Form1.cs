using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Anyfile_To_WAV
{
    public partial class Form1 : Form
    {
        int MB = 1024 * 1024;
        readonly string dpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        int rate = 0;
        short chan = 0;
        short bits = 0;

        public Form1()
        {
            InitializeComponent();
            string[] ratea = new string[3];
            ratea[0] = "44100";
            ratea[1] = "22050";
            ratea[2] = "11025";
            comboBox1.DataSource = ratea;
            string[] chana = new string[2];
            chana[0] = "Mono";
            chana[1] = "Stereo";
            comboBox2.DataSource = chana;
            comboBox2.SelectedIndex = 1;
            string[] bitsa = new string[2];
            bitsa[0] = "8-bit";
            bitsa[1] = "16-bit";
            comboBox3.DataSource = bitsa;
            comboBox3.SelectedIndex = 1;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Text = "Anyfile to Wav";
            // open
            if (comboBox1.SelectedIndex == 0) rate = 44100;
            if (comboBox1.SelectedIndex == 1) rate = 22050;
            if (comboBox1.SelectedIndex == 2) rate = 11050;
            if (comboBox2.SelectedIndex == 0) chan = 1;
            else chan = 2;
            if (comboBox3.SelectedIndex == 0) bits = 8;
            else bits = 16;
            short b = Convert.ToInt16(bits);
            short c = Convert.ToInt16(chan);
            short d = Convert.ToInt16(8);
            openFileDialog1.ShowDialog();
            string sfile = openFileDialog1.FileName;
            string filename = Path.GetFileNameWithoutExtension(sfile) + ".wav";
            if (File.Exists(sfile))
            {
                //      Check if file is too large        
                long size = new System.IO.FileInfo(sfile).Length;
                if (size < 4294967252)
                {
                    //      Start Conversion
                    this.Text = "Converting to WAV...";
                    //      Construct WAV file header 
                    byte[] riff = Encoding.ASCII.GetBytes("RIFF");      // 1-4
                    uint len = Convert.ToUInt32(size) + 44;             // 5-8
                    byte[] wave = Encoding.ASCII.GetBytes("WAVEfmt ");  // 9-16
                    uint st1 = 16;                                      // 17-20
                    short st2 = 1;                                      // 21-22 -  23-24 (assigned to short 'c' from int 'chan') - 25-28 (assigned to 'rate')
                    int sr = (rate * bits * chan) / 8;                  // 29-32 (sample rate * Bits * channels) /8
                    short sr2 = (short)(b * c / d);                     // 33-34 (bits * channels) /8  35-36 (assigned to short 'b' from int 'bits')
                    byte[] dat = Encoding.ASCII.GetBytes("data");       // 37-40
                    uint dlen = Convert.ToUInt32(size);                 // 41-44
                    //      Build WAV file
                    var buffer = new MemoryStream();                                                // Configure variable as memory buffer
                    var write = new BinaryWriter(buffer);                                           // Configure variable for binary writer
                    write.Write(riff); write.Write(len); write.Write(wave);                         // Write header to memory buffer
                    write.Write(st1); write.Write(st2); write.Write(c);                             //    
                    write.Write(rate); write.Write(sr); write.Write(sr2);                           //
                    write.Write(b); write.Write(dat); write.Write(dlen); write.Close();             //
                    byte[] nfile = buffer.ToArray();                                                // Write memory buffer to byte array
                    File.WriteAllBytes(dpath + @"\" + filename, nfile);                             // Write WAV file header
                    FileStream Stream = new(sfile, FileMode.Open, FileAccess.Read);                 // Open source file for read only
                    FileStream Dest = new(dpath + @"\" + filename, FileMode.Append);                // Open Destination file for append
                    //      Construct file in 1mb blocks until completed
                    uint W = (uint)(dlen / MB);
                    try
                    {
                        for (uint i = 0; i <= W; i++)
                        {
                            if (i == W) { MB = (int)(dlen - (W * MB)); }
                            byte[] buff = new byte[MB];
                            Stream.Seek(i * MB, SeekOrigin.Begin);
                            Stream.Read(buff, 0, MB);
                            Dest.Write(buff, 0, buff.Length);
                        }
                        Stream.Close();
                        Dest.Close();
                    }
                    catch { }
                }
                else this.Text = "File exceeds 4gb limit!";
            }
        }
    }
}

