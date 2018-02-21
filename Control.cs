using System;
using Xamarin.Forms;
using System.Windows.Input;

namespace GalleyFramework.Views.Controls
{
	public class GalleyBrowserControl : View
	{
        public event Action<string> JavaScriptInvoked;

        public static readonly BindableProperty InvokeCallbackCommandProperty = BindableProperty.Create(
            nameof(InvokeCallbackCommand),
			typeof(ICommand),
			typeof(GalleyBrowserControl),
			default(ICommand));

		public static readonly BindableProperty UriProperty = BindableProperty.Create(
		  nameof(Uri),
		  typeof(string),
		  typeof(GalleyBrowserControl),
		  default(string));

        public static readonly BindableProperty HtmlProperty = BindableProperty.Create(
          nameof(Html),
		  typeof(string),
		  typeof(GalleyBrowserControl),
		  default(string));

		public static readonly BindableProperty JavaScriptFunctionProperty = BindableProperty.Create(
          nameof(JavaScriptFunction),
          typeof(string),
          typeof(GalleyBrowserControl),
          default(string),
          propertyChanged: OnJavaScriptFunctionPropertyChanged,
          defaultBindingMode: BindingMode.TwoWay);


		public ICommand InvokeCallbackCommand
		{
			get { return (ICommand)GetValue(InvokeCallbackCommandProperty); }
			set { SetValue(InvokeCallbackCommandProperty, value); }
		}

		public string Uri
		{
			get { return GetValue(UriProperty) as string; }
			set { SetValue(UriProperty, value); }
		}

		public string Html
		{
			get { return GetValue(HtmlProperty) as string; }
			set { SetValue(HtmlProperty, value); }
		}

        public string JavaScriptFunction
        {
            get { return GetValue(JavaScriptFunctionProperty) as string; 
            }
            set { SetValue(JavaScriptFunctionProperty, value); }
        }

		public void InvokeCallback(string data)
		{
			InvokeCallbackCommand?.Execute(data);
		}

        private static void OnJavaScriptFunctionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var value = newValue as string;
            var webView = bindable as GalleyBrowserControl;
            if (value != null && webView != null)
            {
                webView.JavaScriptInvoked?.Invoke(value);
                webView.JavaScriptFunction = null;
            }
        }
	}
}
