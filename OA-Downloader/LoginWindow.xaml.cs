using System;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OA_Downloader
{
    public partial class LoginWindow : Window
    {
        private readonly string _saveFilePath = "userCredentials.dat"; // 存储用户凭据的文件路径

        public LoginWindow()
        {
            InitializeComponent();
            LoadCredentials(); // 启动时加载用户凭据
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string userAccount = txtUserAccount.Text;
            string userPwd = txtUserPwd.Password; // 密码框

            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(userPwd))
            {
                MessageBox.Show("请填写账号和密码。");
                return;
            }

            // 构造登录请求的参数
            var loginData = new
            {
                userAccount = userAccount,
                userPwd = userPwd
            };

            // 调用登录接口，获取响应
            string result = await RestClient.PostAsync("/user/login", loginData);

            // 解析响应
            try
            {
                JObject response = JObject.Parse(result);
                int code = (int)response["code"];
                string msg = (string)response["msg"];

                // 检查 code 是否为 200
                if (code == 200)
                {
                    // 解析成功的 JSON 响应，获取 token 和用户数据
                    string token = (string)response["data"]["token"];
                    string userName = (string)response["data"]["data"]["userName"];

                    MessageBox.Show("登录成功！欢迎, " + userName);

                    // 保存 token，用于后续请求
                    Properties.Settings.Default.Token = token;
                    Properties.Settings.Default.Save(); // 持久化保存 token

                    // 读取 Token 的值
                    string savedToken = Properties.Settings.Default.Token;

                    // // 打开下载窗口
                    // var downloadWindow = new DownloadWindow();
                    // downloadWindow.Show();
                    // this.Close();
                    var fileManagerWindow =  new FileManagerWindow();
                    fileManagerWindow.Show();
                    this.Close();
                    

                    // 如果选择了“记住密码”，则保存账号和密码
                    if (chkRememberMe.IsChecked == true)
                    {
                        SaveCredentials(userAccount, userPwd); // 假设 SaveCredentials 是你定义的方法
                    }
                    else
                    {
                        ClearCredentials(); // 如果没有选中记住密码，清除存储的账号密码
                    }
                }
                else
                {
                    // 登录失败，弹出提示框
                    MessageBox.Show($"登录失败: {msg}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析响应时出错：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        


        // 保存账号和密码到文件中（加密）
        private void SaveCredentials(string userAccount, string userPwd)
        {
            try
            {
                string encryptedAccount = Encrypt(userAccount);
                string encryptedPassword = Encrypt(userPwd);

                File.WriteAllText(_saveFilePath, $"{encryptedAccount}\n{encryptedPassword}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存用户凭据失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 从文件中加载账号和密码（解密）
        private void LoadCredentials()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string[] credentials = File.ReadAllLines(_saveFilePath);
                    if (credentials.Length == 2)
                    {
                        txtUserAccount.Text = Decrypt(credentials[0]);
                        txtUserPwd.Password = Decrypt(credentials[1]);
                        chkRememberMe.IsChecked = true; // 如果有存储凭据，自动勾选“记住密码”
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载用户凭据失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 清除存储的账号和密码
        private void ClearCredentials()
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
        }

        // 加密方法
        private string Encrypt(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"); // 假设密钥为 32 位字符串
                aes.IV = Encoding.UTF8.GetBytes("ABCDEF0123456789"); // 16 位初始化向量

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        // 解密方法
        private string Decrypt(string encryptedData)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] decryptedBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"); // 假设密钥为 32 位字符串
                aes.IV = Encoding.UTF8.GetBytes("ABCDEF0123456789"); // 16 位初始化向量

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        
    }
}
