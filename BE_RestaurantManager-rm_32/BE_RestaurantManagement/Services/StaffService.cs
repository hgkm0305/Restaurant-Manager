using BE_RestaurantManagement.Data;
using BE_RestaurantManagement.DTOs;
using BE_RestaurantManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using BE_RestaurantManagement.Interfaces;

namespace BE_RestaurantManagement.Services
{
    public class StaffService : IStaffService
    {
        private readonly RestaurantDbContext _context;

        public StaffService(RestaurantDbContext context)
        {
            _context = context;
        }

        //DONE
        public async Task<IEnumerable<StaffDTO>> GetAllStaffAsync()
        {
            // get list from table Staffs
            var staffList = await _context.Staffs
                .Select(s => new StaffDTO
                {
                    UserId = s.UserId,
                    FullName = s.FullName,
                    Email = s.Email,
                    RoleId = s.RoleId
                })
                .ToListAsync();

            // get list from table KitchenStaffs
            var kitchenStaffList = await _context.KitchenStaffs
                .Select(s => new StaffDTO
                {
                    UserId = s.UserId,
                    FullName = s.FullName,
                    Email = s.Email,
                    RoleId = s.RoleId
                })
                .ToListAsync();

            // Combine list
            return staffList.Concat(kitchenStaffList);
        }

        //DONE
        public async Task<StaffDTO> GetStaffByIdAsync(int id)
        {
            // Find in table Staffs
            var staff = await _context.Staffs.FindAsync(id);
            if (staff != null)
            {
                return new StaffDTO
                {
                    UserId = staff.UserId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    RoleId = staff.RoleId
                };
            }

            // Find in table KitchenStaffs
            var kitchenStaff = await _context.KitchenStaffs.FindAsync(id);
            if (kitchenStaff != null)
            {
                return new StaffDTO
                {
                    UserId = kitchenStaff.UserId,
                    FullName = kitchenStaff.FullName,
                    Email = kitchenStaff.Email,
                    RoleId = kitchenStaff.RoleId
                };
            }

            return null; // Not found
        }

        // DONE
        public async Task<StaffDTO> CreateStaffAsync(CreateStaffDTO staffDto)
        {
            // Kiểm tra xem email đã tồn tại chưa
            var existingStaff = await _context.Users
                .FirstOrDefaultAsync(s => s.Email == staffDto.Email);

            if (existingStaff != null)
            {
                throw new Exception("Staff's email already exists in database"); // Báo lỗi nếu nhân viên đã tồn tại
            }

            User newUser;

            if (staffDto.RoleId == 4)
            {
                newUser = new Staff
                {
                    FullName = staffDto.FullName,
                    Email = staffDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(staffDto.Password),
                    RoleId = staffDto.RoleId
                };
                _context.Staffs.Add((Staff)newUser);
            }
            else if (staffDto.RoleId == 6)
            {
                newUser = new KitchenStaff
                {
                    FullName = staffDto.FullName,
                    Email = staffDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(staffDto.Password),
                    RoleId = staffDto.RoleId
                };
                _context.KitchenStaffs.Add((KitchenStaff)newUser);
            }
            else
            {
                throw new Exception("Wrong roleId");
            }

            await _context.SaveChangesAsync();

            return new StaffDTO
            {
                UserId = newUser.UserId,
                FullName = newUser.FullName,
                Email = newUser.Email,
                RoleId = newUser.RoleId
            };
        }

        //DONE
        public async Task<StaffDTO> UpdateStaffAsync(int id, CreateStaffDTO staffDto)
        {
            var existingStaff = await _context.Users.FindAsync(id);
            if (existingStaff == null) return null;

            // Check duplicated email
            var duplicateEmail = await _context.Users
                .FirstOrDefaultAsync(s => s.Email == staffDto.Email && s.UserId != id);

            if (duplicateEmail != null)
            {
                throw new Exception("Email already exists in database");
            }

            // Check changes
            if (existingStaff.FullName == staffDto.FullName &&
                existingStaff.RoleId == staffDto.RoleId &&
                existingStaff.Email == staffDto.Email &&
                BCrypt.Net.BCrypt.Verify(staffDto.Password, existingStaff.Password))
            {
                return null; // Return null when no change
            }

            // If the roleID changes, it is necessary to delete the staff from the old board and add
            // the new table
            if (existingStaff.RoleId != staffDto.RoleId)
            {
                if (existingStaff.RoleId == 4) // If Staff, delete record from Staffs table
                {
                    var staff = await _context.Staffs.FindAsync(id);
                    if (staff != null) _context.Staffs.Remove(staff);
                }
                else if (existingStaff.RoleId == 6) // If kitchenStaff, delete record from kitchenStaffs table
                {
                    var kitchenStaff = await _context.KitchenStaffs.FindAsync(id);
                    if (kitchenStaff != null) _context.KitchenStaffs.Remove(kitchenStaff);
                }

                // Save to new table
                if (staffDto.RoleId == 4)
                {
                    var newStaff = new Staff
                    {
                        UserId = existingStaff.UserId, // Keep ID
                        FullName = staffDto.FullName,
                        Email = staffDto.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(staffDto.Password),
                        RoleId = staffDto.RoleId
                    };
                    _context.Staffs.Add(newStaff);
                }
                else if (staffDto.RoleId == 6)
                {
                    var newKitchenStaff = new KitchenStaff
                    {
                        UserId = existingStaff.UserId,
                        FullName = staffDto.FullName,
                        Email = staffDto.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(staffDto.Password),
                        RoleId = staffDto.RoleId
                    };
                    _context.KitchenStaffs.Add(newKitchenStaff);
                }
                else
                {
                    throw new Exception("Wrong roleId");
                }
            }
            else // If Roleid does not change, just update the information
            {
                existingStaff.FullName = staffDto.FullName;
                existingStaff.Email = staffDto.Email;
                existingStaff.Password = BCrypt.Net.BCrypt.HashPassword(staffDto.Password);
                existingStaff.RoleId = staffDto.RoleId;

                _context.Users.Update(existingStaff);
            }

            await _context.SaveChangesAsync();

            return new StaffDTO
            {
                UserId = existingStaff.UserId,
                FullName = staffDto.FullName,
                Email = staffDto.Email,
                RoleId = staffDto.RoleId
            };
        }

        //DONE
        public async Task<bool> DeleteStaffAsync(int id)
        {
            var staff = await _context.Users.FindAsync(id);
            if (staff == null) return false;

            // Check the roleid and delete from the corresponding table
            if (staff.RoleId == 4) // If Staff
            {
                var staffRecord = await _context.Staffs.FindAsync(id);
                if (staffRecord != null) _context.Staffs.Remove(staffRecord);
            }
            else if (staff.RoleId == 6) // If KitchenStaff
            {
                var kitchenStaffRecord = await _context.KitchenStaffs.FindAsync(id);
                if (kitchenStaffRecord != null) _context.KitchenStaffs.Remove(kitchenStaffRecord);
            }
            else
            {
                throw new Exception("Invalid RoleId, cannot delete staff");
            }

            //Delete record from Users
            _context.Users.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
