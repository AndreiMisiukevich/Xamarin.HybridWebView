using System;
namespace GalleyFramework.Constants
{
	public static class GalleyHtmlConstants
	{
        /// <summary>
        /// {0} - Your html content
        /// </summary>
        public const string DefaultHtml =
         @"<!DOCTYPE html>
			<html lang=""en"">
			<head>
			    <meta charset=""UTF-8"">
				<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
				<style type=""text/css"">
					html {{ -webkit-touch-callout: none; -webkit-user-select: none; -khtml-user-select: none; -moz-user-select: none; -ms-user-select: none; user-select: none;}}
					body {{ margin: 0; padding-left: 10; padding-right: 10; font-size: 16px; font-family: Arial, Helvetica, sans-serif;}}
					img {{ max-width: 100% }}
				</style>
			</head>
			<body>
			<div>
			   {0} 
			</div>
			</body>
			</html>";

		public const string InvokeSharpCodeActionName = "invokeAction";
		public const string InvokeSharpFunctionPattern = "function invokeAction(data) {{ {0}(data); }}";
	}
}
