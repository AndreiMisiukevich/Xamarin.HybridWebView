using System;
using Android.Content;
using Android.Text;
using Android.Webkit;
using GalleyFramework.Droid.Renderers;
using GalleyFramework.Views.Controls;
using GalleyFramework.Constants;
using Java.Interop;
using Java.Net;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Internals;

[assembly: ExportRenderer (typeof(GalleyBrowserControl), typeof(GalleyBrowserControlRenderer))]
namespace GalleyFramework.Droid.Renderers
{
	public class GalleyBrowserControlRenderer : ViewRenderer<GalleyBrowserControl, Android.Webkit.WebView>
	{
		private const string JsBridgeName = "jsBridge";
		private static readonly string JavaScriptFunction = string.Format(GalleyHtmlConstants.InvokeSharpFunctionPattern, $"{JsBridgeName}.{GalleyHtmlConstants.InvokeSharpCodeActionName}");

        public GalleyBrowserControlRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<GalleyBrowserControl> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var webView = new Android.Webkit.WebView(Forms.Context);
				webView.Settings.DefaultTextEncodingName = "utf-8";
				webView.SetWebViewClient(new HybriddWebViewClient());
				webView.Settings.JavaScriptEnabled = true;
				webView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
				SetNativeControl(webView);
				OnElementPropertyChanged(nameof(Element.Uri));
				OnElementPropertyChanged(nameof(Element.Html));
				OnJavaScriptInvoked(Element.JavaScriptFunction);
			}
			if (e.OldElement != null)
			{
				Control.RemoveJavascriptInterface(JsBridgeName);
                e.OldElement.JavaScriptInvoked -= OnJavaScriptInvoked;
			}
			if (e.NewElement != null)
			{
				Control.AddJavascriptInterface(new JSBridge(this), JsBridgeName);
                e.NewElement.JavaScriptInvoked += OnJavaScriptInvoked;
			}
		}

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            OnElementPropertyChanged(e.PropertyName);

        }

		private void OnElementPropertyChanged(string propertyName)
		{
            switch (propertyName)
			{
				case nameof(GalleyBrowserControl.Uri):
                    ExecuteNotNull(Element.Uri, p => Control.LoadUrl(p));
					return;
				case nameof(GalleyBrowserControl.Html):
                    ExecuteNotNull(Element.Html, p => Control.LoadDataWithBaseURL("", p, "text/html; charset=utf-8", "utf-8", ""));
					return;
			}
		}

        private void OnJavaScriptInvoked(string js)
        {
            ExecuteNotNull(Element.JavaScriptFunction, p =>
            {
                try
                {
                    Control.LoadUrl($"javascript:{Element.JavaScriptFunction}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Cant invoke {Element.JavaScriptFunction}");
                }
            });
        }

		private void ExecuteNotNull(string parameter, Action<string> action)
		{
			if (parameter != null)
			{
				action.Invoke(parameter);
			}
		}
		
		private class JSBridge : Java.Lang.Object
		{
			private readonly WeakReference<GalleyBrowserControlRenderer> _hybridWebViewRenderer;

			public JSBridge(GalleyBrowserControlRenderer hybridRenderer)
			{
				_hybridWebViewRenderer = new WeakReference<GalleyBrowserControlRenderer>(hybridRenderer);
			}

			[JavascriptInterface]
			[Export(GalleyHtmlConstants.InvokeSharpCodeActionName)]
			public void InvokeAction(string data)
			{
				GalleyBrowserControlRenderer hybridRenderer;
				if (_hybridWebViewRenderer != null && _hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
				{
					hybridRenderer.Element?.InvokeCallback(data);
				}
			}
		}

		private class HybriddWebViewClient : WebViewClient
		{
			public override void OnPageFinished(Android.Webkit.WebView view, string url)
			{
				base.OnPageFinished(view, url);
				view.LoadUrl($"javascript:{JavaScriptFunction}");
			}

            public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
            {
                try
                {
                    var loadIntent = new Intent(Intent.ActionView);
                    loadIntent.SetData(request.Url);
                    view.Context.StartActivity(loadIntent);
                    return true;
                }
				catch
				{
					return base.ShouldOverrideUrlLoading(view, request);
				}
            }

			public override void OnReceivedSslError(Android.Webkit.WebView view, SslErrorHandler handler, Android.Net.Http.SslError error)
			{
				handler.Proceed();
			}
		}
	}
}
