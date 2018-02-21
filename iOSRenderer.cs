using System;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using GalleyFramework.Views.Controls;
using GalleyFramework.iOS.Renderers;
using System.Diagnostics;
using GalleyFramework.Constants;

[assembly: ExportRenderer (typeof(GalleyBrowserControl), typeof(GalleyBrowserControlRenderer))]
namespace GalleyFramework.iOS.Renderers
{
	public class GalleyBrowserControlRenderer : ViewRenderer<GalleyBrowserControl, WKWebView>, IWKScriptMessageHandler
	{
		private static readonly string JavaScriptFunction = 
            string.Format(GalleyHtmlConstants.InvokeSharpFunctionPattern, $"window.webkit.messageHandlers.{GalleyHtmlConstants.InvokeSharpCodeActionName}.postMessage");

		private WKUserContentController _userController;

		protected override void OnElementChanged(ElementChangedEventArgs<GalleyBrowserControl> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				_userController = new WKUserContentController();
				var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
				_userController.AddUserScript(script);
				_userController.AddScriptMessageHandler(this, GalleyHtmlConstants.InvokeSharpCodeActionName);

				var config = new WKWebViewConfiguration { UserContentController = _userController };
				var webView = new WKWebView(Frame, config)
				{
					NavigationDelegate = new CustomWKNavigationDelegate()
				};
				SetNativeControl(webView);
                OnElementPropertyChanged(nameof(Element.Uri));
                OnElementPropertyChanged(nameof(Element.Html));
                OnJavaScriptInvoked(Element.JavaScriptFunction);
			}
			if (e.OldElement != null)
			{
				_userController.RemoveAllUserScripts();
				_userController.RemoveScriptMessageHandler(GalleyHtmlConstants.InvokeSharpCodeActionName);
				e.OldElement.JavaScriptInvoked -= OnJavaScriptInvoked;
			}
            if(e.NewElement != null)
            {
                e.NewElement.JavaScriptInvoked += OnJavaScriptInvoked;
            }
		}

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            OnElementPropertyChanged(e.PropertyName);
        }

		public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
		{
			Element?.InvokeCallback(message?.Body?.ToString());
		}

        private void OnElementPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(GalleyBrowserControl.Uri):
                    ExecuteNotNull(Element.Uri, (p) => Control.LoadRequest(new NSUrlRequest(new NSUrl(p))));
                    return;
                case nameof(GalleyBrowserControl.Html):
                    ExecuteNotNull(Element.Html, (p) => Control.LoadHtmlString(p, null));
                    return;
            }
        }

        private void OnJavaScriptInvoked(string js)
        {
            ExecuteNotNull(js, async p =>
            {
                try
                {
                    await Control?.EvaluateJavaScriptAsync(js);
                }
                catch
                {
                    Debug.WriteLine($"Cant invoke {js}");
                }
            });
        }

        private void ExecuteNotNull(string parameter, Action<string> action)
        {
            if(parameter != null)
            {
                action.Invoke(parameter);
            }
        }
 
		private class CustomWKNavigationDelegate : WKNavigationDelegate
		{
			public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
			{
				try
				{
					var policy = WKNavigationActionPolicy.Allow;
					if (navigationAction.NavigationType == WKNavigationType.LinkActivated)
					{
						var url = new NSUrl(navigationAction.Request.Url.ToString());
						if (UIApplication.SharedApplication.CanOpenUrl(url))
						{
							UIApplication.SharedApplication.OpenUrl(url);
							policy = WKNavigationActionPolicy.Cancel;
						}
					}
                    decisionHandler?.Invoke(policy);
				}
				catch
				{
					base.DecidePolicy(webView, navigationAction, decisionHandler);
				}
			}

            public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
            {
                completionHandler?.Invoke(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
            }
		}
	}
}