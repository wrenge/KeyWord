﻿namespace KeyWord.Client.Application;

public partial class ClickPage : ContentPage
{
    public int Count { get; set; }
    
    public ClickPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        Count++;

        if (Count == 1)
            CounterBtn.Text = $"Clicked {Count} time";
        else
            CounterBtn.Text = $"Clicked {Count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}