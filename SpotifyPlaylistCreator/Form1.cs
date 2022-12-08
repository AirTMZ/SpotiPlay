using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using SpotifyPlaylistCreator.Properties;
using System.Text.RegularExpressions;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace SpotifyPlaylistCreator
{
    public partial class SpotiPlay : Form
    {
        // global returns for top 6
        string href = null;
        string songName = null;
        string artistName = null;
        string uri = null;
        string time = null;
        string imageUrl = null;
        int totalUsers = 1;

        List<string> top6List = new List<string>();

        List<string> user1 = new List<string>();
        List<string> user2 = new List<string>();
        List<string> user3 = new List<string>();
        List<string> user4 = new List<string>();
        List<string> user5 = new List<string>();


        List<string> artistIds = new List<string>();



        string newUserName = null;
        string newUserId = null;
        string newUserPhoto = null;
        List<string> users = new List<string>();

        //Splash screen
        private void SplashTimer_Tick(object sender, EventArgs e)
        {
            //splash screen
            this.Size = new Size(1703, 953);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 50, 50));
            this.CenterToScreen();
            logosplash.Visible = false;

            LogoImage.Show(); UsernameInput.Show(); UsernameBox.Show(); TokenInput.Show(); TokenBox.Show(); tokenButton.Show(); nextButton.Show(); MinimiseButton.Show(); ExitButton.Show(); TopBanner.Show();
            token_label_back.Hide(); token_label.Hide();

            SplashTimer.Stop();
        }

        public SpotiPlay()
        {
            InitializeComponent();

            //set size
            this.Size = new Size(300, 300);
            this.FormBorderStyle = FormBorderStyle.None;
            //set colour
            this.BackColor = Color.FromArgb(17, 17, 18);
            //set corners
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 50, 50));
            logosplash.Enabled = true;
            SplashTimer.Start();
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(Properties.Resources.jingle);
            player.Play();
        }


        //Login Boxes

        private void UsernameInput_Enter(object sender, EventArgs e)
        {
            tabSaver.Hide();
            if (UsernameInput.Text == "Username‎")
            {
                UsernameInput.Text = "";
            }

            UsernameInput.ForeColor = Color.White;
            if (TokenInput.Text == "")
            {
                TokenInput.ForeColor = Color.FromArgb(156, 163, 169);
                TokenInput.PasswordChar = '\0';
                TokenInput.Text = "Token‎";
            }
        }
        private void TokenInput_Enter(object sender, EventArgs e)
        {
            tabSaver.Hide();
            if (TokenInput.Text == "Token‎")
            {
                TokenInput.Text = "";
            }

            TokenInput.PasswordChar = '*';
            TokenInput.ForeColor = Color.White;


            if (UsernameInput.Text == "")
            {
                UsernameInput.ForeColor = Color.FromArgb(156, 163, 169);
                UsernameInput.Text = "Username‎";
            }
        }


        //Token Gen
        private void tokenButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Generate a token with all permissions, excluding:\nplaylist-read-public");
            System.Diagnostics.Process.Start("http://shorturl.at/akAV4");
        }

        //pressing enter to force the next button
        private void UsernameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NextButton_Click(UsernameInput, EventArgs.Empty);
            }
        }

        private void TokenInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NextButton_Click(TokenInput, EventArgs.Empty);
            }
        }

        //Exit
        private void ExitButton_Click(object sender, EventArgs e)
        {
            EndTimer.Start();
        }

        private void EndTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0)
            {
                System.Windows.Forms.Application.Exit();
            }
            Opacity -= 0.08;
        }

        private void ExitButton_MouseHover(object sender, EventArgs e)
        {
            ExitButton.Image = Properties.Resources.exit_select;
        }

        private void ExitButton_MouseLeave(object sender, EventArgs e)
        {
            ExitButton.Image = Properties.Resources.exit_deselect;
        }


        //Minimise
        private void MinimiseButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MinimiseButton_MouseHover(object sender, EventArgs e)
        {
            MinimiseButton.Image = Properties.Resources.minimise_select;
        }

        private void MinimiseButton_MouseLeave(object sender, EventArgs e)
        {
            MinimiseButton.Image = Properties.Resources.minimise_deselect;
        }

        //banner movement
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void top_banner_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        //Curved Corners initialise
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);



    //the real code starts
        private void NextButton_Click(object sender, EventArgs e)
        {
            Process cmd = new Process();
            if (UsernameInput.Text == "debug")
            {
                string text = File.ReadAllText("debugProfileData.txt");
                defaultCurlData(text, cmd);
                return;
            }
            if (UsernameInput.Text == "" || UsernameInput.Text == "Username‎" || TokenInput.Text == "" || TokenInput.Text == "Token‎")
            {
                errorLabel.Text = ("Sorry bud. You have to input something first");
                errorLabel.Show();
                return;
            }
            //run the curl request using cmd
            
            cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();

            //the actual request
            cmd.StandardInput.WriteLine($"curl -X \"GET\" \"https://api.spotify.com/v1/me\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {TokenInput.Text}\"");

            cmd.StandardInput.Flush(); cmd.StandardInput.Close();

            // removes clutter from response
            
            string curlResponse = (cmd.StandardOutput.ReadToEnd());
            cmd.WaitForExit();
            try
            {
                curlResponse = curlResponse.Substring(curlResponse.IndexOf('{'));
            }
            catch (ArgumentOutOfRangeException)
            {
                errorLabel.Text = ("Sorry bud. Your don't have the correct perms");
                errorLabel.Show();
                return;
            }
            string remove = curlResponse.Substring(curlResponse.LastIndexOf('}') + 1);
            curlResponse = curlResponse.Replace(remove, string.Empty);
            curlResponse = $"[{curlResponse}]";

            defaultCurlData(curlResponse, cmd);
        }

        private void defaultCurlData(string curlResponse, Process cmd)
        {
            if (curlResponse.Length < 100)
            {
                errorLabel.Text = ("Sorry bud. Your token has expired");
                errorLabel.Show();
            }
            else
            {
                List<jsonCurlData> records = JsonConvert.DeserializeObject<List<jsonCurlData>>(curlResponse);



                foreach (jsonCurlData record in records)
                {
                    userLabel1.Text = record.display_name;
                    token_label.Text = record.id;
                    try
                    {
                        profilePhoto1.ImageLocation = record.images[0].url;
                        if (UsernameInput.Text == "debug")
                        {
                            songArt1.ImageLocation = record.images[0].url;
                            songArt2.ImageLocation = record.images[0].url;
                            songArt3.ImageLocation = record.images[0].url;
                            songArt4.ImageLocation = record.images[0].url;
                            songArt5.ImageLocation = record.images[0].url;
                            songArt6.ImageLocation = record.images[0].url;
                        }
                    }
                    catch
                    {
                        profilePhoto1.Image = Resources.spotiplayLogo2;
                    }

                }


                if (userLabel1.Text == UsernameInput.Text || UsernameInput.Text == token_label.Text)
                {
                    //username correct
                    LogoImage.Hide(); UsernameInput.Hide(); UsernameBox.Hide(); TokenInput.Hide(); TokenBox.Hide(); tokenButton.Hide(); nextButton.Hide(); errorLabel.Hide();
                    token_label_back.Show(); token_label.Show();

                    //contributing window default
                    contributersLabel.Show(); userLabel1.Show(); profilePhoto1.Show();
                    userBack1.Show();
                    contributingBack.Show();



                    //analysis window
                    percentLabel1.Show(); percentLabel2.Show(); percentLabel3.Show(); percentLabel4.Show(); percentLabel5.Show();
                    percentBar1.Show(); percentBar2.Show(); percentBar3.Show(); percentBar4.Show(); percentBar5.Show();
                    genreLabel1.Show(); genreLabel2.Show(); genreLabel3.Show(); genreLabel4.Show(); genreLabel5.Show();
                    genreBack1.Show(); genreBack2.Show(); genreBack3.Show(); genreBack4.Show(); genreBack5.Show();

                    addUserButton.Show(); exportButton.Show();
                    analysisLabel.Show(); analysisBack.Show();

                    defaultTopSix(cmd);
                    addUserButton.Show(); exportButton.Show();

                    users.Add(token_label.Text);

                    percentBar1.Size = new System.Drawing.Size(158, 65);
                    percentBar1.Location = new Point(172, 325);

                    percentLabel1.Size = new System.Drawing.Size(28, 19);
                    percentLabel1.Location = new Point(300, 349);

                    percentBar2.Size = new System.Drawing.Size(158, 65);
                    percentBar2.Location = new Point(172, 413);

                    percentLabel2.Size = new System.Drawing.Size(28, 19);
                    percentLabel2.Location = new Point(300, 437);

                    percentBar3.Size = new System.Drawing.Size(158, 65);
                    percentBar3.Location = new Point(172, 503);

                    percentLabel3.Size = new System.Drawing.Size(28, 19);
                    percentLabel3.Location = new Point(300, 527);

                    percentBar4.Size = new System.Drawing.Size(158, 65);
                    percentBar4.Location = new Point(172, 593);

                    percentLabel4.Size = new System.Drawing.Size(28, 19);
                    percentLabel4.Location = new Point(300, 617);

                    percentBar5.Size = new System.Drawing.Size(158, 65);
                    percentBar5.Location = new Point(172, 683);

                    percentLabel5.Size = new System.Drawing.Size(28, 19);
                    percentLabel5.Location = new Point(300, 707);

                    if (UsernameInput.Text != "debug")
                    {
                        genreAnalysis();
                    }


                }
                else
                {
                    errorLabel.Text = ("Incorrect Username");
                    errorLabel.Show();
                }
            }
        }

        private void defaultTopSix(Process cmd)
        {
            href = "https://api.spotify.com/v1/me/top/tracks?time_range=short_term&limit=1";
            string check = "";
            if (UsernameInput.Text != "debug")
            {
                check = topSix(TokenInput.Text, cmd, href);
                if (check == "none")
                {
                    playlistNameLabel.Show();
                    stageBack.Show();
                    MessageBox.Show("Not enough song data for this user. Please try a different account. Logging out");
                    Application.Exit();
                    return;
                }
                songName1.Text = songName;
                artistName1.Text = artistName;
                timeLabel1.Text = time;
                songArt1.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);
                topSix(TokenInput.Text, cmd, href);

                songName2.Text = songName;
                artistName2.Text = artistName;
                timeLabel2.Text = time;
                songArt2.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);

                topSix(TokenInput.Text, cmd, href);

                songName3.Text = songName;
                artistName3.Text = artistName;
                timeLabel3.Text = time;
                songArt3.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);

                topSix(TokenInput.Text, cmd, href);

                songName4.Text = songName;
                artistName4.Text = artistName;
                timeLabel4.Text = time;
                songArt4.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);

                topSix(TokenInput.Text, cmd, href);

                songName5.Text = songName;
                artistName5.Text = artistName;
                timeLabel5.Text = time;
                songArt5.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);

                topSix(TokenInput.Text, cmd, href);

                songName6.Text = songName;
                artistName6.Text = artistName;
                timeLabel6.Text = time;
                songArt6.ImageLocation = imageUrl;

                user1.Add(songName); user1.Add(artistName); user1.Add(time); user1.Add(imageUrl); user1.Add(uri);

                top6List.AddRange(user1);
            }





            songName1.Show(); artistName1.Show(); timeLabel1.Show(); songArt1.Show();
            songBack1.Show();
            songName2.Show(); artistName2.Show(); timeLabel2.Show(); songArt2.Show();
            songBack2.Show();
            songName3.Show(); artistName3.Show(); timeLabel3.Show(); songArt3.Show();
            songBack3.Show();
            songName4.Show(); artistName4.Show(); timeLabel4.Show(); songArt4.Show();
            songBack4.Show();
            songName5.Show(); artistName5.Show(); timeLabel5.Show(); songArt5.Show();
            songBack5.Show();
            songName6.Show(); artistName6.Show(); timeLabel6.Show(); songArt6.Show();
            songBack6.Show();

            playlistNameLabel.Show();
            stageBack.Show();
        }

        public class jsonCurlData
        {
            public string display_name { get; set; }
            public string id { get; set; }
            public List<Images> images { get; set; }
        }
        public class Images
        {
            public string url { get; set; }
        }

        private string topSix(string token, Process cmd, string apicommand)
        {
            try
            {
                cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();

                //the actual request
                cmd.StandardInput.WriteLine($"curl -X \"GET\" \"{apicommand}\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {token}\"");

                cmd.StandardInput.Flush(); cmd.StandardInput.Close();

                // removes clutter from response
                
                string curlResponse = (cmd.StandardOutput.ReadToEnd());
                cmd.WaitForExit();


                curlResponse = curlResponse.Substring(curlResponse.IndexOf('{'));
                string remove = curlResponse.Substring(curlResponse.LastIndexOf('}') + 1);
                curlResponse = curlResponse.Replace(remove, string.Empty);
                curlResponse = $"[{curlResponse}]";
                List<topSixCurl> records2 = JsonConvert.DeserializeObject<List<topSixCurl>>(curlResponse);
                foreach (topSixCurl record2 in records2)
                {
                    songName = record2.items[0].name; //song name
                    artistName = record2.items[0].artists[0].name;//artist name
                    uri = record2.items[0].uri; //uri
                    href = record2.next;
                    time = $"{msToHuman(record2.items[0].duration_ms)}";
                    imageUrl = record2.items[0].album.images[0].url; //image url
                    artistIds.Add(record2.items[0].artists[0].id);
                }
            }
            catch
            {
                return "none";
            }
            return "fine";
            
        }
        public class topSixCurl
        {
            public List<Item> items { get; set; } //to item
            public string next { get; set; } //test
        }
        public class Item
        {
            public Album album { get; set; } //to album
            public List<Artist> artists { get; set; } //to artist
            public int duration_ms { get; set; } //duration in ms
            public string uri { get; set; } //uri
            public string name { get; set; } //song name
        }
        public class Album
        {
            public List<Image> images { get; set; } //to image
        }
        
        public class Image
        {
            public string url { get; set; } // image url
        }
        public class Artist
        {
            public string name { get; set; } //artist name
            public string id { get; set; } //artist name
        }



        //song time formatter
        public static string msToHuman(long ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            if (t.Hours > 0)
            {
                return $"{t.Hours}:{t.Minutes}";
            }

            else if (t.Minutes > 0)
            {
                if (t.Seconds < 10)
                {
                    return $"{t.Minutes}:0{t.Seconds}";
                }
                else
                {
                    return $"{t.Minutes}:{t.Seconds}";
                }
                
            }

            else if (t.Seconds > 0)
            {
                return $"{t.Seconds}";
            }

            else
            {
                return $"{t.Milliseconds}ms";
            }
        }



        //time for pain
        private void addUserButton_Click(object sender, EventArgs e)
        {
            newUserExit.Show(); newUserInput.Show(); newUserEnter.Show(); newUserLabel.Show();
            newUserBack.Show();
            newUserBack.BringToFront();
            newUserExit.BringToFront(); newUserInput.BringToFront(); newUserEnter.BringToFront(); newUserLabel.BringToFront();

        }

        private void newUserEnter_Click(object sender, EventArgs e)
        {
            string token = null;

            token = newUserInput.Text;
            newUserEnter.Hide();

            string check = getUserDetails(token);
            if (check != "no")
            {
                if (users.Contains(newUserId) == true)
                {
                    MessageBox.Show("user already in group");
                    newUserInput.Text = "";
                    newUserEnter.Show();
                    return;
                }
                else
                {
                    newUserExit_Click(newUserEnter, EventArgs.Empty);
                    newUserInput.Text = "";
                    users.Add(newUserId);
                    addUserSongs(token);

                    percentBar1.Size = new System.Drawing.Size(158, 65);
                    percentBar1.Location = new Point(172, 325);

                    percentLabel1.Size = new System.Drawing.Size(28, 19);
                    percentLabel1.Location = new Point(300, 349);

                    percentBar2.Size = new System.Drawing.Size(158, 65);
                    percentBar2.Location = new Point(172, 413);

                    percentLabel2.Size = new System.Drawing.Size(28, 19);
                    percentLabel2.Location = new Point(300, 437);

                    percentBar3.Size = new System.Drawing.Size(158, 65);
                    percentBar3.Location = new Point(172, 503);

                    percentLabel3.Size = new System.Drawing.Size(28, 19);
                    percentLabel3.Location = new Point(300, 527);

                    percentBar4.Size = new System.Drawing.Size(158, 65);
                    percentBar4.Location = new Point(172, 593);

                    percentLabel4.Size = new System.Drawing.Size(28, 19);
                    percentLabel4.Location = new Point(300, 617);

                    percentBar5.Size = new System.Drawing.Size(158, 65);
                    percentBar5.Location = new Point(172, 683);

                    percentLabel5.Size = new System.Drawing.Size(28, 19);
                    percentLabel5.Location = new Point(300, 707);
                    genreAnalysis();
                }
            }
        }

        private void newUserExit_Click(object sender, EventArgs e)
        {
            newUserExit.Hide(); newUserInput.Hide(); newUserEnter.Hide(); newUserLabel.Hide();
            newUserBack.Hide();
        }

        private void newUserExit_MouseHover(object sender, EventArgs e)
        {
            newUserExit.Image = Properties.Resources.exit_select;
        }

        private void newUserExit_MouseLeave(object sender, EventArgs e)
        {
            newUserExit.Image = Properties.Resources.exit_deselect;
        }

        private string getUserDetails(string token)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();

            

            //the actual request
            cmd.StandardInput.WriteLine($"curl -X \"GET\" \"https://api.spotify.com/v1/me\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {token}\"");
            cmd.StandardInput.Flush(); cmd.StandardInput.Close();

            // removes clutter from response
            string curlResponse = (cmd.StandardOutput.ReadToEnd());

            cmd.WaitForExit();
            curlResponse = curlResponse.Substring(curlResponse.IndexOf('{'));
            string remove = curlResponse.Substring(curlResponse.LastIndexOf('}') + 1);
            curlResponse = curlResponse.Replace(remove, string.Empty);
            curlResponse = $"[{curlResponse}]";
            if (curlResponse.Length < 150)
            {
                MessageBox.Show("Sorry bud. The token has expired");
                newUserInput.Text = "";
                return "no";
            }
            else
            {
                List<jsonCurlData> records = JsonConvert.DeserializeObject<List<jsonCurlData>>(curlResponse);



                foreach (jsonCurlData record in records)
                {
                    newUserName = record.display_name;
                    newUserId = record.id;
                    try
                    {
                        newUserPhoto = record.images[0].url;
                    }
                    catch
                    {
                        newUserPhoto = "none";
                    }

                }

            }
            return "yes";
        }

        private void addUserSongs(string token)
        {
            Process cmd = new Process();
            if (totalUsers == 1)
            {
                href = "https://api.spotify.com/v1/me/top/tracks?time_range=short_term&limit=1";
                string check = topSix(token, cmd, href);

                if (check == "none")
                {
                    playlistNameLabel.Show();
                    stageBack.Show();
                    MessageBox.Show("Not enough song data for this user. Please try a different account");
                    return;
                }

                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);
                topSix(token, cmd, href);
                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);
                topSix(token, cmd, href);
                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);
                topSix(token, cmd, href);
                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);
                topSix(token, cmd, href);
                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);
                topSix(token, cmd, href);
                user2.Add(songName); user2.Add(artistName); user2.Add(time); user2.Add(imageUrl); user2.Add(uri);

                top6List.Clear();



                int i = 0;
                while (i <= 14)
                {
                    top6List.Add(user1[i]);
                    i++;
                }
                i = 0;
                while (i <= 14)
                {
                    top6List.Add(user2[i]);
                    i++;
                }

                profilePhoto2.ImageLocation = newUserPhoto;
                userLabel2.Text = newUserName;
                userLabel2.Show();
                profilePhoto2.Show();
                userBack2.Show();
                userBack2.BringToFront();
                userLabel2.BringToFront();
                profilePhoto2.BringToFront();

                totalUsers++;
                displayTop6();


            }
            else if (totalUsers == 2)
            {

                profilePhoto3.ImageLocation = newUserPhoto;
                userLabel3.Text = newUserName;


                href = "https://api.spotify.com/v1/me/top/tracks?time_range=short_term&limit=1";
                string check = topSix(token, cmd, href);
                if (check == "none")
                {
                    playlistNameLabel.Show();
                    stageBack.Show();
                    MessageBox.Show("Not enough song data for this user. Please try a different account");
                    return;
                }
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);
                topSix(token, cmd, href);
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);
                topSix(token, cmd, href);
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);
                topSix(token, cmd, href);
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);
                topSix(token, cmd, href);
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);
                topSix(token, cmd, href);
                user3.Add(songName); user3.Add(artistName); user3.Add(time); user3.Add(imageUrl); user3.Add(uri);

                top6List.Clear();

                int i = 0;
                while (i <= 9)
                {
                    top6List.Add(user1[i]);
                    i++;
                }
                i = 0;
                while (i <= 9)
                {
                    top6List.Add(user2[i]);
                    i++;
                }
                i = 0;
                while (i <= 9)
                {
                    top6List.Add(user3[i]);
                    i++;
                }

                profilePhoto3.ImageLocation = newUserPhoto;
                userLabel3.Text = newUserName;
                userLabel3.Show();
                profilePhoto3.Show();
                userBack3.Show();
                userBack3.BringToFront();
                userLabel3.BringToFront();
                profilePhoto3.BringToFront();

                displayTop6();

                totalUsers++;
            }
            else if (totalUsers == 3)
            {

                profilePhoto4.ImageLocation = newUserPhoto;
                userLabel4.Text = newUserName;


                href = "https://api.spotify.com/v1/me/top/tracks?time_range=short_term&limit=1";
                string check = topSix(token, cmd, href);
                if (check == "none")
                {
                    playlistNameLabel.Show();
                    stageBack.Show();
                    MessageBox.Show("Not enough song data for this user. Please try a different account");
                    return;
                }
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);
                topSix(token, cmd, href);
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);
                topSix(token, cmd, href);
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);
                topSix(token, cmd, href);
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);
                topSix(token, cmd, href);
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);
                topSix(token, cmd, href);
                user4.Add(songName); user4.Add(artistName); user4.Add(time); user4.Add(imageUrl); user4.Add(uri);

                top6List.Clear();

                int i = 0;
                while (i <= 9)
                {
                    top6List.Add(user1[i]);
                    i++;
                }
                i = 0;
                while (i <= 9)
                {
                    top6List.Add(user2[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user3[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user4[i]);
                    i++;
                }
                top6List.Add(songName); top6List.Add(artistName); top6List.Add(time); top6List.Add(imageUrl); top6List.Add(uri);

                profilePhoto4.ImageLocation = newUserPhoto;
                userLabel4.Text = newUserName;
                userLabel4.Show();
                profilePhoto4.Show();
                userBack4.Show();
                userBack4.BringToFront();
                userLabel4.BringToFront();
                profilePhoto4.BringToFront();

                displayTop6();

                totalUsers++;
            }
            else if (totalUsers == 4)
            {

                profilePhoto5.ImageLocation = newUserPhoto;
                userLabel5.Text = newUserName;


                href = "https://api.spotify.com/v1/me/top/tracks?time_range=short_term&limit=1";
                string check = topSix(token, cmd, href);
                if (check == "none")
                {
                    playlistNameLabel.Show();
                    stageBack.Show();
                    MessageBox.Show("Not enough song data for this user. Please try a different account");
                    return;
                }
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);
                topSix(token, cmd, href);
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);
                topSix(token, cmd, href);
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);
                topSix(token, cmd, href);
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);
                topSix(token, cmd, href);
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);
                topSix(token, cmd, href);
                user5.Add(songName); user5.Add(artistName); user5.Add(time); user5.Add(imageUrl); user5.Add(uri);

                top6List.Clear();

                int i = 0;
                while (i <= 9)
                {
                    top6List.Add(user1[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user2[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user3[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user4[i]);
                    i++;
                }
                i = 0;
                while (i <= 4)
                {
                    top6List.Add(user5[i]);
                    i++;
                }

                profilePhoto5.ImageLocation = newUserPhoto;
                userLabel5.Text = newUserName;
                userLabel5.Show();
                profilePhoto5.Show();
                userBack5.Show();
                userBack5.BringToFront();
                userLabel5.BringToFront();
                profilePhoto5.BringToFront();

                displayTop6();

                totalUsers++;
            }
            else if (totalUsers == 5) // no more
            {
                MessageBox.Show("User limit reached");
            }
        }

        private void displayTop6()
        {
            
            songName1.Text = top6List[0]; //song name for first
            songName2.Text = top6List[5];
            songName3.Text = top6List[10];
            songName4.Text = top6List[15];
            songName5.Text = top6List[20];
            songName6.Text = top6List[25];

            artistName1.Text = top6List[1]; //artist name
            artistName2.Text = top6List[6];
            artistName3.Text = top6List[11];
            artistName4.Text = top6List[16];
            artistName5.Text = top6List[21];
            artistName6.Text = top6List[26];

            timeLabel1.Text = top6List[2]; //time
            timeLabel2.Text = top6List[7];
            timeLabel3.Text = top6List[12];
            timeLabel4.Text = top6List[17];
            timeLabel5.Text = top6List[22];
            timeLabel6.Text = top6List[27];

            songArt1.ImageLocation = top6List[3]; //image location
            songArt2.ImageLocation = top6List[8];
            songArt3.ImageLocation = top6List[13];
            songArt4.ImageLocation = top6List[18];
            songArt5.ImageLocation = top6List[23];
            songArt6.ImageLocation = top6List[28];
        }

        //more pain
        
        private void exportButton_Click(object sender, EventArgs e)
        {
            
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();

            //the actual request
            
            cmd.StandardInput.WriteLine($"curl -X \"POST\" \"https://api.spotify.com/v1/users/{token_label.Text}/playlists\" --data \"{{\\\"name\\\":\\\"{playlistNameLabel.Text}\\\",\\\"description\\\":\\\"A playlist made with SpotiPlay\\\",\\\"public\\\":true}}\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {TokenInput.Text}\"");
            cmd.StandardInput.Flush(); cmd.StandardInput.Close();

            //catch the playlist return data
            string curlResponse = (cmd.StandardOutput.ReadToEnd());

            cmd.WaitForExit();
            curlResponse = curlResponse.Substring(curlResponse.IndexOf('{') + 1);
            curlResponse = curlResponse.Substring(curlResponse.IndexOf('{'));
            string remove = curlResponse.Substring(curlResponse.LastIndexOf('}') + 1);
            curlResponse = curlResponse.Replace(remove, string.Empty);
            curlResponse = $"[{curlResponse}]";

            List<playlistId> records = JsonConvert.DeserializeObject<List<playlistId>>(curlResponse);

            string id = null;

            foreach (playlistId record in records)
            {
               id = record.id;
            }

            string uriComplete = compileUri();


            sendPlaylistData(id, uriComplete);
        }

        private void sendPlaylistData(string id, string uriComplete)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();



            //the actual request
            string command = $"curl -X \"POST\" \"https://api.spotify.com/v1/playlists/{id}/tracks\" --data \"{uriComplete}\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {TokenInput.Text}\"";
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush(); cmd.StandardInput.Close();

            //catch the playlist return data
            string curlResponse = (cmd.StandardOutput.ReadToEnd());



            cmd.WaitForExit();
            MessageBox.Show("Playlist has been exported Successfully :)");

            System.Windows.Forms.Clipboard.SetText($"https://open.spotify.com/playlist/{id}");

        }



        private string compileUri()
        {
            string str = string.Empty;
            List<string> allPlaylists = new List<string>();

            try
            {
                int i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(top6List[i]);
                    i = i + 5;
                }
                i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(user1[i]);
                    i = i + 5;
                }
                i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(user2[i]);
                    i = i + 5;
                }
                i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(user3[i]);
                    i = i + 5;
                }
                i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(user4[i]);
                    i = i + 5;
                }
                i = 4;
                while (i <= 29)
                {
                    allPlaylists.Add(user5[i]);
                    i = i + 5;
                }
            }
            catch
            {
            }




            var noDupes = allPlaylists.Distinct().ToList();

            foreach (var item in noDupes)
            {
                str = str + item + "\\\",\\\"";

            }
            str = str.Remove(str.Length - 5);
            str = Regex.Replace(str, @"\s+", "");
            str = $"\\\"uris\\\":[\\\"{str}\\\"]";
            str = "{" + str + "}";
            return str;
        }




        public class playlistId
        {
            public string id { get; set; }
        }





        //genre work

        private void genreAnalysis()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe"; cmd.StartInfo.RedirectStandardInput = true; cmd.StartInfo.RedirectStandardOutput = true; cmd.StartInfo.CreateNoWindow = true; cmd.StartInfo.UseShellExecute = false; cmd.Start();

            string str = "";

            foreach (var item in artistIds)
            {
                str = str + item + "%2C";
            }
            str = str.Remove(str.Length - 3);

            string command = $"curl -X \"GET\" \"https://api.spotify.com/v1/artists?ids={str}\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"Authorization: Bearer {TokenInput.Text}\"";
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush(); cmd.StandardInput.Close();

            //catch the playlist return data
            string curlResponse = (cmd.StandardOutput.ReadToEnd());
            

            cmd.WaitForExit();



            curlResponse = curlResponse.Substring(curlResponse.IndexOf('{'));
            string remove = curlResponse.Substring(curlResponse.LastIndexOf('}') + 1);
            curlResponse = curlResponse.Replace(remove, string.Empty);
            curlResponse = $"[{curlResponse}]";


            List<string> genreList = new List<string>();

            List<genreList> records = JsonConvert.DeserializeObject<List<genreList>>(curlResponse);


            foreach (genreList record in records)
            {
                try
                {
                    int i = 0;
                    while (i >= 0)
                    {
                        genreList.AddRange(record.artists[i].genres);
                        i++;
                    }
                }
                catch
                {
                }
            }

            string genres = string.Join(",", genreList);

            List<string> genreSave = new List<string>();
            genreSave.AddRange(genreList);
            List<string> top5Genre = new List<string>();

            try
            {
                var first = genreSave.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                genreSave.RemoveAll(item => item == first);
                top5Genre.Add(first);
                var second = genreSave.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                genreSave.RemoveAll(item => item == second);
                top5Genre.Add(second);
                var third = genreSave.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                genreSave.RemoveAll(item => item == third);
                top5Genre.Add(third);
                var fourth = genreSave.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                genreSave.RemoveAll(item => item == fourth);
                top5Genre.Add(fourth);
                var fifth = genreSave.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                genreSave.RemoveAll(item => item == fifth);
                top5Genre.Add(fifth);

                top5Genre.Sort();
                genreLabel1.Text = top5Genre[0];
                genreLabel2.Text = top5Genre[1];
                genreLabel3.Text = top5Genre[2];
                genreLabel4.Text = top5Genre[3];
                genreLabel5.Text = top5Genre[4];
            }
            catch
            {

            }
            List<int> frequency = new List<int>();
            int count = 0;
            while(count <= 4)
            {
                string genresWithoutSpace = genres.Replace(" ", ",");
                List<string> genreListAll = genresWithoutSpace.Split(',').ToList();

                int mostFrequent = genreListAll.Where(x => x.Equals(top5Genre[count])).Count();
                if (mostFrequent == 0)
                {
                    mostFrequent = genreList.Where(x => x.Equals(top5Genre[count])).Count();
                }
                frequency.Add(mostFrequent);
                count++;
            }

            double total = genreList.Count;
            int percent1 = Convert.ToInt32(Math.Round(frequency[0] / total * 100));
            int percent2 = Convert.ToInt32(Math.Round(frequency[1] / total * 100));
            int percent3 = Convert.ToInt32(Math.Round(frequency[2] / total * 100));
            int percent4 = Convert.ToInt32(Math.Round(frequency[3] / total * 100));
            int percent5 = Convert.ToInt32(Math.Round(frequency[4] / total * 100));


            percentLabel1.Text = $"{percent1}%";
            percentLabel2.Text = $"{percent2}%";
            percentLabel3.Text = $"{percent3}%";
            percentLabel4.Text = $"{percent4}%";
            percentLabel5.Text = $"{percent5}%";

            percentBar1.Size = new System.Drawing.Size(158, 65);
            percentBar1.Location = new Point(172, 325);

            percentBar2.Size = new System.Drawing.Size(158, 65);
            percentBar2.Location = new Point(172, 413);

            percentBar3.Size = new System.Drawing.Size(158, 65);
            percentBar3.Location = new Point(172, 503);

            percentBar4.Size = new System.Drawing.Size(158, 65);
            percentBar4.Location = new Point(172, 593);

            percentBar5.Size = new System.Drawing.Size(158, 65);
            percentBar5.Location = new Point(172, 683);

            changePercentageImage(percentBar1, percentLabel1, percent1);
            changePercentageImage(percentBar2, percentLabel2, percent2);
            changePercentageImage(percentBar3, percentLabel3, percent3);
            changePercentageImage(percentBar4, percentLabel4, percent4);
            changePercentageImage(percentBar5, percentLabel5, percent5);
        }

        private void changePercentageImage(PictureBox image, TextBox textBox, int percent)
        {

            double n = 23 + ((percent - 1) * 1.35);
            percent = Convert.ToInt32(Math.Round(n));
            image.Size = new System.Drawing.Size(percent, image.Height);
            image.Location = new Point(image.Location.X + (158 - percent), image.Location.Y);
            int count = 0;
            foreach (char c in textBox.Text)
            {
                count++;
            }
            if (count == 3)
            {
                textBox.Size = new System.Drawing.Size(textBox.Width + 12, textBox.Height);
                textBox.Location = new Point(textBox.Location.X - 10, textBox.Location.Y);

            }
            if (count == 4)
            {
                textBox.Size = new System.Drawing.Size(textBox.Width + 24, textBox.Height);
                textBox.Location = new Point(textBox.Location.X - 22, textBox.Location.Y);

            }
        }

        public class genreArtist
        {
            public List<string> genres { get; set; }
        }


        public class genreList
        {
            public List<genreArtist> artists { get; set; }
        }

        private void playlistNameLabel_TextChanged(object sender, EventArgs e)
        {
            char[] s = playlistNameLabel.Text.ToCharArray();
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {

                if ((s[i] >= 'A' && s[i] <= 'Z')
                        || (s[i] >= 'a' && s[i] <= 'z'))
                {
                    s[j] = s[i];
                    j++;
                }
            }
            playlistNameLabel.Text = (String.Join("", s).Substring(0, j));
        }
    }
}