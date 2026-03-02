using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class BorrowViewModel : ObservableObject
	{
		private readonly IBorrowService _borrowService;

		private ObservableCollection<BorrowRequestDto> _pendingRequests = new ObservableCollection<BorrowRequestDto>();

		public ObservableCollection<BorrowRequestDto> PendingRequests
		{
			get => _pendingRequests;
			set => SetProperty(ref _pendingRequests, value);
		}

		public ICommand RefreshPendingCommand { get; }

		public BorrowViewModel(IBorrowService borrowService)
		{
			_borrowService = borrowService;
			//RefreshPendingCommand = new RelayCommand(RefreshPending);
			RefreshPendingCommand = new RelayCommand(_ => RefreshPending()); // Dùng lambda để wrap
			RefreshPending(); // Load ban đầu
		}

		private async void RefreshPending()
		{
			try
			{
				var requests = await _borrowService.GetPendingRequestsAsync();
				PendingRequests = new ObservableCollection<BorrowRequestDto>(requests);
			}
			catch (Exception ex)
			{
				// Show lỗi
			}
		}
	}
}
