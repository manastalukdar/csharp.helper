using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace csharp.ui.controls.HomeRealmDiscovery
{
    /// <summary>
    /// Interaction logic for Hrd.xaml
    /// </summary>
    public partial class Hrd : Window
    {
        #region Public Constructors

        public Hrd()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Internal Methods

        internal void AddButtons(List<Button> buttons)
        {
            double buttonWidth = 200;
            double buttonHeight = 50;

            double top = 10;
            double bottom = 0;// = -(this.IdpListGrid.ActualHeight - top - (2 * buttonHeight));
            double left = 0;// = -((this.IdpListGrid.ActualWidth - buttonWidth) / 2);
            double right = left;

            foreach (Button b in buttons)
            {
                b.Width = buttonWidth;
                b.Height = buttonHeight;

                b.Margin = new Thickness(left, top, right, bottom);
                b.BorderThickness = new Thickness(1);

                top += b.Height;
                bottom -= b.Height;

                //this.IdpListGrid.Children.Add(b);
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void HandleCloseClick(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HandleHeaderPreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void HandleMinimizeClick(Object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void HandleMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem item1 = new MenuItem();
            MenuItem item2 = new MenuItem();

            item1.Header = "Minimize";
            item1.Click += new RoutedEventHandler(HandleMinimizeClick);
            contextMenu.Items.Add(item1);

            item2.Header = "Close";
            item2.Click += new RoutedEventHandler(HandleCloseClick);
            contextMenu.Items.Add(item2);

            contextMenu.HasDropShadow = true;
            contextMenu.IsOpen = true;
        }

        private void HandlePreviewMouseMove(Object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion Private Methods
    }
}