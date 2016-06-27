using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace term_notify.Pages.Dialogs
{
    public sealed partial class EditServiceProvider : ContentDialog
    {
        public bool Canceled { get; private set; }
        public string NameEx
        {
            get { return NameInput.Text; }
        }

        public string ServiceEx
        {
            get { return UriInput.Text; }
        }

        public string ParamEx
        {
            get { return ParamInput.Text; }
        }

        private EditServiceProvider()
        {
            this.InitializeComponent();
            Canceled = true;
        }

        public EditServiceProvider( ServiceInfo Info, bool New = false )
            :this()
        {
            if( New ) Title = "Add Service Provider";

            if ( Info == null ) return;

            NameInput.Text = Info.Name;
            UriInput.Text = Info.Protocol;
            ParamInput.Text = Info.Param;
        }

        private void ContentDialog_PrimaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
        {
            Canceled = false;
        }
    }
}
