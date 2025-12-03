using Project1.UI.Controls.Models;
using Project1.UI.Cores;
using ProjectEye.Models.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace ProjectEye.Core.Service
{
    public class ThemeService : IService
    {
        private readonly ConfigService config;
        private readonly SystemResourcesService systemResources;
        private readonly Theme theme;


        public delegate void ThemeChangedEventHandler(string OldThemeName, string NewThemeName);
        /// <summary>
        /// 当切换主题时发生
        /// </summary>
        public event ThemeChangedEventHandler OnChangedTheme;
        public ThemeService(ConfigService config,
            SystemResourcesService systemResources)
        {
            this.config = config;
            this.systemResources = systemResources;
            theme = new Theme();
        }
        public void Init()
        {
            string themeName = config.options.Style.Theme.ThemeName;
            if (systemResources.Themes.Where(m => m.ThemeName == themeName).Count() == 0)
            {
                themeName = systemResources.Themes[0].ThemeName;
                config.options.Style.Theme = systemResources.Themes[0];
                //config.Save();
            }
            Project1.UI.Cores.UIDefaultSetting.DefaultThemeName = themeName;

            Project1.UI.Cores.UIDefaultSetting.DefaultThemePath = "/ProjectEye;component/Resources/Themes/";

            HandleDarkMode();
        }
        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="themeName"></param>
        public void SetTheme(string themeName)
        {

            if (Project1.UI.Cores.UIDefaultSetting.DefaultThemeName != themeName)
            {
                string oldName = Project1.UI.Cores.UIDefaultSetting.DefaultThemeName;

                Project1.UI.Cores.UIDefaultSetting.DefaultThemeName = themeName;

                Project1.UI.Cores.UIDefaultSetting.DefaultThemePath = "/ProjectEye;component/Resources/Themes/";

                theme.ApplyTheme();

                OnChangedTheme?.Invoke(oldName, themeName);
            }
        }

        public void HandleDarkMode()
        {
            string darkModeThemeName = "Dark";
            if (config.options.Style.IsAutoDarkMode)
            {
                var darkTheme = systemResources.Themes.Where(m => m.ThemeName == darkModeThemeName).FirstOrDefault();
                if (darkTheme == null)
                {
                    return;
                }
                DateTime startTime = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    config.options.Style.AutoDarkStartH,
                   config.options.Style.AutoDarkStartM,
                    0);
                DateTime endTime = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    config.options.Style.AutoDarkEndH,
                   config.options.Style.AutoDarkEndM,
                    0);

                bool isOpen = false;

                if (config.options.Style.AutoDarkStartH <= config.options.Style.AutoDarkEndH)
                {
                    isOpen = DateTime.Now >= startTime && DateTime.Now <= endTime;
                }
                else
                {
                    isOpen = DateTime.Now >= startTime || DateTime.Now <= endTime;
                }
                if (isOpen)
                {
                    if (config.options.Style.Theme != darkTheme)
                    {
                        Debug.WriteLine("dark mode open!");
                        config.options.Style.Theme = darkTheme;

                        SetTheme(darkModeThemeName);

                    }
                }
                else
                {
                    var defualtTheme = systemResources.Themes[0];
                    if (config.options.Style.Theme != defualtTheme)
                    {
                        Debug.WriteLine("dark mode close!");
                        config.options.Style.Theme = defualtTheme;

                        SetTheme(defualtTheme.ThemeName);

                    }
                }
            }
        }

        /// <summary>
        /// 创建默认的提示界面布局UI
        /// </summary>
        /// <param name="themeName">主题名</param>
        /// <param name="screenName">屏幕名称</param>
        /// <returns></returns>
        public UIDesignModel GetCreateDefaultTipWindowUI(
            string themeName,
            string screenName)
        {
            screenName = screenName.Replace("\\", "");

            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            if (screenName != string.Empty)
            {
                foreach (var item in System.Windows.Forms.Screen.AllScreens)
                {
                    string itemScreenName = item.DeviceName.Replace("\\", "");
                    if (itemScreenName == screenName)
                    {
                        screen = item;
                        break;
                    }
                }
            }

            var screenSize = WindowManager.GetSize(screen);

            //创建默认布局
            var data = new UIDesignModel();
            data.ContainerAttr = new ContainerModel()
            {
                // 使用深色背景，减少刺眼感
                Background = Project1UIColor.Get("#1A1B1C"),
                Opacity = .95
            };

            var elements = new List<ElementModel>();
            
            // 图片
            var tipimage = new ElementModel();
            tipimage.Type = Project1.UI.Controls.Enums.DesignItemType.Image;
            tipimage.Width = 200; // 稍微调小一点图片，让整体更精致
            tipimage.Opacity = 1;
            tipimage.Height = 138;
            tipimage.Image = $"pack://application:,,,/ProjectEye;component/Resources/Themes/{themeName}/Images/tipImage.png";
            tipimage.X = screenSize.Width / 2 - tipimage.Width / 2;
            tipimage.Y = screenSize.Height * .20;

            // 主提示文本
            var tipText = new ElementModel();
            tipText.Type = Project1.UI.Controls.Enums.DesignItemType.Text;
            tipText.Text = "您已持续用眼{t}分钟，休息一会吧！\n请将注意力集中在至少6米远的地方20秒！";
            tipText.Opacity = 1;
            tipText.TextColor = Project1UIColor.Get("#E0E0E0"); // 浅灰白色文字
            tipText.Width = 500;
            tipText.Height = 80;
            tipText.X = screenSize.Width / 2 - tipText.Width / 2;
            tipText.Y = tipimage.Y + tipimage.Height + 30;
            tipText.FontSize = 22;
            tipText.TextAlignment = 1; // Center

            // 健康小贴士
            var healthTipText = new ElementModel();
            healthTipText.Type = Project1.UI.Controls.Enums.DesignItemType.Text;
            healthTipText.Text = "{HealthTip}";
            healthTipText.Opacity = 0.8;
            healthTipText.TextColor = Project1UIColor.Get("#A0A0A0"); // 较暗的灰色
            healthTipText.Width = 600;
            healthTipText.Height = 60;
            healthTipText.X = screenSize.Width / 2 - healthTipText.Width / 2;
            healthTipText.Y = tipText.Y + tipText.Height + 10;
            healthTipText.FontSize = 16;
            healthTipText.TextAlignment = 1; // Center

            // 倒计时
            var countDownText = new ElementModel();
            countDownText.Text = "{countdown}";
            countDownText.FontSize = 60;
            countDownText.IsTextBold = true;
            countDownText.Type = Project1.UI.Controls.Enums.DesignItemType.Text;
            countDownText.TextColor = Project1UIColor.Get("#FFB803"); // 醒目的颜色
            countDownText.Opacity = 1;
            countDownText.Width = 120;
            countDownText.Height = 80;
            countDownText.X = screenSize.Width / 2 - countDownText.Width / 2;
            countDownText.Y = healthTipText.Y + healthTipText.Height + 20;
            countDownText.TextAlignment = 1;

            // 按钮区域
            double buttonY = countDownText.Y + countDownText.Height + 30;

            // 休息按钮
            var restBtn = new ElementModel();
            restBtn.Type = Project1.UI.Controls.Enums.DesignItemType.Button;
            restBtn.Width = 140;
            restBtn.Height = 50;
            restBtn.FontSize = 16;
            restBtn.Text = "好的";
            restBtn.Opacity = 1;
            restBtn.Command = "rest";
            // 使用默认样式或自定义更显眼的样式
            
            restBtn.X = screenSize.Width / 2 - (restBtn.Width * 2 + 40) / 2;
            restBtn.Y = buttonY;

            // 跳过按钮
            var breakBtn = new ElementModel();
            breakBtn.Type = Project1.UI.Controls.Enums.DesignItemType.Button;
            breakBtn.Width = 140;
            breakBtn.Height = 50;
            breakBtn.FontSize = 16;
            breakBtn.Text = "跳过"; // 改为中文“跳过”更直观
            breakBtn.Style = "basic"; // 保持 basic 样式，或者移除以使用默认
            breakBtn.Command = "break";
            breakBtn.Opacity = 0.8;
            breakBtn.X = screenSize.Width / 2 - (restBtn.Width * 2 + 40) / 2 + (restBtn.Width + 40);
            breakBtn.Y = buttonY;

            elements.Add(tipimage);
            elements.Add(tipText);
            elements.Add(healthTipText);
            elements.Add(countDownText);
            elements.Add(restBtn);
            elements.Add(breakBtn);

            data.Elements = elements;

            return data;
        }
    }
}
