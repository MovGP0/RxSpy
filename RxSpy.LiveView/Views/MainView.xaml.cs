﻿using System.Windows;
using ReactiveUI;
using RxSpy.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace RxSpy.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl, IViewFor<MainViewModel>
    {
        public MainView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as MainViewModel;

            this.OneWayBind(ViewModel, vm => vm.GridViewModel, v => v.observablesGrid.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.DetailsViewModel, v => v.detailsView.ViewModel);

            this.OneWayBind(ViewModel, vm => vm.SignalsPerSecond, v => v.signalsPerSecond.Text);
            this.OneWayBind(ViewModel, vm => vm.SignalCount, v => v.signals.Text);
            this.OneWayBind(ViewModel, vm => vm.ErrorCount, v => v.errors.Text);
        }

        public MainViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as MainViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(MainViewModel),
            typeof(MainView),
            new PropertyMetadata(null)
        );

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as MainViewModel; }
        }
    }
}
