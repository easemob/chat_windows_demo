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
    public partial class MainWindow : Window
    {
        // 初始化SDK用的Appkey，此处的值为演示用，如果正式环境需要使用你申请的Appkey
        private static readonly string APPKEY = "41117440#383391";

        private readonly System.Windows.Threading.Dispatcher Dip = null;
        private static readonly HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            Closed += CloseWindow;
            Dip = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            InitSDK();
            AddChatDelegate();
        }

        // 初始化聊天SDK
        private void InitSDK()
        {

        }

        // 添加消息监听
        private void AddChatDelegate()
        {

        }

        // 移除消息监听
        private void RemoveChatDelegate()
        {

        }


        // 关闭窗口事件
        private void CloseWindow(object sender, EventArgs e)
        {
            RemoveChatDelegate();
        }

        // 点击SignIn按钮
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
                // TODO：
            }
            else
            {
                AddLogToLogText($"fetch token error");
            }
        }

        // 点击SignUp按钮
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

        // 点击SignOut按钮
        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
        }

        // 点击Send按钮
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
            
            // TODO:

        }

        // 添加日志到控制台
        private void AddLogToLogText(string log)
        {
            Dip.Invoke(() =>
            {
                LogTextBox.Text += DateTime.Now + ": " + log + "\n";
                LogTextBox.ScrollToEnd();
            });
        }

        // 根据账号密码获取登录token
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

        // 注册账号
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

    }
}
