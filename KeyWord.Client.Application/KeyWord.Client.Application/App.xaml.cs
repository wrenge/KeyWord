﻿using KeyWord.Client.Application.Services;
using KeyWord.Client.Application.Views;
using System;
using KeyWord.Client.Storage;
using KeyWord.Client.Storage.Mobile;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KeyWord.Client.Application
{
    public partial class App
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            var dbPath = DependencyService.Get<IDatabasePath>();
            var storage = new CredentialsStorageMobile(dbPath, "keyword.db3");
            DependencyService.RegisterSingleton<ICredentialsStorage>(storage);
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AuthorizeStorage();
        }

        private async void AuthorizeStorage()
        {
            var password = "password"; // TODO
            var storage = DependencyService.Get<ICredentialsStorage>();
            
            if (!storage.HasPassword()) 
                storage.ChangePassword(password);
            storage.Password = password;
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
