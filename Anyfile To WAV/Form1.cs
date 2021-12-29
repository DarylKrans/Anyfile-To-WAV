// This program is for educational purposes only
// You may modify this program and/or distribute as you please
// as long as it remains free.

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Anyfile_To_WAV
{
    public partial class Form1 : Form
    {
        const int MB = 1024 * 1024;
        readonly string dpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Output path for WAV file (currently the user desktop)
        short chan = 0;
        short bits = 0;
        public Form1()
        {
            InitializeComponent();
            int sr = 11025;
            // Fill comboboxes with selection choices
            string[] srate = new string[6];
            srate[0] = "8000";
            srate[5] = "96000";
            for (int i = 1; i < 5 ; i++)
            { srate[i] = sr.ToString(); sr += sr; }
            comboBox1.DataSource = srate;
            comboBox1.SelectedIndex = 4;
            string[] chnl = new string[2];
            chnl[0] = "Mono";
            chnl[1] = "Stereo";
            comboBox2.DataSource = chnl;
            comboBox2.SelectedIndex = 1;
            string[] bts = new string[2];
            bts[0] = "8-bit";
            bts[1] = "16-bit";
            comboBox3.DataSource = bts;
            comboBox3.SelectedIndex = 1;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Text = "Anyfile to Wav";
            if (comboBox2.SelectedIndex == 0) chan = 1;
            else chan = 2;
            if (comboBox3.SelectedIndex == 0) bits = 8;
            else bits = 16;
            short d = 8;
            openFileDialog1.ShowDialog();
            string sfile = openFileDialog1.FileName;
            string filename = Path.GetFileNameWithoutExtension(sfile) + ".wav";
            // Check if slected file actually exists. (not sure if this is needed since the file is selected from the OpenFileDialog)
            if (File.Exists(sfile))
            {
                long size = new System.IO.FileInfo(sfile).Length; // sets variable 'size' to equal size of source file
                if (size < 4294967252) // Check if file is too large -----< Planning to add support for files larger than 4gb >-  
                {
                    //      Start Conversion
                    this.Text = "Converting to WAV...";
                    //      Construct WAV file header 
                    byte[] riff = Encoding.ASCII.GetBytes("RIFF");           // 1-4   ASCII data 'RIFF'
                    uint len = (uint)(size) + 44;                            // 5-8   (uint) value equals size of entire file (data + header)
                    byte[] wave = Encoding.ASCII.GetBytes("WAVEfmt ");       // 9-16  ASCII data 'WAVEfmt ' <-- with null space
                    int st1 = 16;                                            // 17-20 (int) Length of format data 
                    short st2 = 1;                                           // 21-22 (short) Type of data. 1= PCM, 2= byte integer
                    short c = (short)(chan);                                 // 23-24 (short) Number of channels
                    int rate = int.Parse(comboBox1.SelectedText.ToString()); // 25-28 (int) sample rate
                    int sr = (rate * bits * chan) / 8;                       // 29-32 (int) (sample rate * Bits * channels) /8
                    short sr2 = (short)(bits * c / d);                       // 33-34 (short) (bits * channels) /8
                    short b = (short)(bits);                                 // 35-36 (short) bits per channel
                    byte[] dat = Encoding.ASCII.GetBytes("data");            // 37-40 ASCII data 'data'
                    uint dlen = (uint)(size);                                // 41-44 (uint) size of data chunk (the actual wave data) 
                    
                    //      Build WAV file
                    var buffer = new MemoryStream();                         // Configure variable as memory buffer
                    var write = new BinaryWriter(buffer);                    // Configure variable for binary writer
                    
                    // Write header to memory buffer
                    write.Write(riff);
                    write.Write(len);
                    write.Write(wave);
                    write.Write(st1);
                    write.Write(st2);
                    write.Write(c);
                    write.Write(rate);
                    write.Write(sr);
                    write.Write(sr2);
                    write.Write(b);
                    write.Write(dat);
                    write.Write(dlen);
                    write.Close();
                    
                    // Write memory buffer (wave file header) to byte[] array
                    byte[] nfile = buffer.ToArray();
                    
                    // Write byte array to output file (nfile)
                    File.WriteAllBytes(dpath + @"\" + filename, nfile);               // Write WAV file header
                    FileStream Stream = new(sfile, FileMode.Open, FileAccess.Read);   // Open source file for read only
                    FileStream Dest = new(dpath + @"\" + filename, FileMode.Append);  // Open Destination file for append
                    
                    // Construct file in 1mb blocks until completed
                    try
                    {
                        int length = MB;
                        uint W = (uint)(dlen / MB);                          // sets variable W to equal # of times file can be broken into 1mb sections
                        for (uint i = 0; i <= W; i++)                        // for loop to process file read/write from start to finish
                        {
                            if (i == W) { length = (int)(dlen - (W * MB)); } // sets variable 'MB' to equal the remainder of the source file when/if < 1MB
                            byte[] buff = new byte[length];                  // sets variable 'buff' array size to the value of 'MB'
                            Stream.Seek(i * MB, SeekOrigin.Begin);           // seeks file location in source file
                            Stream.Read(buff, 0, length);                    // reads data into variable 'buff'
                            Dest.Write(buff, 0, buff.Length);                // writes data from 'buff' to the end of the destionation file (appends) 
                            label5.Text = "Progress : " + (i * MB).ToString("N0") + " of " + size.ToString("N0");
                            label5.Refresh();
                        }
                        Stream.Close();
                        Dest.Close();
                        label5.Text = "";
                    }
                    catch { }
                    this.Text = "Anyfile to WAV";
                }
                else this.Text = "File exceeds 4gb limit!";
            }
        }
    }
}

