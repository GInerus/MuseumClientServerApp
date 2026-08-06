using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public enum ChangePasswordResult
    {
        Success,
        OldPasswordInvalid,
        NewPasswordEmpty,
        NewPasswordTooShort
    }

    public class MuseumInfoService
    {
        private readonly MuseumContext _context;

        public MuseumInfoService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<string> GetDescriptionAsync()
        {
            var info = await _context.MuseumInfo.FirstOrDefaultAsync();
            return info?.Description ?? string.Empty;
        }

        public async Task<string> UpdateDescriptionAsync(string description)
        {
            var info = await _context.MuseumInfo.FirstOrDefaultAsync();

            if (info == null)
            {
                // на случай, если строку в БД забыли создать вручную
                info = new MuseumInfo { Description = description };
                _context.MuseumInfo.Add(info);
            }
            else
            {
                info.Description = description;
            }

            await _context.SaveChangesAsync();

            return info.Description ?? string.Empty;
        }

        public async Task<ChangePasswordResult> ChangeAdminPasswordAsync( string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return ChangePasswordResult.NewPasswordEmpty;


            if (newPassword.Trim().Length < 6)
                return ChangePasswordResult.NewPasswordTooShort;


            var info = await _context.MuseumInfo.FirstOrDefaultAsync();

            if (info == null)
                return ChangePasswordResult.OldPasswordInvalid;


            if (!BCrypt.Net.BCrypt.Verify(
                oldPassword,
                info.AdminPasswordHash))
            {
                return ChangePasswordResult.OldPasswordInvalid;
            }


            info.AdminPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);


            await _context.SaveChangesAsync();


            return ChangePasswordResult.Success;
        }
    }
}