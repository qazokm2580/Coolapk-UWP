﻿using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private Symbol paneOpenSymbolIcon = Symbol.OpenPane;

        public Symbol PaneOpenSymbolIcon
        {
            get => paneOpenSymbolIcon;
            private set
            {
                paneOpenSymbolIcon = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ShellPage()
        {
            this.InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching))
            {
                _ = SettingsHelper.CheckUpdateAsync();
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                if (splitView.IsPaneOpen)
                {
                    ee.Handled = true;
                    splitView.IsPaneOpen = false;
                    PaneOpenSymbolIcon = Symbol.ClosePane;
                }

                if (shellFrame.CanGoBack)
                {
                    ee.Handled = true;
                    shellFrame.GoBack();
                }
            };

            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            UIHelper.IsSplitViewPaneOpenedChanged += (s, e) =>
            {
                splitView.IsPaneOpen = e;
                PaneOpenSymbolIcon = e ? Symbol.OpenPane : Symbol.ClosePane;
            };
#pragma warning disable 0612
            _ = ImageCacheHelper.CleanOldVersionImageCacheAsync();
#pragma warning restore 0612
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            shellFrame.Navigate(typeof(MainPage));
            paneFrame.Navigate(typeof(MyPage), new ViewModels.MyPage.ViewMode());
            UIHelper.MainFrame = shellFrame;
            UIHelper.PaneFrame = paneFrame;
            UIHelper.InAppNotification = AppNotification;
            UIHelper.ShellDispatcher = Frame.Dispatcher;
            if (SettingsHelper.Get<bool>(SettingsHelper.IsFirstRun))
            {
                AboutDialog dialog = new AboutDialog();
                await dialog.ShowAsync();
                SettingsHelper.Set(SettingsHelper.IsFirstRun, false);
            }
            splitView.IsPaneOpen = Window.Current.Bounds.Width >= 960;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag as string)
            {
                case "panel":
                    splitView.IsPaneOpen = !splitView.IsPaneOpen;
                    PaneOpenSymbolIcon = splitView.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
                    break;

                case "home":
                    paneFrame.Navigate(typeof(MyPage), new ViewModels.MyPage.ViewMode());
                    break;
            }
        }

        private void paneFrame_Navigated(object sender, NavigationEventArgs e)
        {
            goHomeButton.Visibility = e.SourcePageType == typeof(MyPage) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var canOpen = splitView.IsPaneOpen && (e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 960;

            splitView.IsPaneOpen = canOpen;
            PaneOpenSymbolIcon = canOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }
    }
}