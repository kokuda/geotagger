using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Geotagger
{
    [ComVisibleAttribute(true)]
    public class ScriptInterface
    {
        private System.Windows.Forms.WebBrowser mWebBrowser;

        public ScriptInterface(System.Windows.Forms.WebBrowser webBrowser)
        {
            mWebBrowser = webBrowser;
        }

        public void CallJavaScript(string jsFunc, Object[] args)
        {
            mWebBrowser.Document.InvokeScript(jsFunc, args);
        }

        public void Test(String message)
        {
            MessageBox.Show(message, "client code");
            CallJavaScript("test", new String[] { "called from client code" });
        }
    }
}
