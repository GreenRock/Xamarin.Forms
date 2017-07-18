﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	public class WindowsTestBase
	{
		protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
		protected static WindowsDriver<WindowsElement> Session;

		public static IApp ConfigureApp()
		{
			if (Session == null)
			{
				DesiredCapabilities appCapabilities = new DesiredCapabilities();
				appCapabilities.SetCapability("app", "0d4424f6-1e29-4476-ac00-ba22c3789cb6_wzjw7qdpbr1br!App");
				appCapabilities.SetCapability("deviceName", "WindowsPC");
				Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
				Assert.IsNotNull(Session);
				Session.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(4));
			}

			// Make sure we're at the start screen
			Session?.Keyboard.PressKey(Keys.Escape);

			return new WinDriverApp(Session);
		}

		public static void TearDown()
		{
			Session?.Keyboard.PressKey(Keys.Escape);
		}
	}

	public class WinDriverApp : IApp
	{
	    readonly WindowsDriver<WindowsElement> _session;

		public WinDriverApp(WindowsDriver<WindowsElement> session)
		{
			_session = session;
		}

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
            {"button", "ControlType.Button"} 
        };

		ReadOnlyCollection<WindowsElement> FilterControlType(IEnumerable<WindowsElement> elements, string controlType)
		{
			var tag = controlType;

			if (tag == "*")
			{
				return new ReadOnlyCollection<WindowsElement>(elements.ToList());
			}

			if (_controlNameToTag.ContainsKey(controlType))
			{
				tag = _controlNameToTag[controlType];
			}

			return new ReadOnlyCollection<WindowsElement>(elements.Where(element => element.TagName == tag).ToList()); 
		}

		class WinQuery
		{
			public WinQuery(Func<AppQuery, AppQuery> query)
			{
				Raw = GetRawQuery(query);
				Debug.WriteLine($">>>>> Converting raw query '{Raw}' to {nameof(WinQuery)}");
				var spaceIndex = Raw.IndexOf(" ", StringComparison.Ordinal);
				ControlType = Raw.Substring(0, spaceIndex);
				Marked = Raw.Substring(spaceIndex).Replace("marked:'", "").Replace("'", "").Trim(); // TODO hartez that's just ... awful
			}

			public WinQuery(string marked)
			{
				ControlType = "*";
				Marked = marked;

				Raw = $"* '{marked}'";
			}

			string GetRawQuery(Func<AppQuery, AppQuery> query = null)
			{
				if (query == null)
				{
					return string.Empty;
				}

				return query(new AppQuery(QueryPlatform.iOS)).ToString();
			}

			public string ControlType { get; }

			public string Marked { get; }

			public string Raw { get; }

			public override string ToString()
			{
				return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
			}
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(WinQuery query)
		{
			var resultByName = _session.FindElementsByName(query.Marked);
			var resultByAccessibilityId = _session.FindElementsByAccessibilityId(query.Marked);
			
			var result = resultByName
				.Concat(resultByAccessibilityId);

			return FilterControlType(result, query.ControlType);
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(string marked)
		{
			var winQuery = new WinQuery(marked);
			return QueryWindows(winQuery);
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(Func<AppQuery, AppQuery> query)
		{
			var winQuery = new WinQuery(query);
			return QueryWindows(winQuery);
		}

		// TODO hartez 2017/07/13 18:16:20 Make this actually work	
		AppResult Convert(WindowsElement windowsElement)
		{
			return null;
		}

		public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
		{
			var elements = QueryWindows(new WinQuery(query));
			return elements.Select(Convert).ToArray();
		}

		public AppResult[] Query(string marked)
		{
			throw new NotImplementedException();
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
		{
			throw new NotImplementedException();
		}

		public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(string marked)
		{
			throw new NotImplementedException();
		}

		public void EnterText(string text)
		{
			throw new NotImplementedException();
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			QueryWindows(query).First().SendKeys(text);
		}

		public void EnterText(string marked, string text)
		{
			throw new NotImplementedException();
		}

		public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
		{
			throw new NotImplementedException();
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(string marked)
		{
			throw new NotImplementedException();
		}

		public void ClearText()
		{
			throw new NotImplementedException();
		}

		public void PressEnter()
		{
			throw new NotImplementedException();
		}

		public void DismissKeyboard()
		{
			throw new NotImplementedException();
		}

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			var winQuery = new WinQuery(query);
			Tap(winQuery);
		}

		public void Tap(string marked)
		{
			var winQuery = new WinQuery(marked);
			Tap(winQuery);
		}

		void Tap(WinQuery query)
		{
			Func<ReadOnlyCollection<WindowsElement>> fquery = () => QueryWindows(query);

			var timeoutMessage = $"Timed out waiting for element: {query.Raw}";

			var results = WaitForAtLeastOne(fquery, timeoutMessage);

			if (results.Any())
			{
				results.First().Click();
			}
		}

		public void Tap(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TapCoordinates(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void TouchAndHold(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TouchAndHold(string marked)
		{
			throw new NotImplementedException();
		}

		public void TouchAndHoldCoordinates(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void DoubleTap(string marked)
		{
			throw new NotImplementedException();
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(string marked, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(string marked, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		ReadOnlyCollection<WindowsElement> WaitForAtLeastOne(Func<ReadOnlyCollection<WindowsElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			return Wait(query, i => i > 0, timeoutMessage, timeout, retryFrequency);
		}

		void WaitForNone(Func<ReadOnlyCollection<WindowsElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == 0, timeoutMessage, timeout, retryFrequency);
		}

		ReadOnlyCollection<WindowsElement> Wait(Func<ReadOnlyCollection<WindowsElement>> query,
			Func<int, bool> satisfactory,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(5);
			retryFrequency = retryFrequency ?? TimeSpan.FromMilliseconds(500);
			timeoutMessage = timeoutMessage ?? "Timed out on query.";

			var start = DateTime.Now;

			var result = query();

			while (!satisfactory(result.Count))
			{
				if (DateTime.Now.Subtract(start).Ticks >= timeout.Value.Ticks)
				{
					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
			}

			return result;
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(query);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(Convert).ToArray();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(marked);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(Convert).ToArray();
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public FileInfo Screenshot(string title)
		{
			throw new NotImplementedException();
		}

		public void SwipeRight()
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeft()
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(Func<AppQuery, AppQuery> query = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDown(Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDown(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void SetOrientationPortrait()
		{
			throw new NotImplementedException();
		}

		public void SetOrientationLandscape()
		{
			throw new NotImplementedException();
		}

		public void Repl()
		{
			throw new NotImplementedException();
		}

		public void Back()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeUp()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeDown()
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object argument = null)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object[] arguments)
		{
			throw new NotImplementedException();
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> @from, Func<AppQuery, AppQuery> to)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(string @from, string to)
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(string marked, double value)
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(Func<AppQuery, AppQuery> query, double value)
		{
			throw new NotImplementedException();
		}

		public AppPrintHelper Print { get; }

		public IDevice Device { get; }

		public ITestServer TestServer { get; }
	}
}
