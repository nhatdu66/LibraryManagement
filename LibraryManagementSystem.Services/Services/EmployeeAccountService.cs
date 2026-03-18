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
	public class EmployeeAccountService : IEmployeeAccountService
	{
		private readonly IUnitOfWork _uow;

		public EmployeeAccountService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<EmployeeDto> GetEmployeeByIdAsync(int employeeId)
		{
			var employee = await _uow.EmployeeRepository.GetByIdAsync(employeeId);
			if (employee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");

			return new EmployeeDto
			{
				EmployeeId = employee.EmployeeId,
				Email = employee.Email,
				FullName = employee.FullName,
				RoleName = (await _uow.RoleRepository.GetByIdAsync(employee.RoleId))?.RoleName ?? "Unknown",
				HireDate = employee.HireDate,
				Status = employee.Status
			};
		}

		public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
		{
			var employees = await _uow.EmployeeRepository.GetAllAsync();
			var employeeDtos = await Task.WhenAll(employees.Select(async e => new EmployeeDto
			{
				EmployeeId = e.EmployeeId,
				Email = e.Email,
				FullName = e.FullName,
				RoleName = (await _uow.RoleRepository.GetByIdAsync(e.RoleId))?.RoleName ?? "Unknown",
				HireDate = e.HireDate,
				Status = e.Status
			}));

			return employeeDtos;
		}

		public async Task UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto)
		{
			var employee = await _uow.EmployeeRepository.GetByIdAsync(employeeId);
			if (employee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");

			if (dto.FullName != null) employee.FullName = dto.FullName;
			if (dto.Status != null) employee.Status = dto.Status;
			if (dto.RoleId != null) employee.RoleId = dto.RoleId.Value;

			await _uow.EmployeeRepository.UpdateAsync(employee);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteEmployeeAsync(int employeeId)
		{
			var employee = await _uow.EmployeeRepository.GetByIdAsync(employeeId);
			if (employee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");

			await _uow.EmployeeRepository.DeleteAsync(employee);
			await _uow.SaveChangesAsync();
		}

		public async Task ChangeEmployeeRoleAsync(int employeeId, int newRoleId)
		{
			var employee = await _uow.EmployeeRepository.GetByIdAsync(employeeId);
			if (employee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");

			var role = await _uow.RoleRepository.GetByIdAsync(newRoleId);
			if (role == null) throw new KeyNotFoundException("Không tìm thấy vai trò");

			employee.RoleId = newRoleId;
			await _uow.EmployeeRepository.UpdateAsync(employee);
			await _uow.SaveChangesAsync();
		}
	}
}
