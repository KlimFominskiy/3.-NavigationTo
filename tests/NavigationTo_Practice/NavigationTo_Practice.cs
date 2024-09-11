using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Text.RegularExpressions;

namespace _3._NavigationTo
{
    public class Tests
    {
        private IWebDriver webDriver;

        private WebDriverWait webDriverWait;

        private const string iBMainURL = "https://ib.psbank.ru";
        private const string navbarMenuXPath = "//rtl-navbar-menu";
        private const string financialProductsHeaderText = "Финансовые продукты";
        private const string financialProductsHeaderXPath = 
        $"  //h1[contains(text(), '{financialProductsHeaderText}')]" +
        $"| //h2[contains(text(), '{financialProductsHeaderText}')]" +
        $"| //h3[contains(text(), '{financialProductsHeaderText}')]" +
        $"| //h4[contains(text(), '{financialProductsHeaderText}')]" +
        $"| //h5[contains(text(), '{financialProductsHeaderText}')]" +
        $"| //h6[contains(text(), '{financialProductsHeaderText}')]";
        private const string investmentsComboboxXPath = "//header//a//*[contains(text(),'Инвестиции')]";
        private const string investmentsBrokerageLinkXPath = "//a[@href='/store/products/investmentsbrokerage']";

        private const string consumerLoanURL = "https://ib.psbank.ru/store/products/consumer-loan";
        private const string landingBannerXPath = "//rtl-landing-banner";
        private const string copyrightsXPath = "//rtl-copyrights";

        private const string investmentsBrokerageURL = "https://ib.psbank.ru/store/products/investmentsbrokerage";
        private const string containerWithProductLinks = "//rtl-container-with-product-links";
        private const string licenseInfoText = "Генеральная лицензия на&nbsp;осуществление банковских операций";
        private const string licenseInfoPattern = @"Генеральная лицензия на осуществление банковских операций № \d{4} от \d{2} .* \d{4}";
        private static readonly string licenseInfoXPath = $"//font[contains(text(),'{System.Web.HttpUtility.HtmlDecode(licenseInfoText)}')]";

        [SetUp]
        public void Setup()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            //chromeOptions.AddArgument("--headless");
            webDriver = new ChromeDriver(chromeOptions);
            webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            webDriver.Manage().Window.Maximize();
        }

        [TearDown]
        public void Teardown()
        {
            webDriver.Quit();
            webDriver.Dispose();
        }

        [Test(Description = "1. Определить уникальный элемент на каждой из трех страниц.")]
        [TestCase(iBMainURL, navbarMenuXPath)]
        [TestCase(consumerLoanURL, landingBannerXPath)]
        [TestCase(investmentsBrokerageURL, containerWithProductLinks)]
        public void FindUniqueElements(string uRL, string xPath)
        {
            webDriver.Navigate().GoToUrl(uRL);
            webDriverWait.Until(driver => driver.FindElements(By.XPath(xPath)));
            Assert.DoesNotThrow(() => webDriverWait.Until((driver => driver.FindElement(By.XPath(xPath)))));
            Assert.That(webDriverWait.Until(driver => driver.FindElements(By.XPath(xPath))).Count, Is.EqualTo(1));
        }

        [Test(Description = "2. Реализовать метод для ожидание загрузки страниц из предыдущей задачи")]
        [TestCase(iBMainURL, financialProductsHeaderXPath, financialProductsHeaderText)]
        public void WaitLoading(string uRL, string headerXPath, string headerText)
        {
            webDriver.Navigate().GoToUrl(uRL);
            webDriverWait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            IWebElement header = null;
            Assert.DoesNotThrow(() => header = webDriverWait.Until(driver => driver.FindElement(By.XPath(headerXPath))));
            Assert.That(header.Text, Is.EqualTo(headerText));
        }

        [Test(Description = "3. Проверить переход по ссылке внутри элемента.")]
        public void VerifyNavigation()
        {
            webDriver.Navigate().GoToUrl(iBMainURL);
            webDriverWait.Until(driver => driver.FindElement(By.XPath(investmentsComboboxXPath))).Click();
            webDriverWait.Until(driver => driver.FindElement(By.XPath(investmentsBrokerageLinkXPath))).Click();
            webDriverWait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            webDriverWait.Until(driver => driver.Url.Equals(investmentsBrokerageURL));
        }

        [Test(Description = "4. Работа с различными вкладками браузера. Реализовать механизм переключение между активными вкладками.")]
        public void BrowserTabs()
        {
            webDriver.Navigate().GoToUrl(consumerLoanURL);
            webDriverWait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Assert.That(webDriver.Url, Is.EqualTo(consumerLoanURL));

            ((IJavaScriptExecutor)webDriver).ExecuteScript("window.open();");
            System.Collections.ObjectModel.ReadOnlyCollection<string> tabs = webDriver.WindowHandles;
            webDriver.SwitchTo().Window(tabs[1]);
            webDriver.Navigate().GoToUrl(investmentsBrokerageURL);
            webDriverWait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Assert.That(webDriver.Url, Is.EqualTo(investmentsBrokerageURL));

            Assert.That(Regex.IsMatch(webDriverWait.Until(driver => driver.FindElement(By.XPath(licenseInfoXPath))).Text, licenseInfoPattern));

            webDriver.Close();
            webDriver.SwitchTo().Window(tabs[0]);

            Assert.That(Regex.IsMatch(webDriverWait.Until(driver => driver.FindElement(By.XPath(copyrightsXPath))).Text, licenseInfoPattern));
        }
    }
}