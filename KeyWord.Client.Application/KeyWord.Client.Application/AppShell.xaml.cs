﻿using KeyWord.Client.Application.ViewModels;
using KeyWord.Client.Application.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace KeyWord.Client.Application
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}