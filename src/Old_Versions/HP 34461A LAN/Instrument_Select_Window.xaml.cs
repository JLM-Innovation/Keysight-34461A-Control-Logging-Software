using HP_34461A_LAN.InstrumentSession;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace HP_34461A
{
    /// <summary>
    /// Interaction logic for Instrument_Select_Window.xaml
    /// </summary>
    public partial class Instrument_Select_Window : Window
    {

        //Codes for Info Log Color Palette
        int Success_Code = 0;
        int Error_Code = 1;
        int Warning_Code = 2;
        int Config_Code = 3;
        int Message_Code = 4;        

        //Instrument Information, updated by GUI
        string Instrument_Address = "";
        int Instrument_Port = 5024;

        //Save Data Directory
        string folder_Directory;

        public Instrument_Select_Window()
        {
            InitializeComponent();
            getSoftwarePath();
            insert_Log("Set the IP Address and Port.", Message_Code);
            insert_Log("Click the Connect button when you are ready.", Message_Code);
            Load_Config();
        }

        private bool Is_Instrument_In_Use()
        {
            try
            {               
                using (IInstrumentSession instrumentSession = new TelnetSession(Instrument_Address, Instrument_Port, timeoutMiliseconds: 2000))
                {
                    instrumentSession.OpenSession();
                    return instrumentSession.IsSessionOpen;
                }
            }
            catch (Exception Ex)
            {
                insert_Log(Ex.Message, Error_Code);
                return false;
            }
        }

        private (bool, string) Instrument_Query(string Query_Command)
        {
            try
            {
                string Message_Data = "";
                using (IInstrumentSession instrumentSession = new TelnetSession(Instrument_Address, Instrument_Port, timeoutMiliseconds: 2000))
                {
                    instrumentSession.OpenSession();
                    instrumentSession.WriteLine(Query_Command);
                    Message_Data = instrumentSession.ReadLine();
                }
                return (true, Message_Data);
            }
            catch (Exception Ex)
            {
                insert_Log(Ex.Message, Error_Code);
                return (false, "Query Failed.");
            }
        }

        private void Instrument_Write(string Write_Command)
        {
            try
            {
                using (IInstrumentSession instrumentSession = new TelnetSession(Instrument_Address, Instrument_Port, timeoutMiliseconds: 2000))
                {
                    instrumentSession.OpenSession();
                    instrumentSession.WriteLine(Write_Command);
                }
            }
            catch (Exception Ex)
            {
                insert_Log(Ex.Message, Error_Code);
            }
        }

        private void Instrument_Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Instrument_Address = LAN_Address.Text.Trim();
            int.TryParse(LAN_Port.Text.Trim(), out Instrument_Port);
            Instrument_Write("*RST");
        }

        private void getSoftwarePath()
        {
            try
            {
                folder_Directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + "Log Data (HP34461A)";
                insert_Log("Test Data will be saved inside the software directory.", Config_Code);
                insert_Log(folder_Directory, Config_Code);
                insert_Log("Click the Select button to select another directory.", Config_Code);
            }
            catch (Exception)
            {
                insert_Log("Cannot get software directory path. Choose a new directory.", Error_Code);
            }
        }

        private int folderCreation(string folderPath)
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                return (0);
            }
            catch (Exception)
            {
                insert_Log("Cannot create test data folder. Choose another file directory.", Error_Code);
                return (1);
            }
        }

        private void insert_Log(string Message, int Code)
        {
            SolidColorBrush Color = Brushes.Black;
            string Status = "";
            if (Code == Error_Code) //Error Message
            {
                Status = "[Error]";
                Color = Brushes.Red;
            }
            else if (Code == Success_Code) //Success Message
            {
                Status = "[Success]";
                Color = Brushes.Green;
            }
            else if (Code == Warning_Code) //Warning Message
            {
                Status = "[Warning]";
                Color = Brushes.Orange;
            }
            else if (Code == Config_Code) //Config Message
            {
                Status = "";
                Color = Brushes.Blue;
            }
            else if (Code == Message_Code)//Standard Message
            {
                Status = "";
                Color = Brushes.Black;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Info_Log.Inlines.Add(new Run(Status + " " + Message + "\n") { Foreground = Color });
                Info_Scroll.ScrollToBottom();
            }));
        }

        private void Select_Directory_Click(object sender, RoutedEventArgs e)
        {
            var Choose_Directory = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (Choose_Directory.ShowDialog() == true)
            {
                folder_Directory = Choose_Directory.SelectedPath + @"\" + "Log Data (HP34461A)";
            }
            insert_Log("Test Data will be saved here: " + folder_Directory, Config_Code);
        }

        private void Verify_Instrument_Click(object sender, RoutedEventArgs e)
        {
            Instrument_Address = LAN_Address.Text.Trim();
            int.TryParse(LAN_Port.Text.Trim(), out Instrument_Port);
            (bool Query_status, string Message) = Instrument_Query("*IDN?");
            if (Query_status == true)
            {
                insert_Log("Verify Successful: " + Message, Success_Code);
            }
            else
            {
                insert_Log("Verify failed.", Error_Code);
            }
        }

        private bool Connect_verify_Instrument()
        {
            Instrument_Address = LAN_Address.Text.Trim();
            int.TryParse(LAN_Port.Text.Trim(), out Instrument_Port);
            (bool Query_status, string Message) = Instrument_Query("*IDN?");
            if (Query_status == true)
            {
                if (Message.Contains("34401A") || Message.Contains("34461A"))
                {
                    insert_Log("Verify Successful: " + Message, Success_Code);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Instrument_Address = LAN_Address.Text.Trim();
            int.TryParse(LAN_Port.Text.Trim(), out Instrument_Port);
            if (folderCreation(folder_Directory) == 0)
            {
                folderCreation(folder_Directory + @"\" + "VDC");
                folderCreation(folder_Directory + @"\" + "ADC");
                folderCreation(folder_Directory + @"\" + "VAC");
                folderCreation(folder_Directory + @"\" + "AAC");
                folderCreation(folder_Directory + @"\" + "2WireOhms");
                folderCreation(folder_Directory + @"\" + "4WireOhms");
                folderCreation(folder_Directory + @"\" + "FREQ");
                folderCreation(folder_Directory + @"\" + "PER");
                folderCreation(folder_Directory + @"\" + "DIODE");
                folderCreation(folder_Directory + @"\" + "CONTINUITY");
                if (Connect_verify_Instrument() == true)
                {
                    if (true)
                    {
                        Instrument_Write("*RST");
                        Data_Updater();
                        insert_Log("Please wait....connecting soon", Success_Code);
                        this.Close();
                    }
                }
                else
                {
                    insert_Log("Verify Failed. Try Again.", Error_Code);
                }
            }
            else
            {
                insert_Log("Log Data Directory cannot be created on the selected path.", Error_Code);
                insert_Log("Choose another path by clicking the select button.", Error_Code);
            }
        }

        private void Data_Updater()
        {
            Instrument_Address_Info.folder_Directory = folder_Directory;
            Instrument_Address_Info.Instrument_Address = Instrument_Address;
            Instrument_Address_Info.Instrument_Port = Instrument_Port;
            Instrument_Address_Info.SessionType = SessionType.Telnet;
            Instrument_Address_Info.isConnected = true;
        }

        private void Info_Clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Info_Log.Inlines.Clear();
                Info_Log.Text = string.Empty;
            }
            catch (Exception)
            {

            }
        }

        private void Load_Config()
        {
            try
            {
                string Software_Location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + "Settings.txt";
                using (var readFile = new StreamReader(Software_Location))
                {
                    string Config = readFile.ReadLine().Trim();
                    Process_Config_File_Data(Config);
                    insert_Log("Settings loaded.", Success_Code);
                }
            }
            catch (Exception Ex)
            {
                insert_Log(Ex.Message, Error_Code);
                insert_Log("Loading Settings file failed.", Error_Code);
            }
        }

        private void Process_Config_File_Data(string Config_Data)
        {
            string[] Config_Parts = Config_Data.Split(',');
            Instrument_Address = Config_Parts[0];
            int.TryParse(Config_Parts[1], out Instrument_Port);
            SessionType sessionType;
            Enum.TryParse(Config_Parts[2], out sessionType);
            LAN_Address.Text = Instrument_Address;
            LAN_Port.Text = Config_Parts[1];
        }

        private void Instrument_Config_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Instrument_Address = LAN_Address.Text.Trim();
                int.TryParse(LAN_Port.Text.Trim(), out Instrument_Port);
                SessionType sessionType = SessionType.Telnet;
                string[] Config_Parts = new string[]
                {
                    Instrument_Address,
                    Instrument_Port.ToString(),
                    sessionType.ToString()
                };

                string Software_Location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + "Settings.txt";

                string File_string = string.Join(",", Config_Parts);
                File.WriteAllText(Software_Location, File_string);
                insert_Log("Settings saved.", Success_Code);
            }
            catch (Exception Ex)
            {
                insert_Log(Ex.Message, Error_Code);
                insert_Log("Failed to save settings, try again.", Error_Code);
            }
        }
        private void LAN_Port_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            string mask = @"^[0-9]*$";
            string text = ((TextBox)sender).Text;
            e.Handled = !Regex.IsMatch(text + e.Text, mask);
        }

        private void LAN_Port_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            string mask = @"^[0-9]*$";
            string text = (String)e.DataObject.GetData(typeof(String));
            if (!Regex.IsMatch(text, mask))
                e.CancelCommand();
        }
    }
}
