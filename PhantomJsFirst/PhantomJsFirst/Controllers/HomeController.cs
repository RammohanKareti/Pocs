using NReco.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhantomJsFirst.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GrabASite()
        {
            LoadAWebPageUsingPhantom();
            return null;
        }

        private void LoadAWebPageUsingPhantom()
        {
            //ExecuteCommand("https://www.google.co.in");
            ReadResponseHeaders("https://syntrafficv1.azurewebsites.net/");
        }


        private string ExecuteCommand(string url)
        {
            
            try
            {

                using (var phantom = new PhantomJSDriver())
                {
                    var indexFile = Server.MapPath("~/Scripts/PhantomJS/index.js");
                    var scriptSource = System.IO.File.ReadAllText(indexFile);
                    var script = phantom.ExecutePhantomJS(scriptSource);

                    phantom.Navigate().GoToUrl("https://www.bing.com");

                    phantom.FindElement(By.Id("sb_form_q")).SendKeys("learn2automate");

                    //Click on Search
                    phantom.FindElement(By.Id("sb_form_go")).Click();

                    Screenshot sh = phantom.GetScreenshot();
                    sh.SaveAsFile(@"C:\Temp.jpg", ImageFormat.Png);
                    
                    phantom.Quit();
                }
                

            }
            catch (Exception ex)
            {
            }

            return string.Empty;
        }

        private void ReadResponseHeaders(string url)
        {
            var scriptFile = Server.MapPath("~/Scripts/PhantomJS/ReadResponseHeaders.js");
            var scriptSource = System.IO.File.ReadAllText(scriptFile);

            var phantomJs = new PhantomJS();

            phantomJs.OutputReceived += (s, e) =>
                {
                    var response = e.Data;

                };

            phantomJs.RunScript(@"var system = require('system');
var page = require('webpage').create();

page.onResourceReceived = function (response) {
    console.log('Response (#' + response.id + ', stage ""' + response.stage + '""): ' + JSON.stringify(response));
};

page.open(system.args[1]);", new string[] { url});
        }

    }
}