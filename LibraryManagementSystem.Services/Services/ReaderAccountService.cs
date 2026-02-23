using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	public class ReaderAccountService : IReaderAccountService
	{
		private readonly IUnitOfWork _uow;

		public ReaderAccountService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<ReaderDto> GetReaderByIdAsync(int readerId)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(readerId);
			if (reader == null) throw new KeyNotFoundException("Không tìm thấy độc giả");

			return new ReaderDto
			{
				ReaderId = reader.ReaderId,
				CardNumber = reader.CardNumber,
				Email = reader.Email,
				FullName = reader.FullName,
				PhoneNumber = reader.PhoneNumber,
				Address = reader.Address,
				RegisterDate = reader.RegisterDate,
				ExpiredDate = reader.ExpiredDate,
				ReaderStatus = reader.ReaderStatus
			};
		}

		public async Task<IEnumerable<ReaderDto>> GetAllReadersAsync()
		{
			var readers = await _uow.ReaderRepository.GetAllAsync();
			return readers.Select(r => new ReaderDto
			{
				ReaderId = r.ReaderId,
				CardNumber = r.CardNumber,
				Email = r.Email,
				FullName = r.FullName,
				PhoneNumber = r.PhoneNumber,
				Address = r.Address,
				RegisterDate = r.RegisterDate,
				ExpiredDate = r.ExpiredDate,
				ReaderStatus = r.ReaderStatus
			});
		}

		public async Task UpdateReaderAsync(int readerId, UpdateReaderDto dto)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(readerId);
			if (reader == null) throw new KeyNotFoundException("Không tìm thấy độc giả");

			if (dto.FullName != null) reader.FullName = dto.FullName;
			if (dto.PhoneNumber != null) reader.PhoneNumber = dto.PhoneNumber;
			if (dto.Address != null) reader.Address = dto.Address;
			if (dto.ReaderStatus != null) reader.ReaderStatus = dto.ReaderStatus;
			if (dto.ExpiredDate != null) reader.ExpiredDate = dto.ExpiredDate.Value;

			await _uow.ReaderRepository.UpdateAsync(reader);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteReaderAsync(int readerId)
		{
			var reader = await _uow.ReaderRepository.GetByIdAsync(readerId);
			if (reader == null) throw new KeyNotFoundException("Không tìm thấy độc giả");

			await _uow.ReaderRepository.DeleteAsync(reader);
			await _uow.SaveChangesAsync();
		}

		public async Task<ReaderDto> CreateReaderAsync(CreateReaderDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
				throw new ArgumentException("Email, Password và FullName không được để trống");

			if (await _uow.ReaderRepository.GetByEmailAsync(dto.Email) != null)
				throw new ArgumentException("Email đã tồn tại");

			var reader = new Reader
			{
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				FullName = dto.FullName,
				PhoneNumber = dto.PhoneNumber,
				Address = dto.Address,
				RegisterDate = DateTime.Now,
				ExpiredDate = dto.ExpiredDate ?? DateTime.Now.AddYears(2),
				ReaderStatus = "Active",
				CardNumber = "RD" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
			};

			await _uow.ReaderRepository.AddAsync(reader);
			await _uow.SaveChangesAsync();

			return new ReaderDto
			{
				ReaderId = reader.ReaderId,
				CardNumber = reader.CardNumber,
				Email = reader.Email,
				FullName = reader.FullName,
				PhoneNumber = reader.PhoneNumber,
				Address = reader.Address,
				RegisterDate = reader.RegisterDate,
				ExpiredDate = reader.ExpiredDate,
				ReaderStatus = reader.ReaderStatus
			};
		}
	}
}
