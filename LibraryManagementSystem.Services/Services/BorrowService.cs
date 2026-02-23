using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services
{
	public class BorrowService : IBorrowService
	{
		private readonly IUnitOfWork _uow;

		public BorrowService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<BorrowRequestDto> SubmitBorrowRequestAsync(int readerId, SubmitBorrowRequestDto dto)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(readerId);
			if (reader == null) throw new KeyNotFoundException("Không tìm thấy độc giả");

			var request = new BorrowRequest
			{
				ReaderId = readerId,
				RequestDate = DateTime.Now,
				Status = "Pending"
			};

			await _uow.BorrowRequestRepository.AddAsync(request);
			await _uow.SaveChangesAsync(); // Save để có RequestId

			foreach (var detail in dto.Details)
			{
				var requestDetail = new BorrowRequestDetail
				{
					RequestId = request.RequestId,
					WorkId = detail.WorkId,
					RequestedQuantity = detail.RequestedQuantity
				};

				// Không dùng _context trực tiếp, tạo entity rồi SaveChanges sẽ attach tự động
				_uow.DbContext.BorrowRequestDetails.Add(requestDetail); // Nếu IUnitOfWork có DbContext, hoặc bỏ dòng này và SaveChanges sau
			}

			await _uow.SaveChangesAsync();

			return await MapToBorrowRequestDto(request);
		}

		public async Task<IEnumerable<BorrowRequestDto>> GetPendingRequestsAsync()
		{
			var requests = await _uow.BorrowRequestRepository.GetPendingRequestsAsync();
			return await Task.WhenAll(requests.Select(async r => await MapToBorrowRequestDto(r)));
		}

		public async Task ApproveBorrowRequestAsync(int requestId, int employeeId, ApproveBorrowRequestDto dto)
		{
			var request = await _uow.BorrowRequestRepository.GetRequestWithDetailsAsync(requestId);
			if (request == null) throw new KeyNotFoundException("Không tìm thấy request");

			if (request.Status != "Pending") throw new InvalidOperationException("Request không ở trạng thái Pending");

			foreach (var approved in dto.ApprovedDetails)
			{
				var detail = request.Details.FirstOrDefault(d => d.RequestDetailId == approved.RequestDetailId);
				if (detail != null)
					detail.ApprovedQuantity = approved.ApprovedQuantity;
			}

			request.ApprovedByEmployeeId = employeeId;
			request.ApprovedDate = DateTime.Now;
			request.Status = "Approved";

			await _uow.SaveChangesAsync();
		}

		public async Task RejectBorrowRequestAsync(int requestId, string reason)
		{
			var request = await _uow.BorrowRequestRepository.GetByIdAsync(requestId);
			if (request == null) throw new KeyNotFoundException("Không tìm thấy request");

			if (request.Status != "Pending") throw new InvalidOperationException("Request không ở trạng thái Pending");

			request.Status = "Rejected";
			request.RejectReason = reason;

			await _uow.SaveChangesAsync();
		}

		public async Task<BorrowTransactionDto> CreateBorrowTransactionAsync(int requestId, int employeeId, List<int> copyIds)
		{
			var request = await _uow.BorrowRequestRepository.GetRequestWithDetailsAsync(requestId);
			if (request == null) throw new KeyNotFoundException("Không tìm thấy request");

			if (request.Status != "Approved") throw new InvalidOperationException("Request chưa được duyệt");

			var transaction = new BorrowTransaction
			{
				ReaderId = request.ReaderId,
				EmployeeId = employeeId,
				RequestId = requestId,
				BorrowDate = DateTime.Now,
				Status = "Borrowed"
			};

			await _uow.BorrowTransactionRepository.AddAsync(transaction);
			await _uow.SaveChangesAsync(); // Save để có BorrowId

			foreach (var copyId in copyIds)
			{
				var detail = new BorrowTransactionDetail
				{
					BorrowId = transaction.BorrowId,
					CopyId = copyId,
					DueDate = DateTime.Now.AddDays(14),
					ItemStatus = "Borrowed"
				};

				// Không dùng _context trực tiếp, tạo entity rồi SaveChanges sẽ attach tự động
				_uow.DbContext.BorrowTransactionDetails.Add(detail);
			}

			await _uow.SaveChangesAsync();

			return await MapToBorrowTransactionDto(transaction);
		}

		public async Task ReturnBookAsync(int borrowDetailId, ReturnBookDto dto)
		{
			var detail = await _uow.DbContext.BorrowTransactionDetails.FindAsync(borrowDetailId);
			if (detail == null) throw new KeyNotFoundException("Không tìm thấy chi tiết giao dịch");

			if (detail.ReturnDate != null) throw new InvalidOperationException("Sách đã được trả");

			detail.ReturnDate = dto.ReturnDate ?? DateTime.Now;
			detail.ItemStatus = dto.ItemStatus;
			detail.FineAmount = dto.FineAmount;
			detail.ConditionNote = dto.ConditionNote;

			await _uow.SaveChangesAsync();
		}

		public async Task<IEnumerable<BorrowTransactionDto>> GetReaderBorrowHistoryAsync(int readerId)
		{
			var transactions = await _uow.BorrowTransactionRepository.GetByReaderIdAsync(readerId);
			return await Task.WhenAll(transactions.Select(async t => await MapToBorrowTransactionDto(t)));
		}

		public async Task<IEnumerable<BorrowTransactionDto>> GetOverdueTransactionsAsync()
		{
			var transactions = await _uow.BorrowTransactionRepository.GetOverdueTransactionsAsync();
			return await Task.WhenAll(transactions.Select(async t => await MapToBorrowTransactionDto(t)));
		}

		// Helper để map DTO
		private async Task<BorrowRequestDto> MapToBorrowRequestDto(BorrowRequest request)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(request.ReaderId);
			var approvedEmployee = request.ApprovedByEmployeeId.HasValue
				? await _uow.EmployeeRepository.GetByIdAsync(request.ApprovedByEmployeeId.Value)
				: null;

			var detailDtos = await Task.WhenAll(request.Details.Select(async d => new BorrowRequestDetailDto
			{
				WorkId = d.WorkId,
				Title = (await _uow.BookWorkRepository.GetByIdAsync(d.WorkId))?.Title ?? "Unknown",
				RequestedQuantity = d.RequestedQuantity,
				ApprovedQuantity = d.ApprovedQuantity
			}));

			return new BorrowRequestDto
			{
				RequestId = request.RequestId,
				ReaderId = request.ReaderId,
				ReaderFullName = reader?.FullName ?? "Unknown",
				RequestDate = request.RequestDate,
				Status = request.Status,
				ApprovedByEmployeeId = request.ApprovedByEmployeeId,
				ApprovedByEmployeeName = approvedEmployee?.FullName,
				ApprovedDate = request.ApprovedDate,
				RejectReason = request.RejectReason,
				Details = detailDtos.ToList()
			};
		}

		private async Task<BorrowTransactionDto> MapToBorrowTransactionDto(BorrowTransaction transaction)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(transaction.ReaderId);
			var employee = await _uow.EmployeeRepository.GetByIdAsync(transaction.EmployeeId);

			var detailDtos = await Task.WhenAll(transaction.Details.Select(async d => new BorrowTransactionDetailDto
			{
				BorrowDetailId = d.BorrowDetailId,
				CopyId = d.CopyId,
				Title = (await _uow.BookCopyRepository.GetByIdAsync(d.CopyId))?.BookEdition?.BookWork?.Title ?? "Unknown",
				DueDate = d.DueDate,
				ReturnDate = d.ReturnDate,
				ItemStatus = d.ItemStatus,
				FineAmount = d.FineAmount,
				ConditionNote = d.ConditionNote
			}));

			return new BorrowTransactionDto
			{
				BorrowId = transaction.BorrowId,
				ReaderId = transaction.ReaderId,
				ReaderFullName = reader?.FullName ?? "Unknown",
				EmployeeId = transaction.EmployeeId,
				EmployeeFullName = employee?.FullName ?? "Unknown",
				RequestId = transaction.RequestId,
				BorrowDate = transaction.BorrowDate,
				Status = transaction.Status,
				Details = detailDtos.ToList()
			};
		}
	}
}