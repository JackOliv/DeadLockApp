using DeadLockApp.ViewModels;
using DeadLockApp.Models;
using System;
using Microsoft.Maui.Controls;
namespace DeadLockApp;

public partial class Heroes : ContentPage
{
	public Heroes()
	{
		InitializeComponent();
        BindingContext = new MainViewModel();
    }

    
    private void OnFavoriteButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var hero = (Hero)button.BindingContext;  // �������� ������ ����� �� ��������� ��������

        // �������� ��������� ����������
        hero.IsFavorite = !hero.IsFavorite;

        // ����� �������� ���������� �����, ����� ���������, ��� �������� ����������
        Console.WriteLine($"{hero.Name} - ���������: {hero.IsFavorite}");
    }
}