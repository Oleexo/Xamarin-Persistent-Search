using System;

namespace Orion.Xam.Android.SearchBox.Events {
	public class SearchBoxKeyBoardEventArgs : EventArgs {
		public enum KeyboardStatus {
			Opened,
			Closed
		}
		public bool IsClosingAfterSelection { get; }
		public KeyboardStatus Status { get; }
		public SearchBoxKeyBoardEventArgs(bool isSelection, KeyboardStatus status) {
			IsClosingAfterSelection = isSelection;
			Status = status;
		}
	}
}