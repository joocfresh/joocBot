using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using OpenQA.Selenium.Support.UI;
using System.Drawing;

namespace joocBot.Albion
{
    public class WebScreenshotCapturer: IDisposable
    {
        private IWebDriver _driver;

        public WebScreenshotCapturer()
        {
            // 클래스 초기화 시에 Chrome 드라이버를 헤드리스 모드로 설정
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");

            // Chrome 드라이버 초기화
            _driver = new ChromeDriver(options);
        }

        public void CaptureScreenshot(string url, string outputPath)
        {
            // 웹 페이지 열기
            _driver.Navigate().GoToUrl(url);

            // 페이지 로딩을 위해 최대 10초까지 기다림
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(_driver => _driver.FindElement(By.TagName("img")));
            
            // 스크린샷 찍기
            ITakesScreenshot screenshotDriver = (ITakesScreenshot)_driver;
            Screenshot screenshot = ((ITakesScreenshot)_driver).GetScreenshot();

            // 스크린샷 저장
            screenshot.SaveAsFile(outputPath);
        }
        public void CaptureScreenshot(string url, string className , string outputPath)
        {
            // 웹 페이지 열기
            _driver.Navigate().GoToUrl(url);
            //_driver.Manage().Window.Maximize();
            // 스크롤 다운
            ScrollDown();

            // 페이지 로딩을 위해 최대 10초까지 기다림

            IWebElement element = _driver.FindElement(By.ClassName("kill__value-row"));

            // 스크린샷 찍기
            ITakesScreenshot screenshotDriver = (ITakesScreenshot)_driver;
            @Screenshot screenshot = screenshotDriver.GetScreenshot();

            using (var bitmap = new Bitmap(new MemoryStream(screenshot.AsByteArray)))
            {
                Rectangle elementRectangle = new Rectangle(element.Location, new Size(element.Size.Width*2, element.Size.Height*2));
                Bitmap elementBitmap = bitmap.Clone(elementRectangle, bitmap.PixelFormat);
                elementBitmap.Save(outputPath, format: System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void ScrollDown()
        {
            // JavaScript를 사용하여 스크롤 다운
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)_driver;
            jsExecutor.ExecuteScript("window.scrollBy(0, 500);");

            // 하위 태그들이 로딩될 때까지 대기
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(_driver => _driver.FindElement(By.ClassName("reactable-data")));
        }

        public void Dispose()
        {
            // 클래스 사용이 끝날 때 Chrome 드라이버를 종료
            _driver.Quit();
        }
    }
}
