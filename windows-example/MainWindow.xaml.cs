using System;
using System.Collections.Generic;
using System.Windows;
using ChatSDK;
using ChatSDK.MessageBody;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace windows_example
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IChatManagerDelegate
    {
        private static readonly string APPKEY = "41117440#383391";

        private readonly System.Windows.Threading.Dispatcher Dip = null;
        private static readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            Dip = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            InitSDK();
            AddChatDelegate();
        }

        private void InitSDK()
        {
            Options options = new Options(appKey: APPKEY);
            SDKClient.Instance.InitWithOptions(options);
        }

        private void AddChatDelegate()
        {
            SDKClient.Instance.ChatManager.AddChatManagerDelegate(this);
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (UserIdTextBox.Text.Length == 0 || PasswordTextBox.Text.Length == 0)
            {
                AddLogToLogText("username or password is null");
                return;
            }


            string token = await LoginToAppServer(UserIdTextBox.Text, PasswordTextBox.Text);
            if (token != null)
            {
                SDKClient.Instance.LoginWithAgoraToken(UserIdTextBox.Text, token, handle: new CallBack(
                    onSuccess: () =>
                    {
                        AddLogToLogText("sign in sdk succeed");
                    },
                    onError: (code, desc) =>
                    {
                        AddLogToLogText($"sign in sdk failed, code: {code}, desc: {desc}");
                    }
                ));
            }
            else
            {
                AddLogToLogText($"fetch token error");
            }
        }

        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            if (UserIdTextBox.Text.Length == 0 || PasswordTextBox.Text.Length == 0)
            {
                AddLogToLogText("username or password is null");
                return;
            }

            bool result = await RegisterToAppServer(UserIdTextBox.Text, PasswordTextBox.Text);
            if (result)
            {
                AddLogToLogText("sign up succeed");
            }
            else
            {
                AddLogToLogText("sign up failed");
            }

        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SDKClient.Instance.Logout(true, handle: new CallBack(
               onSuccess: () =>
               {
                   AddLogToLogText("sign out sdk succeed");
               },
               onError: (code, desc) =>
               {
                   AddLogToLogText($"sign out sdk failed, code: {code}, desc: {desc}");
               }
           ));
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SingleChatIdTextBox.Text.Length == 0)
            {
                AddLogToLogText("single chat id is null");
                return;
            }

            if (MessageContentTextBox.Text.Length == 0)
            {
                AddLogToLogText("message content is null !");
                return;
            }
            Message msg = Message.CreateTextSendMessage(SingleChatIdTextBox.Text, MessageContentTextBox.Text);
            SDKClient.Instance.ChatManager.SendMessage(ref msg, new CallBack(
                onSuccess: () =>
                {
                    Dip.Invoke(() =>
                    {
                        AddLogToLogText($"send message succeed, receiver: {SingleChatIdTextBox.Text},  message: {MessageContentTextBox.Text}");
                    });
                },
                onError: (code, desc) =>
                {
                    AddLogToLogText($"send message failed, code: {code}, desc: {desc}");
                }
            ));

        }

        private void AddLogToLogText(string log)
        {
            Dip.Invoke(() =>
            {
                LogTextBox.Text += DateTime.Now + ": " + log + "\n";
                LogTextBox.ScrollToEnd();
            });
        }

        public void OnMessagesReceived(List<Message> messages)
        {
            foreach (Message msg in messages)
            {
                if (msg.Body.Type == MessageBodyType.TXT)
                {
                    TextBody txtBody = msg.Body as TextBody;
                    AddLogToLogText($"received text message: {txtBody.Text}, from: {msg.From}");
                }
                else if (msg.Body.Type == MessageBodyType.IMAGE)
                {
                    _ = msg.Body as ImageBody;
                    AddLogToLogText($"received image message, from: {msg.From}");
                }
                else if (msg.Body.Type == MessageBodyType.VIDEO)
                {
                    _ = msg.Body as VideoBody;
                    AddLogToLogText($"received video message, from: {msg.From}");
                }
                else if (msg.Body.Type == MessageBodyType.VOICE)
                {
                    _ = msg.Body as VoiceBody;
                    AddLogToLogText($"received voice message, from: {msg.From}");
                }
                else if (msg.Body.Type == MessageBodyType.LOCATION)
                {
                    _ = msg.Body as LocationBody;
                    AddLogToLogText($"received location message, from: {msg.From}");
                }
                else if (msg.Body.Type == MessageBodyType.FILE)
                {
                    _ = msg.Body as FileBody;
                    AddLogToLogText($"received file message, from: {msg.From}");
                }
            }
        }


        private async Task<string> LoginToAppServer(string username, string password)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("userAccount", username);
            values.Add("userPassword", password);
            string jsonStr = JsonConvert.SerializeObject(values);
            HttpContent content = new StringContent(jsonStr);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync("https://a41.easemob.com/app/chat/user/login", content);
            try
            {
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                return dict["accessToken"];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<Boolean> RegisterToAppServer(string username, string password)
        {
    
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("userAccount", username);
            values.Add("userPassword", password);
            string jsonStr = JsonConvert.SerializeObject(values);
            HttpContent content = new StringContent(jsonStr);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync("https://a41.easemob.com/app/chat/user/register", content);
            try
            {
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                if (dict["code"] == "RES_OK")
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void OnCmdMessagesReceived(List<Message> messages)
        {

        }

        public void OnMessagesRead(List<Message> messages)
        {

        }

        public void OnMessagesDelivered(List<Message> messages)
        {

        }

        public void OnMessagesRecalled(List<Message> messages)
        {

        }

        public void OnReadAckForGroupMessageUpdated()
        {

        }

        public void OnGroupMessageRead(List<GroupReadAck> list)
        {

        }

        public void OnConversationsUpdate()
        {

        }

        public void OnConversationRead(string from, string to)
        {

        }
    }
}
